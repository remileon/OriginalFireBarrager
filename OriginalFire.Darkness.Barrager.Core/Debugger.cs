using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OriginalFire.Darkness.Barrager.Core
{
	/// <summary>
	/// 用于调试的消息输出
	/// </summary>
	public static class Debugger
	{
		static Debugger()
		{
#if (DEBUG)
#else
			writer = new StreamWriter("debugger.log");
			System.Windows.Forms.Application.ApplicationExit += (sender, e) =>
			{
				writer.Flush();
				writer.Dispose();
			};
#endif
		}

		/// <summary>
		/// 记录一行内容。
		/// </summary>
		/// <param name="content">待记录的内容。</param>
		public static void Log(string content)
		{
#if (DEBUG)
			Console.WriteLine(DateTime.Now.ToString() + ": " + content);
#else
			writer.WriteLine(DateTime.Now.ToString() + ": " + content);
			writer.Flush();
#endif
		}

		/// <summary>
		/// 记录一个异常。
		/// </summary>
		/// <param name="e">待记录的异常。</param>
		public static void Exception(Exception e)
		{
#if (DEBUG)
			Console.WriteLine(DateTime.Now.ToString() + ": ");
			Console.WriteLine(e.ToString());
			Console.WriteLine("-------------------------------------");
#else
			writer.WriteLine(DateTime.Now.ToString() + ": ");
			writer.WriteLine(e.ToString());
			writer.WriteLine("-------------------------------------");
			writer.Flush();
#endif
		}
#if (DEBUG)
#else
		private static StreamWriter writer;
#endif
	}
}
