using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace OriginalFire.Darkness.Barrager
{
	static class RenderUtils
	{
		public static Bitmap BoxBlur(Bitmap src, int radius)
		{
			Rectangle rect = new Rectangle(Point.Empty, src.Size);
			Bitmap dst = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppPArgb);
			BitmapData srcData = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
			BitmapData dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
			unsafe
			{
				byte* pSrc = (byte*)srcData.Scan0.ToPointer();
				byte* pDst = (byte*)dstData.Scan0.ToPointer();
				int line = rect.Width * 4;
				double div = (radius + radius + 1) * (radius + radius + 1);
				for (int i = 0; i < rect.Width; i++)
				{
					int bSum = 0;
					int gSum = 0;
					int rSum = 0;
					int aSum = 0;
					int upper = Math.Max(i - radius, 0);
					int under = Math.Min(rect.Width - 1, i + radius);
					// 初始化方阵和
					for (int j = 0; j < radius; j++)
					{
						byte* pS = pSrc + line * j + upper * 4;
						for (int k = upper; k <= under; k++)
						{
							bSum += *(pS++);
							gSum += *(pS++);
							rSum += *(pS++);
							aSum += *(pS++);
						}
					}
					// 遍历像素
					for (int j = 0; j < rect.Height; j++)
					{
						if (j - radius >= 0)
						{
							// 减左列
							byte* pS = pSrc + line * (j - radius) + upper * 4;
							for (int k = upper; k <= under; k++)
							{
								bSum -= *(pS++);
								gSum -= *(pS++);
								rSum -= *(pS++);
								aSum -= *(pS++);
							}
						}
						if (j + radius < rect.Height)
						{
							byte* pS = pSrc + line * (j + radius) + upper * 4;
							for (int k = upper; k <= under; k++)
							{
								bSum += *(pS++);
								gSum += *(pS++);
								rSum += *(pS++);
								aSum += *(pS++);
							}
						}
						byte* pD = pDst + j * line + i * 4;
						*(pD++) = (byte)Math.Round(bSum / div);
						*(pD++) = (byte)Math.Round(gSum / div);
						*(pD++) = (byte)Math.Round(rSum / div);
						*(pD++) = (byte)Math.Round(aSum / div);
					}

				}
			}
			src.UnlockBits(srcData);
			dst.UnlockBits(dstData);
			return dst;
		}

		public static void SetAlpha(Bitmap bitmap, float transparency)
		{
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
			unsafe
			{
				byte* p = (byte*)data.Scan0.ToPointer();
				for (int i = 0; i < rect.Height * rect.Width * 4; i++)
				{
					*p = Convert.ToByte(transparency * (*p));
					p++;
				}
			}
			bitmap.UnlockBits(data);
		}
	}
}
