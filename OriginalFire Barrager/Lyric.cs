using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace OriginalFire.Darkness.Barrager
{
	/// <summary>
	/// 表示一条歌词。
	/// </summary>
	class Lyric : IComparable<Lyric>
	{
		public Lyric()
		{
		}

		public Lyric(long timeTag, string content)
		{
			TimeTag = timeTag;
			Content = content;
		}

		/// <summary>
		/// 获取或设置此Lyric的时间戳。
		/// </summary>
		public long TimeTag { get; set; }

		/// <summary>
		/// 获取或设置此Lyric的内容。
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 比较两条歌词的时间顺序。
		/// </summary>
		/// <param name="b">表示另一条歌词。</param>
		/// <returns>比较结果。</returns>
		public int CompareTo(Lyric b)
		{
			return TimeTag.CompareTo(b.TimeTag);
		}
	}

}
