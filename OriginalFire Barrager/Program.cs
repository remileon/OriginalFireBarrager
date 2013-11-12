using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using OriginalFire.Darkness.Barrager.Core;

namespace OriginalFire.Darkness.Barrager
{
	class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			bool createdNew;
			using (Mutex mutex = new Mutex(true, "OriginalFire Barrager", out createdNew))
			{
				if (!createdNew)
				{
					mutex.Close();
					MessageBox.Show("OriginalFire Barrager已在运行中。", "OriginalFire Barrager");
					return;
				}
				ConfigurationsCollection confs = new ConfigurationsCollection("config.ini");
				Application.Run(new BarrageWindow(confs));
			}
		}
	}
}
