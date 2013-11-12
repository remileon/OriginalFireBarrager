using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OriginalFire.Darkness.Barrager.Core
{
	/// <summary>
	/// 表示设置项的集合，包含一系列键和值。
	/// </summary>
	public class Configurations : Dictionary<string, string>
	{
		/// <summary>
		/// 初始化Configurations类的新实例。
		/// </summary>
		public Configurations()
		{
		}

		/// <summary>
		/// 获取字符串值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个字符串，表示具有对应键的值。</returns>
		public string GetString(string key, string defaultValue)
		{
			return ContainsKey(key) ? this[key] : defaultValue;
		}

		/// <summary>
		/// 获取整数值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个整数，表示具有对应键的值。</returns>
		public int GetInt(string key, int defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			int n;
			if (!Int32.TryParse(this[key], out n))
				return defaultValue;
			return n;
		}

		/// <summary>
		/// 获取整数值，并限制值在指定大小范围内。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <param name="minValue">限制获取的最小值。</param>
		/// <param name="maxValue">限制获取的最大值。</param>
		/// <returns>一个整数，表示具有对应键的值。</returns>
		public int GetInt(string key, int defaultValue, int minValue, int maxValue)
		{
			int n = GetInt(key, defaultValue);
			if (n < minValue)
				return minValue;
			if (n > maxValue)
				return maxValue;
			return n;
		}

		/// <summary>
		/// 获取单精度浮点数值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个单精度浮点数，表示具有对应键的值。</returns>
		public float GetSingle(string key, float defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			float n;
			if (!Single.TryParse(this[key], out n))
				return defaultValue;
			return n;
		}

		/// <summary>
		/// 获取单精度浮点数值，并限制值在指定大小范围内。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <param name="minValue">限制获取的最小值。</param>
		/// <param name="maxValue">限制获取的最大值。</param>
		/// <returns>一个单精度浮点数，表示具有对应键的值。</returns>
		public float GetSingle(string key, float defaultValue, float minValue, float maxValue)
		{
			float n = GetSingle(key, defaultValue);
			if (n < minValue)
				return minValue;
			if (n > maxValue)
				return maxValue;
			return n;
		}

		/// <summary>
		/// 获取双精度浮点数值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个双精度浮点数，表示具有对应键的值。</returns>
		public double GetDouble(string key, double defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			double n;
			if (!Double.TryParse(this[key], out n))
				return defaultValue;
			return n;
		}

		/// <summary>
		/// 获取双精度浮点数值，并限制值在指定大小范围内。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <param name="minValue">限制获取的最小值。</param>
		/// <param name="maxValue">限制获取的最大值。</param>
		/// <returns>一个双精度浮点数，表示具有对应键的值。</returns>
		public double GetDouble(string key, double defaultValue, double minValue, double maxValue)
		{
			double n = GetDouble(key, defaultValue);
			if (n < minValue)
				return minValue;
			if (n > maxValue)
				return maxValue;
			return n;
		}

		/// <summary>
		/// 获取布尔值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个布尔值，表示具有对应键的值。</returns>
		public bool GetBoolean(string key, bool defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			string value = this[key];
			if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
				return true;
			if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
				return false;
			return defaultValue;
		}

		/// <summary>
		/// 获取“red, green, blue”形式字符串所表示的颜色值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个System.Drawing.Color，表示具有对应键的颜色。</returns>
		public Color GetRgbColor(string key, Color defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			try
			{
				string[] rgb = this[key].Split(commaSeparator);
				return Color.FromArgb(
					Byte.MaxValue,
					Byte.Parse(rgb[0]),
					Byte.Parse(rgb[1]),
					Byte.Parse(rgb[2])
				);
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// 获取“alpha, red, green, blue”形式字符串所表示的颜色值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个System.Drawing.Color，表示具有对应键的颜色。</returns>
		public Color GetArgbColor(string key, Color defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			try
			{
				string[] argb = this[key].Split(commaSeparator);
				return Color.FromArgb(
					Byte.Parse(argb[0]),
					Byte.Parse(argb[1]),
					Byte.Parse(argb[2]),
					Byte.Parse(argb[3])
				);
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// 获取按键设定值。
		/// </summary>
		/// <param name="key">待获取的值所对应的键。</param>
		/// <param name="defaultValue">当获取失败时，返回的默认值。</param>
		/// <returns>一个System.Windows.Forms.Keys，表示具有对应键的颜色。</returns>
		public Keys GetKeys(string key, Keys defaultValue)
		{
			if (!ContainsKey(key))
				return defaultValue;
			string[] keys = this[key].Split('+');
			Keys nk = System.Windows.Forms.Keys.None;
			Keys ak = System.Windows.Forms.Keys.None;
			foreach (string ks in keys)
			{
				Keys k;
				if (Enum.TryParse<Keys>(ks, true, out k))
				{
					if (k == System.Windows.Forms.Keys.Shift
						|| k == System.Windows.Forms.Keys.Control
						|| k == System.Windows.Forms.Keys.Alt)
						ak |= k;
					else
						nk = k;
				}
			}
			return nk | ak;
		}

		private static readonly char[] commaSeparator = { ',' };

		private static readonly char[] colonSeparator = { ':' };
	}
}
