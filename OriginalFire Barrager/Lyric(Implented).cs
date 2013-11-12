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
	class Lyric:IComparable
	{
		/// <summary>
		/// 从LRC文件中读取所有lyric，并按时间顺序排列。
		/// </summary>
		/// <param name="fileName">待读取的文件路径。</param>
		/// <returns>一个队列，包含待读取文件中的所有lyric。</returns>
		public static Queue<Lyric> LoadFromFile(string fileName)
		{
			StreamReader reader = null;
			try
			{
				reader = new StreamReader(fileName, Encoding.UTF8);
				Queue<Lyric> results = new Queue<Lyric>();
                ArrayList lines = new ArrayList();
                String readin = null;

                Regex time_tag = new Regex(@"\[(\d+):(\d+)\.(\d+)\]");
                Regex lyric_tag = new Regex(@"\[\d\d:\d\d\.\d\d\]+");
                Match time;
                string[] lrcOut = null;
                string tmp_tag = null;

                while ((readin = reader.ReadLine()) != null)
                {
                    time = time_tag.Match(readin);
                    while (time.Success)
                    {
                        tmp_tag = null;
                        lrcOut = null;
                        for (int i = 1; i < time.Groups.Count; i++)
                            tmp_tag += time.Groups[i];
                        lrcOut = lyric_tag.Split(readin);
                        Lyric item = new Lyric(tmp_tag, lrcOut[lrcOut.Length-1]);
                        lines.Add(item);
                        time = time.NextMatch();
                    }
                }
                lines.Sort();
                for (int i = 0; i < lines.Count; i++)
                    results.Enqueue((Lyric)lines[i]);
                    // 实现时删除本行
                    //throw new NotImplementedException();

                return results;
			}
			finally
			{
				if (reader != null)
					reader.Dispose();
			}
		}

		private Lyric()
		{
			// 用于在LoadFromFile中生成Lyric对象的构造方法，可以自行添加参数
            timeTag = null;
            content = null;
		}
        private Lyric(string _timeTag, string _content)
        {
            timeTag = _timeTag;
            content = _content;
        }

        /// <summary>
        /// 获得歌词时间戳的接口
        /// </summary>
        /// <returns>一行歌词的时间戳，类型为string</returns>
        public string GetTimeTag()
        {
            return timeTag;
        }

        /// <summary>
        /// 获得歌词内容的接口
        /// </summary>
        /// <returns>一行歌词，类型为string</returns>
        public string GetLyric()
        {
            return content;
        }

        /// <summary>
        /// 为了使用ArrayList的Sort()函数而重载的比较函数
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int CompareTo(Object b)
        {
            string b_tag = ((Lyric)b).timeTag;
            return this.timeTag.CompareTo(b_tag);
        }

		/// <summary>
		/// lrc格式中每一行的时间戳格式为[mm:ss.xx]，分别代表分钟，秒和百分之一秒
        /// 这里保存的是去掉了括号和标点后的时间戳
		/// </summary>
		private string timeTag;

		/// <summary>
		/// 此Lyric的内容。
		/// </summary>
		private string content;
	}

}
