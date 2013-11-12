using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OriginalFire.Darkness.Barrager.Core;
using System.Windows.Forms;
using System.Diagnostics;

namespace OriginalFire.Darkness.Barrager
{
	class SubtitleManager
	{
		public SubtitleManager(BarrageWindow parent, Rectangle rect, Configurations subtitleConf)
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (subtitleConf.GetBoolean("Font-Bold", true))
				fontStyle |= FontStyle.Bold;
			if (subtitleConf.GetBoolean("Font-Italic", false))
				fontStyle |= FontStyle.Italic;
			Font = new Font(subtitleConf.GetString("Font-Family", "黑体"),
				subtitleConf.GetSingle("Font-Size", 30), fontStyle, GraphicsUnit.Pixel);
			Padding = subtitleConf.GetInt("Padding", 10);
			FillColor = subtitleConf.GetRgbColor("Fill-Color", Color.White);
			BorderColor = subtitleConf.GetRgbColor("Border-Color", Color.Black);
			BorderWidth = subtitleConf.GetSingle("Border-Width", 1.0f);

			current = -1;
			groups = new List<SubtitleGroup>();
			int n;
			var lycFiles = from pair in subtitleConf
						   where pair.Key.StartsWith("Lyc-File")
							&& Int32.TryParse(pair.Key.Substring("Lyc-File".Length), out n)
						   select new { Index = Int32.Parse(pair.Key.Substring("Lyc-File".Length)) - 1,
							   FileName = pair.Value };
			foreach (var lycFile in lycFiles.OrderBy(l => l.Index))
			{
				try
				{
					groups.Add(new SubtitleGroup(this, new LyricFile(lycFile.FileName)));
					Core.Debugger.Log("lyc loaded: " + lycFile.FileName);
				}
				catch (Exception exc)
				{
					Core.Debugger.Log("lyc file load failed: " + lycFile.FileName + " (" + exc.Message + ")");
				}
			}
			if (groups.Count >= 0)
				current = 0;

			stopwatch = new Stopwatch();
			this.Rect = Rectangle.FromLTRB(rect.Left + Padding, rect.Top + Padding,
				rect.Right - Padding, rect.Bottom - Padding);

			GlobalKeyHook.Instance.SetProcessor(
				subtitleConf.GetKeys("Start-Key", Keys.Control | Keys.Alt | Keys.D1),
				k =>
				{
					if (StartSubtitle())
						parent.ShowNotice("字幕开始");
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				subtitleConf.GetKeys("End-Key", Keys.Control | Keys.Alt | Keys.D2),
				k =>
				{
					if (StopSubtitle())
						parent.ShowNotice("字幕停止");
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				subtitleConf.GetKeys("Prev-Key", Keys.Control | Keys.Alt | Keys.PageUp),
				k =>
				{
					PrevSubtitle();
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				subtitleConf.GetKeys("Next-Key", Keys.Control | Keys.Alt | Keys.PageDown),
				k =>
				{
					NextSubtitle();
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				subtitleConf.GetKeys("Prev-Lrc", Keys.Control | Keys.Alt | Keys.D3),
				k =>
				{
					if (PrevGroup())
						parent.ShowNotice("字幕：" + CurrentGroup.Title);
					return true;
				}
			);

			GlobalKeyHook.Instance.SetProcessor(
				subtitleConf.GetKeys("Next-Lrc", Keys.Control | Keys.Alt | Keys.D4),
				k =>
				{
					if (NextGroup())
						parent.ShowNotice("字幕：" + CurrentGroup.Title);
					return true;
				}
			);
		}

		public bool StartSubtitle()
		{
			if (CurrentGroup != null)
			{
				CurrentGroup.ResetToStart();
				Running = true;
				stopwatch.Restart();
				return true;
			}
			return false;
		}

		public bool StopSubtitle()
		{
			if (Running)
			{
				stopwatch.Stop();
				Running = false;
				return true;
			}
			return false;
		}

		public bool PrevGroup()
		{
			if (groups.Count > 0)
			{
				if (Running)
					Running = false;
				current--;
				if (current < 0)
					current += groups.Count;
				CurrentGroup.ResetToStart();
				return true;
			}
			return false;
		}

		public bool NextGroup()
		{
			if (groups.Count > 0)
			{
				if (Running)
					Running = false;
				current++;
				if (current >= groups.Count)
					current -= groups.Count;
				CurrentGroup.ResetToStart();
				return true;
			}
			return false;
		}

		public void PrevSubtitle()
		{
			if (Running)
			{
				CurrentGroup.PrevSubtitle(stopwatch.ElapsedMilliseconds);
			}
		}

		public void NextSubtitle()
		{
			if (Running)
			{
				CurrentGroup.NextSubtitle(stopwatch.ElapsedMilliseconds);
			}
		}

		public void UpdateTime()
		{
			if (CurrentGroup != null)
			{
				CurrentGroup.CurrentTime = stopwatch.ElapsedMilliseconds;
				if (CurrentGroup.Ended)
					Running = false;
			}
		}

		public void DrawSubtitles(IntPtr hDc)
		{
			if (CurrentGroup != null)
				CurrentGroup.Draw(hDc);
		}

		public int Padding { get; set; }

		public bool Running { get; private set; }

		public Color FillColor { get; set; }

		public Color BorderColor { get; set; }

		public float BorderWidth { get; set; }

		public Rectangle Rect { get; private set; }

		public Font Font { get; set; }

		private int current;

		private List<SubtitleGroup> groups;

		private SubtitleGroup CurrentGroup
		{
			get
			{
				if (current >= 0 && current < groups.Count)
					return groups[current];
				return null;
			}
		}

		private BarrageWindow parent;

		private Stopwatch stopwatch;
	}
}
