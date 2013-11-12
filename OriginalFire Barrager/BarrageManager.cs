using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
	/// <summary>
	/// 提供滚动弹幕管理。
	/// </summary>
	class BarrageManager
	{
		/// <summary>
		/// 表示一行弹幕管道。
		/// </summary>
		private class BarrageLine
		{
			/// <summary>
			/// 初始化BarrageLine类的新实例。
			/// </summary>
			/// <param name="parent">管理此BarrageLine的BarrageManager。</param>
			public BarrageLine(BarrageManager parent)
			{
				this.parent = parent;
				queue = new Queue<Barrage>();
			}

			public void Dispose()
			{
				foreach (Barrage barrage in queue)
					barrage.Dispose();
			}

			/// <summary>
			/// 计算弹幕等待时间（按刷新步数）。
			/// </summary>
			/// <param name="barrage">将要新插入的弹幕。</param>
			/// <returns>一个int值，表示经过该值对应的时间后弹幕将显示。</returns>
			public int WaitingTime(Barrage barrage)
			{
				if (queue.Count == 0)
					return 0;
				Barrage last = queue.Last();
				if (last.Speed >= barrage.Speed)
				{
					int w = last.WaitingTime + last.Width / last.Speed + 1;
					return w < 0 ? 0 : w;
				}
				int diffSpeed = barrage.Speed - last.Speed;
				int lastCur = -last.WaitingTime * last.Speed;
				int conflict = parent.size.Width - parent.size.Width / barrage.Speed * last.Speed;
				if (conflict < lastCur)
					return 0;
				return (conflict - lastCur) / last.Speed;
			}

			/// <summary>
			/// 添加一条新弹幕。弹幕的等待时间将被修改为在该BarrageLine中的等待时间。
			/// </summary>
			/// <param name="barrage">将要新插入的弹幕。</param>
			public void AddBarrage(Barrage barrage)
			{
				barrage.WaitingTime = WaitingTime(barrage);
				queue.Enqueue(barrage);
			}

			/// <summary>
			/// 刷新弹幕位置。
			/// </summary>
			public void UpdateBarrageLocations()
			{
				foreach (Barrage barrage in queue)
				{
					barrage.WaitingTime--;
					if (barrage.WaitingTime < 0)
						barrage.X -= barrage.Speed;
				}
				if (queue.Count > 0)
				{
					Barrage b = queue.First();
					if (b.X + b.Width < 0)
					{
						queue.Dequeue();
						b.Dispose();
						parent.OnBarrageRemoved();
					}
				}
			}

			public void DrawBarrages(IntPtr hDc)
			{
				foreach (Barrage barrage in queue)
				{
					if (barrage.WaitingTime < 0)
					{
						barrage.Draw(hDc);
					}
				}
			}

			private BarrageManager parent;

			private Queue<Barrage> queue;
		}

		public BarrageManager(BarrageWindow parent, Size size, Configurations styleConf, Configurations actionConf)
		{
			renderer = new BarrageRenderer(styleConf);

			GlobalKeyHook.Instance.SetProcessor(
				styleConf.GetKeys("ReverseColor-Key", Keys.Control | Keys.Alt | Keys.W),
				k =>
				{
					reverseColor = !reverseColor;
					renderer.BorderColor = Color.FromArgb(
						255 - renderer.BorderColor.R,
						255 - renderer.BorderColor.G,
						255 - renderer.BorderColor.B);
					renderer.FillColor = Color.FromArgb(
						255 - renderer.FillColor.R,
						255 - renderer.FillColor.G,
						255 - renderer.FillColor.B);

					parent.ShowNotice(reverseColor ? "颜色：反色" : "颜色：正常");
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				styleConf.GetKeys("IncreaseAlpha-Key", Keys.Control | Keys.Alt | Keys.Oemplus),
				k =>
				{
					float value = (float)Math.Round(renderer.Transparency + 0.1f, 1);
					if (value > 1.0f)
						value = 1.0f;
					renderer.Transparency = value;
					parent.ShowNotice("透明度：" + (renderer.Transparency * 100).ToString() + "%");
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				styleConf.GetKeys("DecreaseAlpha-Key", Keys.Control | Keys.Alt | Keys.OemMinus),
				k =>
				{
					float value = (float)Math.Round(renderer.Transparency - 0.1f, 1);
					if (value < 0.0f)
						value = 0.0f;
					renderer.Transparency = value;
					parent.ShowNotice("透明度：" + (renderer.Transparency * 100).ToString() + "%");
					return true;
				}
			);

			string style = styleConf.GetString("Style", "border-blur");
			switch (style)
			{
				case "border-blur":
					this.barrageStyle = BarrageStyle.BorderBlur;
					break;
				case "border":
					this.barrageStyle = BarrageStyle.Border;
					break;
				default:
					this.barrageStyle = BarrageStyle.Shadow;
					break;
			}
			this.barrageHeight = styleConf.GetInt("Height", 40);
			this.speed = actionConf.GetInt("Speed", 5);
			this.maxTime = actionConf.GetInt("Max-Time", 560);
			this.maxWait = actionConf.GetInt("Max-Wait", 100);
			this.maxRenderWait = actionConf.GetInt("Max-RenderWait", 500);
			this.maxCount = actionConf.GetInt("Max-Count", 200);
			this.maxLength = actionConf.GetInt("Max-Length", 40);

			this.size = size;
			barrageLines = new BarrageLine[size.Height / barrageHeight];
			for (int i = 0; i < barrageLines.Length; i++)
				barrageLines[i] = new BarrageLine(this);

			Visible = true;
		}

		public void DrawBarrages(IntPtr hDc)
		{
			if (Visible){
			foreach (BarrageLine line in barrageLines)
				line.DrawBarrages(hDc);
			}
		}

		/// <summary>
		/// 刷新弹幕位置。
		/// </summary>
		public void UpdateBarrageLocations()
		{
			foreach (BarrageLine line in barrageLines)
				line.UpdateBarrageLocations();
		}

		/// <summary>
		/// 添加一条新的弹幕评论。
		/// </summary>
		/// <param name="comment">将要添加的弹幕评论内容。</param>
		public void AddBarrage(string comment)
		{
			if (count >= maxCount || comment.Length > maxLength)
				return;
			
			AsyncOperation op = AsyncOperationManager.CreateOperation(Guid.NewGuid());
			Thread thread = new Thread(() =>
			{
				bool entered = false;
				Barrage barrage = null;
				try
				{
					entered = Monitor.TryEnter(renderer, maxRenderWait);
					if (!entered)
					{
						Debugger.Log("等待绘制超时: " + comment);
						return;
					}
					Bitmap bitmap = renderer.RenderBarrage(comment, barrageStyle);

					barrage = new Barrage(bitmap);
					bitmap.Dispose();
				}
				catch (Exception e)
				{
					Debugger.Exception(e);
				}
				finally
				{
					if (entered)
						Monitor.Exit(renderer);
					if (barrage != null)
					{
						op.PostOperationCompleted(o =>
						{
							InternalAddBarrage(barrage);
							Debugger.Log("弹幕: " + comment);
						}, null);
					}
				}
			});
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}

		private void InternalAddBarrage(Barrage barrage)
		{
			barrage.WaitingTime = 0;
			barrage.Speed = speed;
			int normalTime = (barrage.Width + size.Width) / speed;
			if (normalTime > maxTime)
				barrage.Speed = (barrage.Width + size.Width) / maxTime;
			int min = 0;
			int minWaitingTime = Int32.MaxValue;
			for (int i = 0; i < barrageLines.Length; i++)
			{
				int waitingTime = barrageLines[i].WaitingTime(barrage);
				if (waitingTime < minWaitingTime)
				{
					min = i;
					minWaitingTime = waitingTime;
				}
			}
			if (minWaitingTime > maxWait)
				return;
			barrage.X = size.Width;
			barrage.Y = min * barrageHeight;
			barrageLines[min].AddBarrage(barrage);
			count++;
		}

		private void OnBarrageRemoved()
		{
			count--;
		}

		public void Dispose()
		{
			foreach (BarrageLine line in barrageLines)
				line.Dispose();
		}

		public bool Visible { get; set; }

		private int count;

		private Size size;

		private BarrageLine[] barrageLines;

		private BarrageStyle barrageStyle;

		private int maxTime;

		private int maxWait;

		private int maxRenderWait;

		private int maxCount;

		private int maxLength;

		private int speed;

		private int barrageHeight;

		private bool reverseColor;

		private BarrageRenderer renderer;

		private BarrageWindow parent;
	}
}
