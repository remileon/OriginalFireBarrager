using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OriginalFire.Darkness.Barrager
{
	static class Infomations
	{
		public static Bitmap CreateInfoBitmap()
		{
			return CreateInfomationBitmap(@"OriginalFire Barrager 2nd Edition ver0.93
双击：最大化
拖动：移动位置（可拖动至第二屏幕）
拖动边框：调整大小
右击：开始弹幕",
		360, 120);
		}

		public static Bitmap CreateCopyrightBitmap()
		{
			return CreateInfomationBitmap(@"©2013.9 OriginalFire Darkness. All Rights Reserved.",
			500, 40, StringAlignment.Far, StringAlignment.Far);
		}

		private static Bitmap CreateInfomationBitmap(string content, int width, int height,
			StringAlignment hAlign = StringAlignment.Near,
			StringAlignment vAlign = StringAlignment.Near)
		{
			Bitmap b = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(b);
			GraphicsPath path = new GraphicsPath();
			FontFamily fontFamily = new FontFamily("微软雅黑");
			StringFormat format = new StringFormat();
			format.Alignment = hAlign;
			format.LineAlignment = vAlign;
			path.AddString(content,
				fontFamily, (int)FontStyle.Bold, 16, new Rectangle(0, 0, width, height), format);
			fontFamily.Dispose();
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.FillPath(Brushes.Black, path);
			Pen pen = new Pen(Color.Black, 2);
			g.DrawPath(pen, path);
			g.Dispose();
			pen.Dispose();
			Bitmap infoBitmap = RenderUtils.BoxBlur(b, 1);
			g = Graphics.FromImage(infoBitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.FillPath(Brushes.White, path);
			g.Dispose();
			return infoBitmap;
		}
	}
}
