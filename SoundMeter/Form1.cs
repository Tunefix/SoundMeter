using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SoundMeter
{
	public partial class Form1 : Form
	{
		WasapiLoopbackCapture _waveIn;
		BufferedWaveProvider buffer;
		Dictionary<int, MMDevice> outputs = new Dictionary<int, MMDevice>();

		ComboBox outputSelect;
		Goniometer goniometer;
		QPPM qppm;
		Correlation correlation;
		Label lblDebug;

		List<byte> sampleStore = new List<byte>();
		WaveFormat inputFormat;
		int channels;
		int sampleRate;
		int bitsPrSample;

		int loopTime = 5; // ms

		List<byte> loopBytes = new List<byte>();
		List<short> loopSamplesL = new List<short>();
		List<short> loopSamplesR = new List<short>();

		System.Windows.Forms.Timer mainLoop;

		Stopwatch watch = new Stopwatch();
		int runtime = 0;

		int sleepTime = 0;

		int bitsPrSec;
		int bytesPrSec;

		int bytesPrLoop;

		int samplesPrLoop;

		int bytesPrSample;

		byte[] tmpBytes;
		short bit16;
		float bit32;
		int bitOther;

		int bytesToProcess;

		static private List<PrivateFontCollection> _fontCollections;
		Font font;


		public Form1()
		{

			InitializeComponent();
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Text = "SoundMeter";
			this.BackColor = Color.FromArgb(255, 0, 32, 64);
			this.ClientSize = new Size(515, 475);

			getFonts();

			makeLayout();

			initAudio();
			/*Task metering = new Task(runMeter);
			metering.Start();*/

			mainLoop = new System.Windows.Forms.Timer();
			mainLoop.Interval = loopTime;
			mainLoop.Tick += runMeters;
			mainLoop.Start();
		}

		void getFonts()
		{
			font = GetCustomFont(GetBytesFromFile(AppDomain.CurrentDomain.BaseDirectory + "Abel.ttf"), 10, FontStyle.Bold);
		}

		void initAudio(object sender, EventArgs e)
		{
			initAudio();
		}

		void initAudio()
		{ 
			_waveIn = new WasapiLoopbackCapture(outputs[outputSelect.SelectedIndex]);
			_waveIn.DataAvailable += OnDataAvailable;
			_waveIn.ShareMode = AudioClientShareMode.Shared;
			_waveIn.StartRecording();
			inputFormat = _waveIn.WaveFormat;
			channels = inputFormat.Channels;
			sampleRate = inputFormat.SampleRate;
			bitsPrSample = inputFormat.BitsPerSample;

			bytesPrSample = bitsPrSample / 8;

			buffer = new BufferedWaveProvider(_waveIn.WaveFormat);
		}

		void OnDataAvailable(object sender, WaveInEventArgs e)
		{
			byte[] samples = new byte[e.BytesRecorded];
			Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);
			sampleStore.AddRange(samples);
		}

		

		void runMeters(object sender, EventArgs e)
		{
			bytesToProcess = sampleStore.Count;
			loopSamplesL.Clear();
			loopSamplesR.Clear();

			if (bytesToProcess > 0)
			{
				//Console.WriteLine("BUFFER SIZE: " + sampleStore.Count);

				// Decrease number of bytes to match with LR-samples
				while (bytesToProcess % (bytesPrSample * channels) != 0)
				{
					bytesToProcess--;
				}

				// Move bytes to loopBytes
				loopBytes.Clear();
				loopBytes.AddRange(sampleStore.GetRange(0, bytesToProcess));
				sampleStore.RemoveRange(0, bytesToProcess);


				// CONVERT LOOPBYTES TO LOOPSAMPLES
				tmpBytes = loopBytes.ToArray();
				

				for (int i = 0; i < tmpBytes.Length - (bytesPrSample * channels); i += (bytesPrSample * channels))
				{
					switch (bitsPrSample)
					{
						case 16:
							bit16 = BitConverter.ToInt16(tmpBytes, i);
							loopSamplesL.Add(bit16);
							if (channels >= 2)
							{
								bit16 = BitConverter.ToInt16(tmpBytes, i + bytesPrSample);
								loopSamplesR.Add(bit16);
							}
							break;
						case 32:
							bit32 = BitConverter.ToSingle(tmpBytes, i);
							loopSamplesL.Add((short)(bit32 * Int16.MaxValue));
							if (channels >= 2)
							{
								bit32 = BitConverter.ToSingle(tmpBytes, i + bytesPrSample);
								loopSamplesR.Add((short)(bit32 * Int16.MaxValue));
							}
							break;
					}
				}


				
			}

			// DO STUFF WITH LOOP SAMPLES
			qppm.AddSamples(loopSamplesL, loopSamplesR);
			goniometer.AddSamples(loopSamplesL, loopSamplesR);
			correlation.AddSamples(loopSamplesL, loopSamplesR);
		}

		

		void makeLayout()
		{
			// OUTPUT SELECT
			outputSelect = new ComboBox();
			outputSelect.Location = new Point(5, 5);
			outputSelect.Size = new Size(300, 20);
			outputSelect.SelectedIndexChanged += initAudio;
			this.Controls.Add(outputSelect);

			// GONEOMETER
			goniometer = new Goniometer();
			goniometer.Location = new Point(5, 30);
			goniometer.Size = new Size(400, 400);
			goniometer.Font = font;
			this.Controls.Add(goniometer);

			// QPPM
			qppm = new QPPM();
			qppm.Location = new Point(410, 30);
			qppm.Size = new Size(100, 400);
			qppm.Font = font;
			this.Controls.Add(qppm);

			// CORRELATION METER
			correlation = new Correlation();
			correlation.Location = new Point(5, 435);
			correlation.Size = new Size(505, 35);
			correlation.Font = font;
			this.Controls.Add(correlation);

			// DEBUG LABEL
			lblDebug = new Label();
			lblDebug.Location = new Point(10, 40);
			lblDebug.Size = new Size(200, 20);
			lblDebug.Font = font;
			this.Controls.Add(lblDebug);


			// GET AUDIO OUTPUTS
			if (outputs.Count > 0)
			{
				outputs.Clear();
			}

			MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
			int i = 0;
			foreach (MMDevice wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
			{
				Console.WriteLine($"{wasapi.ID} {wasapi.DataFlow} {wasapi.FriendlyName} {wasapi.DeviceFriendlyName} {wasapi.State}");
				outputs.Add(i, wasapi);
				outputSelect.Items.Add(wasapi.FriendlyName);
				i++;
			}

			if (outputs.Count > 0)
			{
				outputSelect.SelectedIndex = 0;
			}
		}

		

		static public Font GetCustomFont(byte[] fontData, float size, FontStyle style)
		{
			if (_fontCollections == null) _fontCollections = new List<PrivateFontCollection>();
			PrivateFontCollection fontCol = new PrivateFontCollection();
			IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);

			Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
			fontCol.AddMemoryFont(fontPtr, fontData.Length);
			Marshal.FreeCoTaskMem(fontPtr); //<-- It works!
			_fontCollections.Add(fontCol);
			return new Font(fontCol.Families[0], size, style);
		}

		public static byte[] GetBytesFromFile(string fullFilePath)
		{
			// this method is limited to 2^32 byte files (4.2 GB)

			FileStream fs = File.OpenRead(fullFilePath);
			try
			{
				byte[] bytes = new byte[fs.Length];
				fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
				fs.Close();
				return bytes;
			}
			finally
			{
				fs.Close();
			}

		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			mainLoop.Stop();
			_waveIn.StopRecording();
			Application.Exit();
			Environment.Exit(0);
		}
	}
}
