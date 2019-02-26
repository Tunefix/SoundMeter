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
	class Correlation : Control
	{
		Brush brushBackground = new SolidBrush(Color.FromArgb(255, 16, 16, 16));
		Brush boxBrush = new SolidBrush(Color.FromArgb(255, 0, 255, 64));
		Brush brushFont = new SolidBrush(Color.FromArgb(255, 255, 204, 0));

		Pen penBorder = new Pen(Color.FromArgb(255, 200, 200, 200), 1f);
		Pen innerBorder = new Pen(Color.FromArgb(128, 200, 200, 200), 1f);
		Pen penLineWhiteDotted = new Pen(Color.FromArgb(255, 128, 128, 128), 1f);

		List<short> samplesL = new List<short>();
		List<short> samplesR = new List<short>();

		double phase;
		double magnitude;
		double correlation;
		double runningTotal;
		double avg;

		double maxChange = 0.05;
		double value;
		double prevAvg;

		float boxWidth = 15f;
		float boxHeight = 15f;
		float y;
		float x;

		StringFormat centerAlign;

		public Correlation()
		{
			this.DoubleBuffered = true;

			centerAlign = new StringFormat();
			centerAlign.Alignment = StringAlignment.Center;
			centerAlign.LineAlignment = StringAlignment.Far;

			penLineWhiteDotted.DashPattern = new float[] { 1, 1 };
		}

		public void AddSamples(List<short> L, List<short> R)
		{
			samplesL.Clear();
			samplesR.Clear();

			samplesL.AddRange(L);
			samplesR.AddRange(R);

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

			// INNER BORDER
			g.DrawRectangle(innerBorder, 2f, Height - 21f, Width - 5f, 18f);

			// SCALE
			g.DrawString("-1", Font, brushFont, (boxWidth / 2f) + 3f, Height - 16f, centerAlign);
			g.DrawString("+1", Font, brushFont, -(boxWidth / 2f) + Width - 6f, Height - 16f, centerAlign);
			g.DrawString("0", Font, brushFont, Width / 2f, Height - 16f, centerAlign);

			

			runningTotal = 0;
			for(int i = 0; i < samplesL.Count; i++)
			{
				
				phase = -rad2deg(Math.Atan2(samplesL[i], samplesR[i]));
				magnitude = Math.Sqrt((samplesL[i] * samplesL[i]) + (samplesR[i] * samplesR[i]));
				correlation = (phase / 45.0);

				runningTotal += correlation;
			}

			avg = runningTotal / samplesL.Count;
			if (avg < -1) avg = -2.0 - avg;
			if (avg > 1) avg = 2.0 - avg;

			if (Math.Abs(avg - prevAvg) > maxChange)
			{
				if (avg > prevAvg)
				{
					value = prevAvg + maxChange;
				}
				else
				{
					value = prevAvg - maxChange;
				}
			}
			else
			{
				value = avg;
			}

			// CLAMP VALUE
			if (value > 1) value = 1;
			if (value < -1) value = -1;

			//g.DrawString(avg.ToString(), Font, brushFont, new PointF(5f, 2f));

			// CORRELATION BOX
			y = Height - 19f;
			x = (float)((Width / 2f) + (((Width - 6f - boxWidth) / 2f) * value) - (boxWidth / 2));
			RectangleF box = new RectangleF(x, y, boxWidth, boxHeight);
			g.FillRectangle(boxBrush, box);

			// LINES
			g.DrawLine(penLineWhiteDotted, Width / 2f, Height - 19f, Width / 2f, Height - 4f);

			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f, Height - 19f, (boxWidth / 2f) + 3f, Height - 4f);
			g.DrawLine(penLineWhiteDotted, -(boxWidth / 2f) + Width - 6f, Height - 19f, -(boxWidth / 2f) + Width - 6f, Height - 4f);

			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 0.25f), Height - 19f, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 0.25f), Height - 4f);
			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 0.5f), Height - 19f, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 0.5f), Height - 4f);
			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 0.75f), Height - 19f, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 0.75f), Height - 4f);

			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 1.25f), Height - 19f, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 1.25f), Height - 4f);
			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 1.5f), Height - 19f, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 1.5f), Height - 4f);
			g.DrawLine(penLineWhiteDotted, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 1.75f), Height - 19f, (boxWidth / 2f) + 3f + (((Width - 6f - boxWidth) / 2f) * 1.75f), Height - 4f);

			prevAvg = value;
			
		}

		private double rad2deg(double rad)
		{
			return rad / (Math.PI / 180);
		}

		
	}
}
