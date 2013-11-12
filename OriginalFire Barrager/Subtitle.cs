using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
	class Subtitle : IDisposable
	{
		private static readonly StringFormat defaultFormat;

		static Subtitle()
		{
			defaultFormat = new StringFormat(StringFormatFlags.NoWrap);
			defaultFormat.Alignment = StringAlignment.Center;
			defaultFormat.LineAlignment = StringAlignment.Center;
		}

		public Subtitle(string content,
			Font font, Color fillColor, Color borderColor, float borderWidth,
			long startTime, long endTime)
		{
			StartTime = startTime;
			EndTime = endTime;

			IntPtr screenDc = ApiHelper.GetDC(IntPtr.Zero);
			Graphics g = Graphics.FromHdc(screenDc);
			SizeF gsize = g.MeasureString(content, font);
			g.Dispose();
			this.Size = Size.Ceiling(new SizeF(gsize.Width + borderWidth, gsize.Height + borderWidth));

			hMemDc = ApiHelper.CreateCompatibleDC(screenDc);
			hMemBitmap = ApiHelper.CreateCompatibleBitmap(screenDc, Size.Width, Size.Height);
			ApiHelper.ReleaseDC(IntPtr.Zero, screenDc);
			ApiHelper.SelectObject(hMemDc, hMemBitmap);
			g = Graphics.FromHdc(hMemDc);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			GraphicsPath path = new GraphicsPath();
			path.AddString(content, font.FontFamily, (int)font.Style, font.Size,
				new Rectangle(Point.Empty, Size), defaultFormat);
			
			Pen pen = new Pen(borderColor, borderWidth);
			pen.LineJoin = LineJoin.Round;
			pen.Alignment = PenAlignment.Outset;
			g.DrawPath(pen, path);
			pen.Dispose();
			Brush brush = new SolidBrush(fillColor);
			g.FillPath(brush, path);
			brush.Dispose();
			g.Dispose();
		}

		~Subtitle()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!disposed)
			{
				ApiHelper.DeleteDC(hMemDc);
				ApiHelper.DeleteObject(hMemBitmap);
				disposed = true;
			}
		}

		public void Draw(IntPtr hDc)
		{
			if (!ApiHelper.AlphaBlend(hDc, X, Y, Width, Height,
				hMemDc, 0, 0, Width, Height,
				ApiHelper.DefaultBlend))
				Debugger.Log(ApiHelper.GetLastError().ToString());
		}

		public long StartTime { get; set; }

		public long EndTime { get; set; }

		public Point Location
		{
			get
			{
				return location;
			}
			set
			{
				location = value;
			}
		}

		public int X
		{
			get
			{
				return location.X;
			}
			set
			{
				location.X = value;
			}
		}

		public int Y
		{
			get
			{
				return location.Y;
			}
			set
			{
				location.Y = value;
			}
		}

		public Size Size { get; private set; }

		public int Width { get { return Size.Width; } }

		public int Height { get { return Size.Height; } }

		private IntPtr hMemDc;
		private IntPtr hMemBitmap;
		private Point location;
		private bool disposed = false;
	}
}
