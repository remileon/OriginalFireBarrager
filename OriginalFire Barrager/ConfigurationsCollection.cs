using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
	/// <summary>
	/// 表示多组设置项的集合。
	/// </summary>
	class ConfigurationsCollection
	{
		public ConfigurationsCollection(string fileName)
		{
			confs = new Dictionary<string,Configurations>();
			StreamReader reader = null;
			try
			{
				reader = new StreamReader(fileName);
				string firstLine = reader.ReadLine();

				string content = Regex.Replace(reader.ReadToEnd(), @"//.*?$|/\*.*?\*/", String.Empty,
					RegexOptions.Singleline | RegexOptions.Multiline);
				Configurations curConf = null;
				using (StringReader r = new StringReader(content))
				{
					while (r.Peek() > 0)
					{
						string line = r.ReadLine().Trim();
						if (line.Length == 0)
							continue;
						if (line.First() == '[' && line.Last() == ']')
						{
							string confName = line.Substring(1, line.Length - 2).Trim();
							if (!confs.ContainsKey(confName))
								confs.Add(confName, new Configurations());
							curConf = confs[confName];
						}
						string[] keyValue = line.Split(colonSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
						if (keyValue.Length < 2)
							continue;
						curConf[keyValue[0].Trim()] = keyValue[1].Trim();
					}
				}
			}
			catch (Exception e)
			{
				throw new ArgumentException("设置文件读取失败。", e);
			}
			finally
			{
				if (reader != null)
					reader.Dispose();
			}
		}

		public bool ContainsConfiguration(string key)
		{
			return confs.ContainsKey(key);
		}

		public Configurations this[string key]
		{
			get
			{
				if (confs.ContainsKey(key))
					return confs[key];
				else
					return new Configurations();
			}
		}

		private Dictionary<string, Configurations> confs;

		private static readonly char[] colonSeparator = { ':' };
	}
}
