using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
    sealed class BarrageRenderer : IDisposable
    {
		private StringFormat stringFormat;
		private Pen borderPen;
		private SolidBrush borderBrush;
		private SolidBrush fillBrush;

		public Color BorderColor
		{
			get
			{
				return borderPen.Color;
			}
			set
			{
				borderPen.Color = value;
				borderBrush.Color = value;
			}
		}

		public float BorderWidth
		{
			get
			{
				return borderPen.Width;
			}
			set
			{
				borderPen.Width = value;
			}
		}

		public Color FillColor
		{
			get
			{
				return fillBrush.Color;
			}
			set
			{
				fillBrush.Color = value;
			}
		}

		public Font Font { get; set; }
		public int HorizontalMargin { get; set; }
		public int BarrageHeight { get; set; }
		public int BlurRadius { get; set; }
		public float Transparency { get; set; }
		public int ShadowX { get; set; }
		public int ShadowY { get; set; }

        public BarrageRenderer(Configurations conf)
		{
			stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			borderPen = new Pen(conf.GetRgbColor("Border-Color", Color.Black), conf.GetSingle("Border-Width", 2));
			borderPen.LineJoin = LineJoin.Round;
			borderPen.Alignment = PenAlignment.Outset;
			borderBrush = new SolidBrush(conf.GetRgbColor("Border-Color", Color.Black));
			fillBrush = new SolidBrush(conf.GetRgbColor("Fill-Color", Color.White));
			FontStyle fontStyle = FontStyle.Regular;
			if (conf.GetBoolean("Font-Bold", true))
				fontStyle |= FontStyle.Bold;
			if (conf.GetBoolean("Font-Italic", false))
				fontStyle |= FontStyle.Italic;
			Font = new Font(conf.GetString("Font-Family", "黑体"),
				conf.GetSingle("Font-Size", 30), fontStyle, GraphicsUnit.Pixel);
			HorizontalMargin = conf.GetInt("Horizontal-Margin", 5);
			BarrageHeight = conf.GetInt("Height", 40);
			BlurRadius = conf.GetInt("Blur-Radius", 2);
			Transparency = conf.GetSingle("Transparency", 1.0f);
			ShadowX = conf.GetInt("Shadow-XOffset", 1);
			ShadowY = conf.GetInt("Shadow-YOffset", 1);
		}

        public void Dispose()
        {
			Font.Dispose();
            borderPen.Dispose();
			borderBrush.Dispose();
			fillBrush.Dispose();
        }

		public Bitmap RenderBarrage(string comment, BarrageStyle style)
		{
			IntPtr screenDc = ApiHelper.GetDC(IntPtr.Zero);
			Graphics measureGraphics = Graphics.FromHdc(screenDc);
			SizeF size = measureGraphics.MeasureString(comment, Font);
			int width = (int)Math.Ceiling(size.Width + HorizontalMargin * 2);
			measureGraphics.Dispose();
			ApiHelper.ReleaseDC(IntPtr.Zero, screenDc);
			RectangleF rect = new Rectangle(0, 0, width, BarrageHeight);
			switch (style)
			{
				case BarrageStyle.BorderBlur:
					{
						Bitmap bitmap = new Bitmap(width, BarrageHeight, PixelFormat.Format32bppPArgb);
						Graphics g = Graphics.FromImage(bitmap);
						g.SmoothingMode = SmoothingMode.AntiAlias;
						GraphicsPath path = new GraphicsPath();
						path.AddString(comment, Font.FontFamily, (int)Font.Style, Font.Size, rect, stringFormat);
						g.FillPath(borderBrush, path);
						g.DrawPath(borderPen, path);
						g.Dispose();
						Bitmap newBitmap = RenderUtils.BoxBlur(bitmap, BlurRadius);
						bitmap.Dispose();
						g = Graphics.FromImage(newBitmap);
						g.SmoothingMode = SmoothingMode.AntiAlias;
						g.CompositingQuality = CompositingQuality.HighQuality;
						g.FillPath(fillBrush, path);
						g.Dispose();
						RenderUtils.SetAlpha(newBitmap, Transparency);
						return newBitmap;
					}
				case BarrageStyle.Border:
					{
						Bitmap bitmap = new Bitmap(width, BarrageHeight, PixelFormat.Format32bppPArgb);
						Graphics g = Graphics.FromImage(bitmap);
						g.SmoothingMode = SmoothingMode.AntiAlias;
						GraphicsPath path = new GraphicsPath();
						path.AddString(comment, Font.FontFamily, (int)Font.Style, Font.Size, rect, stringFormat);
						g.DrawPath(borderPen, path);
						g.CompositingQuality = CompositingQuality.HighQuality;
						g.FillPath(fillBrush, path);
						g.Dispose();
						RenderUtils.SetAlpha(bitmap, Transparency);
						return bitmap;
					}
				case BarrageStyle.Shadow:
					{
						Bitmap bitmap = new Bitmap(width, BarrageHeight, PixelFormat.Format32bppPArgb);
						Graphics g = Graphics.FromImage(bitmap);
						g.SmoothingMode = SmoothingMode.AntiAlias;
						RectangleF newRect = rect;
						newRect.Offset(ShadowX, ShadowY);
						GraphicsPath path = new GraphicsPath();
						path.AddString(comment, Font.FontFamily, (int)Font.Style, Font.Size, newRect, stringFormat);
						g.FillPath(borderBrush, path);
						g.Dispose();
						Bitmap newBitmap = RenderUtils.BoxBlur(bitmap, BlurRadius);
						bitmap.Dispose();
						g = Graphics.FromImage(newBitmap);
						g.SmoothingMode = SmoothingMode.AntiAlias;
						g.CompositingQuality = CompositingQuality.HighQuality;
						path = new GraphicsPath();
						path.AddString(comment, Font.FontFamily, (int)Font.Style, Font.Size, rect, stringFormat);
						g.FillPath(fillBrush, path);
						g.Dispose();
						RenderUtils.SetAlpha(newBitmap, Transparency);
						return newBitmap;
					}
				default:
					throw new ArgumentException("style参数无效。", "style");
			}
		}
    }
}
