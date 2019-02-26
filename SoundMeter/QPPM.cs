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
	class QPPM : Control
	{
		Brush brushFont = new SolidBrush(Color.FromArgb(255, 255, 204, 0));
		Brush brushBackground = new SolidBrush(Color.FromArgb(255, 16, 16, 16));
		Brush brushBar = new SolidBrush(Color.FromArgb(255, 0, 164, 32));

		Pen penBorder = new Pen(Color.FromArgb(255, 200, 200, 200), 1f);
		Pen penCross = new Pen(Color.FromArgb(255, 128, 0, 128), 1f);
		Pen penSamples = new Pen(Color.FromArgb(128, 0, 255, 0), 1f);
		Pen penLineWhite = new Pen(Color.FromArgb(255, 200, 200, 200), 1f);
		Pen penLineWhiteDotted = new Pen(Color.FromArgb(255, 200, 200, 200), 1f);
		Pen penLineRed = new Pen(Color.FromArgb(255, 200, 0, 0), 1f);
		Pen penLineRedDotted = new Pen(Color.FromArgb(255, 200, 0, 0), 1f);

		StringFormat centerAlign;
		StringFormat rightAlign;

		DateTime lastDrawTime;
		DateTime currentDrawTime;

		double scaleFactor;
		float verticalOffset = 10f;

		double levelL = -72f;
		double levelR = -72f;
		float heightL = 0f;
		float heightR = 0;

		double returnSpeed = 20 / 1.7; // DB pr. sec


		public QPPM()
		{
			this.DoubleBuffered = true;

			centerAlign = new StringFormat();
			centerAlign.Alignment = StringAlignment.Center;
			centerAlign.LineAlignment = StringAlignment.Center;

			rightAlign = new StringFormat();
			rightAlign.Alignment = StringAlignment.Far;
			rightAlign.LineAlignment = StringAlignment.Center;

			penLineWhiteDotted.DashPattern = new float[] { 1, 1 };
			penLineRedDotted.DashPattern = new float[] { 1, 1 };
		}

		/// <summary>
		/// Set the meter value in dBFS
		/// </summary>
		/// <param name="L"></param>
		/// <param name="R"></param>
		public void setLevels(double L, double R)
		{
			currentDrawTime = DateTime.Now;

			// Do return
			levelL = (levelL - returnSpeed * (currentDrawTime - lastDrawTime).TotalSeconds);
			levelR = (levelR - returnSpeed * (currentDrawTime - lastDrawTime).TotalSeconds);

			if (L > levelL) levelL = L;
			if (R > levelR) levelR = R;

			if (levelL < -72) levelL = -72;
			if (levelR < -72) levelR = -72;

			

			this.Invalidate();
			lastDrawTime = currentDrawTime;
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

			// SCALE
			scaleFactor = (Height - 30.0) / 60.0;

			g.DrawString("-42", Font, brushFont, new RectangleF(0f, (float)(60.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("-36", Font, brushFont, new RectangleF(0f, (float)(54.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("-30", Font, brushFont, new RectangleF(0f, (float)(48.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("-24", Font, brushFont, new RectangleF(0f, (float)(42.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("-18", Font, brushFont, new RectangleF(0f, (float)(36.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("-12", Font, brushFont, new RectangleF(0f, (float)(30.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("-6", Font, brushFont, new RectangleF(0f, (float)(24.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("TEST", Font, brushFont, new RectangleF(0f, (float)(18.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("+6", Font, brushFont, new RectangleF(0f, (float)(12.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("+9", Font, brushFont, new RectangleF(0f, (float)(9.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("+12", Font, brushFont, new RectangleF(0f, (float)(6.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);
			g.DrawString("+18", Font, brushFont, new RectangleF(0f, (float)(0.0 * scaleFactor) + verticalOffset, 34f, 0f), rightAlign);

			// SCALE LINES
			g.DrawLine(penLineWhite, 35f, (float)(60.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(60.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(57.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(57.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(54.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(54.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(51.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(51.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(48.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(48.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(45.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(45.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(42.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(42.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(39.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(39.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(36.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(36.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(33.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(33.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(30.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(30.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(27.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(27.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(24.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(24.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(21.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(21.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhite, 35f, (float)(18.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(18.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineWhiteDotted, 35f, (float)(15.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(15.0 * scaleFactor) + verticalOffset);

			g.DrawLine(penLineRed, 35f, (float)(12.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(12.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineRedDotted, 35f, (float)(9.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(9.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineRed, 35f, (float)(6.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(6.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineRedDotted, 35f, (float)(3.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(3.0 * scaleFactor) + verticalOffset);
			g.DrawLine(penLineRed, 35f, (float)(0.0 * scaleFactor) + verticalOffset, Width - 5f, (float)(0.0 * scaleFactor) + verticalOffset);

			// DRAW BAR
			heightL = (float)(-levelL * scaleFactor);
			if (heightL > scaleFactor * 62f) heightL = (float)scaleFactor * 62f;
			g.FillRectangle(brushBar, 40f, heightL + verticalOffset, (Width - 60f) / 2f, Height - heightL - verticalOffset - 2f);

			heightR = (float)(-levelR * scaleFactor);
			if (heightR > scaleFactor * 62f) heightR = (float)scaleFactor * 62f;
			g.FillRectangle(brushBar, 50f + ((Width - 60f) / 2f), heightR + verticalOffset, (Width - 60f) / 2f, Height - heightR - verticalOffset - 2f);
		}
	}
}
