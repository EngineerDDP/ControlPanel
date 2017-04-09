using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ControlPanel
{
	/// <summary>
	/// BulletScreen.xaml 的交互逻辑
	/// </summary>
	public partial class BulletScreen : Window
	{
		/// <summary>
		/// 用于定时刷新的UI线程计时器
		/// </summary>
		private DispatcherTimer Timer;
		public BulletScreen()
		{
			InitializeComponent();

			Timer = new DispatcherTimer();
			Timer.Interval = TimeSpan.FromMilliseconds(500);
			Timer.Tick += Timer_Tick;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Timer Tick.");
		}
	}
}
