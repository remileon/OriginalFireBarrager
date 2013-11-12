using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager.HttpInput
{
	/// <summary>
	/// 提供web方式添加弹幕评论的输入源。
	/// </summary>
	class HttpInput : CommentInput
	{
		public override void Initialize(Configurations conf)
		{
			listener = new HttpListener();
			listener.Prefixes.Add("http://*:" + conf.GetInt("Port", 80) + "/");
			Thread thread = new Thread(RecievingLoop);
			thread.Priority = ThreadPriority.Lowest;
			thread.Start();
		}

		public override void Dispose()
		{
			base.Dispose();
			listener.Close();
		}

		/// <summary>
		/// 侦听循环。
		/// </summary>
		private void RecievingLoop()
		{
			listener.Start();
			try
			{
				while (listener.IsListening)
				{
					HttpListenerContext context = listener.GetContext();
					HttpListenerRequest request = context.Request;
					HttpListenerResponse response = context.Response;
					if (request.HttpMethod.Equals("GET", StringComparison.CurrentCultureIgnoreCase))
					{
						// get方式请求页面。
						response.ContentType = "text/html";
						response.ContentEncoding = Encoding.UTF8;
						StreamWriter w = new StreamWriter(response.OutputStream, Encoding.UTF8);
						w.Write(Resource.page);
						w.Flush();
						w.Dispose();
						response.StatusCode = 200;
						response.Close();
					}
					else if (request.HttpMethod.Equals("POST", StringComparison.CurrentCultureIgnoreCase))
					{
						// post方式发送弹幕。
						StreamReader reader = new StreamReader(request.InputStream);
						string comment = reader.ReadLine();
						reader.Close();
						response.StatusCode = 200;
						response.Close();

						// 提交弹幕评论。
						SubmitComment(comment);
					}
					Thread.Sleep(0);
				}
			}
			catch (HttpListenerException e)
			{
				// 995为已关闭
				if (e.ErrorCode != 995)
					Debugger.Exception(e);
			}
			catch (Exception e)
			{
				Debugger.Exception(e);
			}
		}

		/// <summary>
		/// http侦听器。
		/// </summary>
		private HttpListener listener;
	}
}
