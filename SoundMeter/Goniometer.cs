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
		float meterSize;
		float x;
		float y;
		float prevX;
		float prevY;
		List<PointF> tmpLine = new List<PointF>();

		float sin45 = 0.7071067812f;
		float cos45 = 0.7071067812f;
		float sqr2 = 1.414213562f;
		float normR;
		float normL;
		float xL;
		float xR;
		float yL;
		float yR;
		float xNorm;
		float yNorm;
		public float amp = 8;
		float ampMax = 8;
		float ampStep = 0.1f;
		float maxL;
		float maxR;
		DateTime lastAmpDown;
		float ampStickTime = 750; // ms

		public Goniometer()
		{
			this.DoubleBuffered = true;
			penCross.DashPattern = new float[]{ 2, 2 };

			centerAlign = new StringFormat();
			centerAlign.Alignment = StringAlignment.Center;
			centerAlign.LineAlignment = StringAlignment.Center;


			lastAmpDown = DateTime.Now;
		}

		public void AddSamples(List<short> L, List<short> R)
		{
			if (L.Count > 4)
			{
				samplesL.Clear();
				samplesR.Clear();

				samplesL.AddRange(L);
				samplesR.AddRange(R);

				// TRIM TO A NOT-CRAZY AMOUNT
				if(samplesL.Count > 4096)
				{
					samplesL.RemoveRange(4096, samplesL.Count - 4096);
					samplesR.RemoveRange(4096, samplesR.Count - 4096);
				}

			
				while (samplesL.Count % 3 != 1)
				{
					samplesL.RemoveAt(0);
					samplesR.RemoveAt(0);
				}

				this.Invalidate();
			}
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

			maxL = 0;
			maxR = 0;

			for (int i = 0; i < samplesL.Count; i++)
			{
				normL = (samplesL[i] / (float)Int16.MaxValue) * amp;
				xL = normL * cos45;
				yL = normL * sin45;

				normR = (samplesR[i] / (float)Int16.MaxValue) * amp;
				xR = normR * cos45;
				yR = normR * sin45;

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

				if (normL > maxL) maxL = normL;
				if (normR > maxR) maxR = normR;
			}

			// AMP IF NECESSARY
			if(maxL > 1.0 || maxR > 1.0)
			{
				float max = maxL > maxR ? maxL : maxR;
				amp -= max - 1.0f;
				lastAmpDown = DateTime.Now;
			}
			else if(amp < ampMax && maxL < 0.9 && maxR < 0.9)
			{
				if ((DateTime.Now - lastAmpDown).TotalMilliseconds > ampStickTime)
				{
					amp += ampStep;
					// CLAMP
					if (amp > ampMax) amp = ampMax;
				}
			}



			if (tmpLine.Count > 3)
			{
				g.DrawBeziers(penSamples, tmpLine.ToArray());
			}
		}

		private double deg2rad(float deg)
		{
			return (deg * (Math.PI / 180.0));
		}
	}
}
