using System;
using System.Drawing;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
	/// <summary>
	/// 表示一条弹幕评论。
	/// </summary>
	class Barrage : IDisposable
	{
		/// <summary>
		/// 初始化Barrage类的新实例。
		/// </summary>
		public Barrage(Bitmap bitmap)
		{
			IntPtr screenDc = ApiHelper.GetDC(IntPtr.Zero);
			hMemDc = ApiHelper.CreateCompatibleDC(screenDc);
			hMemBitmap = ApiHelper.CreateCompatibleBitmap(screenDc, bitmap.Width, bitmap.Height);
			ApiHelper.SelectObject(hMemDc, hMemBitmap);
			ApiHelper.ReleaseDC(IntPtr.Zero, screenDc);

			Graphics g = Graphics.FromHdc(hMemDc);
			g.DrawImage(bitmap, 0, 0);
			g.Dispose();

			size = new Size(bitmap.Width, bitmap.Height);
		}

		~Barrage()
		{
			ApiHelper.DeleteDC(hMemDc);
			ApiHelper.DeleteObject(hMemBitmap);
		}

		/// <summary>
		/// 释放此Barrage所包含的资源。
		/// </summary>
		public void Dispose()
		{
			ApiHelper.DeleteDC(hMemDc);
			ApiHelper.DeleteObject(hMemBitmap);
		}

		/// <summary>
		/// 获取或设置此Barrage的移动速度。
		/// </summary>
		public int Speed
		{
			get
			{
				return speed;
			}
			set
			{
				speed = value;
			}
		}

		/// <summary>
		/// 获取或设置此Barrage的等待时间。
		/// </summary>
		public int WaitingTime
		{
			get
			{
				return waitingTime;
			}
			set
			{
				waitingTime = value;
			}
		}

		/// <summary>
		/// 获取或设置此Barrage的位置。
		/// </summary>
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

		/// <summary>
		/// 获取或设置此Barrage的 x 坐标。
		/// </summary>
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

		/// <summary>
		/// 获取或设置此Barrage的 y 坐标。
		/// </summary>
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

		/// <summary>
		/// 获取此Barrage的大小。
		/// </summary>
		public Size Size
		{
			get
			{
				return size;
			}
		}

		/// <summary>
		/// 获取此Barrage的宽度。
		/// </summary>
		public int Width
		{
			get
			{
				return size.Width;
			}
		}

		/// <summary>
		/// 获取此Barrage的高度。
		/// </summary>
		public int Height
		{
			get
			{
				return size.Height;
			}
		}

		public void Draw(IntPtr hDc)
		{
			if (!ApiHelper.AlphaBlend(hDc, X, Y, Width, Height,
				hMemDc, 0, 0, Width, Height,
				ApiHelper.DefaultBlend))
				Debugger.Log(ApiHelper.GetLastError().ToString());
		}

		private int waitingTime;

		private int speed;

		private Point location;

		private Size size;

		private IntPtr hMemDc;

		private IntPtr hMemBitmap;
	}
}
