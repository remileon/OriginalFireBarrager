using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OriginalFire.Darkness.Barrager
{
	class SubtitleGroup
	{
		/*******************************************************
		 * 
		 * 时间计算方式：
		 *     - offset：        全局偏移量
		 *     - subtitle：      字幕时间
		 *     - currentTime：   计时器当前时间
		 * 
		 *  offset + subtitle = currentTime
		 ******************************************************/

		public SubtitleGroup(SubtitleManager manager, LyricFile file, int spaceTime = 300)
		{
			parentManager = manager;
			orgOffset = file.Offset;
			offset = orgOffset;
			currentTime = 0;
			lyrics = new LinkedList<SubtitleContent>();
			lyrics.AddLast(new SubtitleContent(String.Empty));
			foreach (Lyric lyric in file.Lyrics)
			{
				if (!String.IsNullOrWhiteSpace(lyrics.Last.Value.Content)
					&& lyrics.Last.Value.StartTime + spaceTime * 2 < lyric.TimeTag)
				{
					lyrics.Last.Value.EndTime = lyric.TimeTag - spaceTime;
					lyrics.AddLast(new SubtitleContent(String.Empty, lyric.TimeTag - spaceTime, lyric.TimeTag));
				}
				else
					lyrics.Last.Value.EndTime = lyric.TimeTag;
				lyrics.AddLast(new SubtitleContent(lyric.Content, lyric.TimeTag, lyric.TimeTag));
			}
			currentSubtitle = lyrics.First;
			Title = file.FileName;
		}

		public void Draw(IntPtr hDc)
		{
			if (Current != null)
				Current.Draw(hDc);
			if (Alternative != null)
				Alternative.Draw(hDc);
		}

		/// <summary>
		/// 获取当前Subtitle。
		/// </summary>
		public Subtitle Current { get; private set; }

		/// <summary>
		/// 获取当前第二Subtitle。
		/// </summary>
		public Subtitle Alternative { get; private set; }

		/// <summary>
		/// 获取或设置当先时间。
		/// </summary>
		public long CurrentTime
		{
			get
			{
				return currentTime;
			}
			set
			{
				currentTime = value;
				long subtitleTime = value - offset;
				var node = currentSubtitle;
				if (currentSubtitle.Value.StartTime > subtitleTime)
				{
					while (currentSubtitle.Previous != null
						&& currentSubtitle.Value.StartTime > subtitleTime)
						currentSubtitle = currentSubtitle.Previous;
					if (node != currentSubtitle)
						ResetCurrent();
				}
				else if (currentSubtitle.Value.EndTime < subtitleTime)
				{
					while (currentSubtitle.Next != null
						&& currentSubtitle.Value.EndTime < subtitleTime)
						currentSubtitle = currentSubtitle.Next;
					if (node != currentSubtitle)
						ResetCurrent();
				}
			}
		}

		public bool Ended
		{
			get
			{
				return currentSubtitle.Next == null;
			}
		}

		public string Title { get; set; }

		public void NextSubtitle(long time)
		{
			var cur = currentSubtitle;
			if (currentSubtitle.Next != null)
				currentSubtitle = currentSubtitle.Next;
			while (String.IsNullOrWhiteSpace(currentSubtitle.Value.Content)
				&& currentSubtitle.Next != null)
				currentSubtitle = currentSubtitle.Next;
			offset = time - currentSubtitle.Value.StartTime;
			if (cur != currentSubtitle)
				ResetCurrent();
		}

		public void PrevSubtitle(long time)
		{
			var cur = currentSubtitle;
			if (currentSubtitle.Previous != null)
				currentSubtitle = currentSubtitle.Previous;
			while (String.IsNullOrWhiteSpace(currentSubtitle.Value.Content)
				&& currentSubtitle.Previous != null)
				currentSubtitle = currentSubtitle.Previous;
			offset = time - currentSubtitle.Value.StartTime;
			if (cur != currentSubtitle)
				ResetCurrent();
		}

		public void ResetToStart()
		{
			currentSubtitle = lyrics.First;
			currentTime = 0;
			offset = orgOffset;
			if (Current != null)
				Current.Dispose();
		}

		private void ResetCurrent()
		{
			if (Current != null)
				Current.Dispose();
			if (Alternative != null)
				Alternative.Dispose();
			string content = currentSubtitle.Value.Content;
			if (String.IsNullOrWhiteSpace(content))
			{
				Current = null;
				Alternative = null;
			}
			else
			{
				string[] strs = content.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);

				if (strs.Length == 2 && !String.IsNullOrWhiteSpace(strs[0]) && !String.IsNullOrWhiteSpace(strs[1]))
				{
					Alternative = new Subtitle(strs[1].Trim(),
						parentManager.Font, parentManager.FillColor,
						parentManager.BorderColor, parentManager.BorderWidth,
						currentSubtitle.Value.StartTime, currentSubtitle.Value.EndTime);
					Size size = Alternative.Size;
					Rectangle rect = parentManager.Rect;
					Alternative.Location = new Point(rect.Right - size.Width, rect.Bottom - size.Height);
					Current = new Subtitle(strs[0].Trim(),
						parentManager.Font, parentManager.FillColor,
						parentManager.BorderColor, parentManager.BorderWidth,
						currentSubtitle.Value.StartTime, currentSubtitle.Value.EndTime);
					size = Current.Size;
					Current.Location = new Point(rect.Left, rect.Bottom - size.Height - Alternative.Height);
				}
				else
				{
					Alternative = null;
					Current = new Subtitle(strs[0].Trim(),
						   parentManager.Font, parentManager.FillColor,
						   parentManager.BorderColor, parentManager.BorderWidth,
						   currentSubtitle.Value.StartTime, currentSubtitle.Value.EndTime);
					Size size = Current.Size;
					Rectangle rect = parentManager.Rect;
					Current.Location = new Point((rect.Left + rect.Right - size.Width) / 2, rect.Bottom - size.Height);
				}
			}
		}

		private long offset;

		private long orgOffset;

		private long currentTime;

		private LinkedList<SubtitleContent> lyrics;

		private LinkedListNode<SubtitleContent> currentSubtitle;

		private SubtitleManager parentManager;

		private static char[] separator = { ';' };
	}
}
