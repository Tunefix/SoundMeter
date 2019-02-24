using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundMeter
{
	class Goniometer : Control
	{
		Brush brushFont = new SolidBrush(Color.FromArgb(255, 255, 204, 0));
		Brush brushBackground = new SolidBrush(Color.FromArgb(255, 16, 16, 16));
		Brush brushSampleDot = new SolidBrush(Color.FromArgb(164, 0, 255, 64));

		Pen penBorder = new Pen(Color.FromArgb(255, 200, 200, 200), 1f);
		Pen penCross = new Pen(Color.FromArgb(255, 128, 0, 128), 1f);
		Pen penSamples = new Pen(Color.FromArgb(164, 0, 255, 64), 1f);

		StringFormat centerAlign;

		List<short> samplesL = new List<short>();
		List<short> samplesR = new List<short>();
		int samplesToDisplay = 1024;
		float meterSize;
		float logValueX;
		float logValueY;
		float x;
		float y;
		float prevX;
		float prevY;
		bool drawLine;
		List<PointF> tmpLine = new List<PointF>();

		float sin45 = 0.7071067812f;
		float cos45 = 0.7071067812f;
		float sqr2 = 1.414213562f;
		float xL;
		float xR;
		float yL;
		float yR;
		float xNorm;
		float yNorm;
		public float amp = 8;
		float ampMax = 8;
		float ampStep = 0.1f;
		float xMax;
		float yMax;
		DateTime lastAmpDown;
		float ampStickTime = 750; // ms

		public Goniometer()
		{
			this.DoubleBuffered = true;
			penCross.DashPattern = new float[]{ 2, 2 };

			centerAlign = new StringFormat();
			centerAlign.Alignment = StringAlignment.Center;
			centerAlign.LineAlignment = StringAlignment.Center;

			// PREFILL SAMPLES WITH 0s
			for(int i = 0; i < samplesToDisplay; i++)
			{
				samplesL.Add(0);
				samplesR.Add(0);
			}

			lastAmpDown = DateTime.Now;
		}

		public void AddSamples(List<short> L, List<short> R)
		{
			samplesL.Clear();
			samplesR.Clear();

			samplesL.AddRange(L);
			samplesR.AddRange(R);

			//if (samplesL.Count > samplesToDisplay) samplesL.RemoveRange(0, samplesL.Count - samplesToDisplay);
			//if (samplesR.Count > samplesToDisplay) samplesR.RemoveRange(0, samplesR.Count - samplesToDisplay);

			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.None;
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			// BACKGROUND
			g.FillRectangle(brushBackground, 0f, 0f, Width, Height);

			// BORDER
			g.DrawRectangle(penBorder, 0f, 0f, Width - 1f, Height - 1f);

			// CIRCLE
			g.DrawEllipse(penCross, 30f, 30f, Width - 60f, Height - 60f);

			// CROSS
			g.DrawLine(penCross, Width / 2f, 30f, Width / 2f, Height - 30f);
			g.DrawLine(penCross, 30f, Height / 2f, Width - 30f, Height / 2f);

			// LABELS
			g.DrawString("MONO", Font, brushFont, Width / 2f, 15f, centerAlign);
			g.DrawString("L", Font, brushFont, Width / 6f, Height / 6f, centerAlign);
			g.DrawString("R", Font, brushFont, Width * (5f/6f), Height / 6f, centerAlign);
			g.DrawString("+S", Font, brushFont, 15f, Height / 2f, centerAlign);
			g.DrawString("-S", Font, brushFont, Width - 15f, Height / 2f, centerAlign);

			// DRAW SAMPLES
			meterSize = (Height - 60f) / 2f;
			tmpLine.Clear();

			xMax = 0;
			yMax = 0;

			for (int i = 0; i < samplesToDisplay; i++)
			{
				xL = (samplesL[i] / (float)Int16.MaxValue) * cos45 * amp;
				yL = (samplesL[i] / (float)Int16.MaxValue) * sin45 * amp;

				xR = (samplesR[i] / (float)Int16.MaxValue) * cos45 * amp;
				yR = (samplesR[i] / (float)Int16.MaxValue) * sin45 * amp;

				xL = xL * cos45;
				yL = yL * sin45;

				xR = xR * cos45;
				yR = yR * sin45;

				xNorm = (xR - xL) / sqr2;
				yNorm = (yR + yL) / sqr2;

				x = (Width / 2f) + (xNorm * meterSize);
				y = (Height / 2f) - (yNorm * meterSize);

				tmpLine.Add(new PointF(x, y));

				prevX = x;
				prevY = y;

				if (xNorm > xMax) xMax = xNorm;
				if (yNorm > yMax) yMax = yNorm;
			}

			// AMP IF NECESSARY
			if(xMax > 1.0 || yMax > 1.0)
			{
				float max = xMax > yMax ? xMax : yMax;
				amp -= max - 1.0f;
				lastAmpDown = DateTime.Now;
			}
			else if(amp < ampMax && xMax < 0.9 && yMax < 0.9)
			{
				if ((DateTime.Now - lastAmpDown).TotalMilliseconds > ampStickTime)
				{
					amp += ampStep;
					// CLAMP (just in case)
					if (amp > ampMax) amp = ampMax;
				}
			}

			g.DrawBeziers(penSamples, tmpLine.ToArray());
		}

		private double deg2rad(float deg)
		{
			return (deg * (Math.PI / 180.0));
		}
	}
}
