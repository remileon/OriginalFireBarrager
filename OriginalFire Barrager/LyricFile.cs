using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace OriginalFire.Darkness.Barrager
{
	class LyricFile
	{
		private static Regex timeTagReg = new Regex(
			@"\[(?<min>\d+):(?<sec>\d+(\.\d+)?)\]",
			RegexOptions.Compiled);
		private static Regex specTagReg = new Regex(
			@"\[(?<key>.+?):(?<value>.*?)\]",
			RegexOptions.Compiled);

		public LyricFile(string fileName)
		{
			FileName = Path.GetFileNameWithoutExtension(fileName);
			using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
			{
				SpectialTags = new Dictionary<string, string>
				{
					{"ar", String.Empty},
					{"ti", String.Empty},
					{"al", String.Empty},
					{"by", String.Empty},
					{"offset", "0"}
				};
				List<Lyric> lines = new List<Lyric>();
				String readin;
				while ((readin = reader.ReadLine()) != null)
				{
					string content = readin.Substring(readin.LastIndexOf(']') + 1).Trim();
					foreach (Match match in specTagReg.Matches(readin))
						SpectialTags[match.Groups["key"].Value] = match.Groups["value"].Value;
					foreach (Match match in timeTagReg.Matches(readin))
					{
						long timeTag = Int64.Parse(match.Groups["min"].Value) * 60000
							+ Convert.ToInt64(Single.Parse(match.Groups["sec"].Value) * 1000);
						Lyric item = new Lyric(timeTag, content);
						lines.Add(item);
					}
				}
				Offset = Int32.Parse(SpectialTags["offset"]);
				Lyrics = new Queue<Lyric>(lines.OrderBy(l => l.TimeTag));
			}
		}

		/// <summary>
		/// 获取或设置此lrc的标识标签。
		/// </summary>
		public Dictionary<string, string> SpectialTags { get; set; }

		/// <summary>
		/// 获取或设置此lrc的时间补偿值。
		/// </summary>
		public int Offset { get; set; }

		/// <summary>
		/// 获取或设置此lrc的歌词序列。
		/// </summary>
		public Queue<Lyric> Lyrics { get; set; }

		public string FileName { get; set; }
	}
}
