using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
	class BarrageWindow : Form
	{
		public BarrageWindow(ConfigurationsCollection confs)
		{
			// Initialize Form

			this.ShowInTaskbar = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Size = new Size(600, 400);
			this.MinimumSize = new Size(500, 300);
			this.StartPosition = FormStartPosition.WindowsDefaultLocation;

			IntPtr screenDc = ApiHelper.GetDC(IntPtr.Zero);
			hMemDc = ApiHelper.CreateCompatibleDC(screenDc);
			hBitmap = ApiHelper.CreateCompatibleBitmap(screenDc, ClientSize.Width, ClientSize.Height);
			ApiHelper.ReleaseDC(IntPtr.Zero, screenDc);
			ApiHelper.SelectObject(hMemDc, hBitmap);

			// Initialize Configurations
			
			this.confs = confs;
			hideKey = confs["General"].GetKeys("Hide-Key", Keys.Control | Keys.Alt | Keys.Q);

			GlobalKeyHook.Instance.SetProcessor(confs["General"].GetKeys("Exit-Key",
					Keys.Control | Keys.Alt | Keys.Shift | Keys.P),
				k =>
			{
				this.BeginInvoke(new Action(() => this.Close()));
				return true;
			});
			GlobalKeyHook.Instance.Start();

			reverseColorKey = confs["General"].GetKeys("ReversColor-Key", Keys.Control | Keys.Alt | Keys.W);
			increaseAlphaKey = confs["General"].GetKeys("IncreaseAlpha-Key", Keys.Control | Keys.Alt | Keys.E);
			decreaseAlphaKey = confs["General"].GetKeys("DecreaseAlpha-Key", Keys.Control | Keys.Alt | Keys.R);
			infoBackColor = confs["General"].GetArgbColor("Info-BackColor", Color.FromArgb(128, Color.Gray));
			infoBorderColor = confs["General"].GetArgbColor("Info-BorderColor", Color.Red);

			// Initialize NotifyIcon

			notifyIcon = new NotifyIcon();
			notifyIcon.Text = "OriginalFire Barrager";
			notifyIcon.Icon = Resource.OriginalFire_Barrager;
			MenuItem exitMenu = new MenuItem("退出", (sender, e) => this.Close());
			ContextMenu contextMenu = new ContextMenu();
			contextMenu.MenuItems.Add(exitMenu);
			notifyIcon.ContextMenu = contextMenu;
			notifyIcon.Visible = true;

			// Initialize Infomations

			infoBitmap = Infomations.CreateInfoBitmap();
			copyrightBitmap = Infomations.CreateCopyrightBitmap();
		}

		public void UpdateWindow()
		{
			IntPtr screenDc = ApiHelper.GetDC(IntPtr.Zero);
			try
			{
				ApiHelper.Size newSize = new ApiHelper.Size(ClientSize.Width, ClientSize.Height);
				ApiHelper.Point sourceLocation = new ApiHelper.Point(0, 0);
				ApiHelper.Point newLocation = new ApiHelper.Point(this.Left, this.Top);

				if (ApiHelper.UpdateLayeredWindow(Handle, screenDc, ref newLocation, ref newSize,
					hMemDc, ref sourceLocation, 0, ref ApiHelper.DefaultBlend, ApiHelper.ULW_ALPHA) != ApiHelper.Bool.True)
					Debugger.Log("UpdateLayeredWindow failed，Last Error: " + ApiHelper.GetLastError());
			}
			catch (Exception e)
			{
				Debugger.Exception(e);
			}
			finally
			{
				ApiHelper.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= ApiHelper.WS_EX_LAYERED;
				return createParams;
			}
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == ApiHelper.WM_NCHITTEST)
			{
				Point p = new Point(message.LParam.ToInt32());
				message.Result = (IntPtr)HitTest(p.X, p.Y);
			}
			else if (message.Msg == ApiHelper.WM_NCRBUTTONUP)
			{
				StartBarrager();
				message.Result = (IntPtr)1;
			}
			else
			{
				base.WndProc(ref message);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			TopMost = true;
			started = false;
			ShowInfo();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (Visible)
			{
				IntPtr screenDc = ApiHelper.GetDC(IntPtr.Zero);
				ApiHelper.DeleteObject(hBitmap);
				hBitmap = ApiHelper.CreateCompatibleBitmap(screenDc,Width, Height);
				ApiHelper.ReleaseDC(IntPtr.Zero, screenDc);
				ApiHelper.SelectObject(hMemDc, hBitmap);
			}
			if (Visible && !started)
				ShowInfo();
		}

		private void ShowInfo()
		{
			Graphics g = Graphics.FromHdc(hMemDc);
			g.Clear(infoBackColor);
			g.DrawImage(infoBitmap, new Point(10, 10));
			g.DrawImage(copyrightBitmap, Width - copyrightBitmap.Width - 10, Height - copyrightBitmap.Height - 10);
			GraphicsPath path = new GraphicsPath();
			path.AddRectangle(new Rectangle(0, 0, Width, Height));
			path.AddRectangle(Rectangle.FromLTRB(infoBorderWidth, infoBorderWidth, Width - infoBorderWidth, Height - infoBorderWidth));
			Brush brush = new SolidBrush(infoBorderColor);
			g.FillPath(brush, path);
			brush.Dispose();
			path.Dispose();
			g.Dispose();
			UpdateWindow();
		}

		public void StartBarrager()
		{
			started = true;

			int exStyle = ApiHelper.GetWindowLong(Handle, ApiHelper.GWL_EXSTYLE);
			ApiHelper.SetWindowLong(Handle, ApiHelper.GWL_EXSTYLE, exStyle | ApiHelper.WS_EX_TRANSPARENT);

			// 隐藏弹幕快捷键。
			GlobalKeyHook.Instance.SetProcessor(hideKey, k =>
			{
				barrageManager.Visible = !barrageManager.Visible;
				ShowNotice(barrageManager.Visible ? "弹幕显示" : "弹幕隐藏");
				return true;
			});
			

			// 实例化弹幕源。
			
			inputs = new List<CommentInput>();
			foreach (KeyValuePair<string, string> keyValue in confs["Input"])
			{
				try
				{
					Assembly assembly = Assembly.LoadFrom(keyValue.Value);
					foreach (Type type in assembly.GetTypes())
					{
						if (type.Name.Equals(keyValue.Key))
						{
							CommentInput input = (CommentInput)Activator.CreateInstance(type);
							input.Initialize(confs[keyValue.Key]);
							input.Comment += (sender, ce) => barrageManager.AddBarrage(ce.Comment);
							Debugger.Log("弹幕源: " + input.GetType().FullName);
							inputs.Add(input);
						}
					}
				}
				catch (Exception exc)
				{
					Debugger.Exception(exc);
				}
			}

			barrageManager = new BarrageManager(this, ClientSize, confs["Appearance"], confs["Action"]);
			subtitleManager = new SubtitleManager(this, ClientRectangle, confs["Subtitle"]);

			stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			frameTimer = new Timer();
			frameTimer.Interval = 30;
			frameTimer.Tick += new EventHandler(frameTimer_Tick);
			frameTimer.Start();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (inputs != null)
					foreach (CommentInput input in inputs)
						input.Dispose();
				if (frameTimer != null)
					frameTimer.Dispose();
				notifyIcon.Dispose();
				if (barrageManager != null)
					barrageManager.Dispose();
			}
			GlobalKeyHook.Instance.Stop();
			ApiHelper.DeleteDC(hMemDc);
			ApiHelper.DeleteObject(hBitmap);
		}

		private void frameTimer_Tick(object sender, EventArgs e)
		{
			barrageManager.UpdateBarrageLocations();
			if (subtitleManager.Running)
				subtitleManager.UpdateTime();
			Redraw();
		}

		private void Redraw()
		{
			Graphics g = Graphics.FromHdc(hMemDc);
			g.Clear(Color.Transparent);
			g.Dispose();
			barrageManager.DrawBarrages(hMemDc);
			if (subtitleManager.Running)
				subtitleManager.DrawSubtitles(hMemDc);
			if (noticeSubtitle != null)
			{
				if (noticeSubtitle.EndTime < stopwatch.ElapsedMilliseconds)
				{
					noticeSubtitle.Dispose();
					noticeSubtitle = null;
				}
				else
					noticeSubtitle.Draw(hMemDc);
			}
			UpdateWindow();
		}

		public void ShowNotice(string notice, int time = 1000)
		{
			if (noticeSubtitle != null)
			{
				noticeSubtitle.Dispose();
				noticeSubtitle = null;
			}
			Font font = new System.Drawing.Font("微软雅黑", 30, FontStyle.Bold, GraphicsUnit.Pixel);
			noticeSubtitle = new Subtitle(notice, font, Color.Yellow, Color.Black, 3.0f,
				stopwatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds + time);
			noticeSubtitle.Location = new Point(10, Height - noticeSubtitle.Height - 10);
		}

		private ApiHelper.HTValues HitTest(int x, int y)
		{
			int index = 0;
			Point point = this.DesktopLocation;
			Size size = this.Size;
			int m_x = point.X;
			int m_y = point.Y;
			int m_width = size.Width;
			int m_height = size.Height;
			if (m_x <= x && m_x + infoBorderWidth > x)
				index += 0;
			else if (m_x + infoBorderWidth <= x && m_x + m_width - infoBorderWidth > x)
				index += 1;
			else if (m_x + m_width - infoBorderWidth <= x && m_x + m_width > x)
				index += 2;
			else
				return ApiHelper.HTValues.HTNOWHERE;
			if (m_y <= y && m_y + infoBorderWidth > y)
				index += 0;
			else if (m_y + infoBorderWidth <= y && m_y + m_height - infoBorderWidth > y)
				index += 3;
			else if (m_y + m_height - infoBorderWidth <= y && m_y + m_height > y)
				index += 6;
			else
				return ApiHelper.HTValues.HTNOWHERE;
			return hitTestValue[index];
		}

		private static readonly ApiHelper.HTValues[] hitTestValue =
		{
			ApiHelper.HTValues.HTTOPLEFT, ApiHelper.HTValues.HTTOP, ApiHelper.HTValues.HTTOPRIGHT,
			ApiHelper.HTValues.HTLEFT, ApiHelper.HTValues.HTCAPTION, ApiHelper.HTValues.HTRIGHT,
			ApiHelper.HTValues.HTBOTTOMLEFT, ApiHelper.HTValues.HTBOTTOM, ApiHelper.HTValues.HTBOTTOMRIGHT
		};

		private ConfigurationsCollection confs;

		private Keys hideKey;

		private Keys reverseColorKey;

		private Keys increaseAlphaKey;

		private Keys decreaseAlphaKey;

		private NotifyIcon notifyIcon;

		private Timer frameTimer;

		private System.Diagnostics.Stopwatch stopwatch;

		private BarrageManager barrageManager;

		private SubtitleManager subtitleManager;

		private List<CommentInput> inputs;

		private IntPtr hMemDc;

		private IntPtr hBitmap;

		private Bitmap infoBitmap;

		private Bitmap copyrightBitmap;

		private bool started;

		private int infoBorderWidth = 3;

		private Color infoBackColor;

		private Color infoBorderColor;

		private Subtitle noticeSubtitle;

	}
}
