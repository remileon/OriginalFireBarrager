using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager.UdpInput
{
	class UdpInput : CommentInput
	{
		public override void Initialize(Configurations conf)
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPAddress address;
			if (!IPAddress.TryParse(conf.GetString("Address", "127.0.0.1"), out address))
				address = IPAddress.Loopback;
			socket.Bind(new IPEndPoint(address, conf.GetInt("Port", 10888)));
			Thread thread = new Thread(RecievingLoop);
			thread.Priority = ThreadPriority.Lowest;
			thread.Start();
		}

		/// <summary>
		/// 释放此UdpInput所持有的资源。
		/// </summary>
		public override void Dispose()
		{
			socket.Close();
		}

		/// <summary>
		/// 接收UDP数据报循环。
		/// </summary>
		private void RecievingLoop()
		{
			try
			{
				byte[] buffer = new byte[1024];
				while (true)
				{
					EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
					int length = socket.ReceiveFrom(buffer, ref endPoint);
					try
					{
						SubmitComment(Encoding.Unicode.GetString(buffer, 0, length));
					}
					catch
					{
					}
					Thread.Sleep(0);
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// 此UdpInput的Socket。
		/// </summary>
		private Socket socket;
	}
}
