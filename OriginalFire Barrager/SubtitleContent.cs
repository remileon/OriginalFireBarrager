using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriginalFire.Darkness.Barrager
{
	class SubtitleContent
	{
		public SubtitleContent()
		{
		}

		public SubtitleContent(string content, long startTime = 0, long endTime = 0)
		{
			Content = content;
			StartTime = startTime;
			EndTime = endTime;
		}

		public long StartTime { get; set; }

		public long EndTime { get; set; }

		public string Content { get; set; }
	}
}
