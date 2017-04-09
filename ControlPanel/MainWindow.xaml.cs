using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlPanel
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// 设置间距
		/// </summary>
		private const Int32 MarginDis = 5;
		private const Int32 SpaceDis = 50;
		/// <summary>
		/// obs目录
		/// </summary>
		private const String OBSDirectory = @"C:\Users\ZZADD\Documents\Visual Studio 2015\Projects\ObsVSBuild\rundir\Release\bin\64bit\";
		private const String OBSExecute = @"obs64.exe";
		/// <summary>
		/// OBS进程句柄
		/// </summary>
		private Process OBSProcess;
		/// <summary>
		/// 输出匿名管道流
		/// </summary>
		private StreamWriter PipeOut;
		/// <summary>
		/// 输入匿名管道流
		/// </summary>
		private StreamReader PipeIn;
		/// <summary>
		/// 设置弹幕子窗口
		/// </summary>
		private Window InsTextWindow;
		/// <summary>
		/// 设置画板子进程
		/// </summary>
		private Process DrawBroadProcess;

		public MainWindow()
		{
			InitializeComponent();

			this.WindowStartupLocation = WindowStartupLocation.Manual;
			this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth - this.Width - MarginDis);
			this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight - SpaceDis + MarginDis - this.Height);
			this.LocationChanged += MainWindow_LocationChanged;

			InsTextWindow = new BulletScreen();
			UpdateChildFormLocation(InsTextWindow);
			InsTextWindow.Show();

			//InitOBS();
		}
		/// <summary>
		/// 初始化OBS进程
		/// </summary>
		private void InitOBS()
		{
			OBSProcess.StartInfo.FileName = OBSDirectory + OBSExecute;
			OBSProcess.StartInfo.UseShellExecute = false;
			OBSProcess.StartInfo.RedirectStandardInput = true;
			OBSProcess.StartInfo.RedirectStandardOutput = true;

			System.IO.Directory.SetCurrentDirectory(OBSDirectory);

			OBSProcess.Start();

			PipeOut = OBSProcess.StandardInput;
			PipeIn = OBSProcess.StandardOutput;
		}
		/// <summary>
		/// 父窗口位置刷新
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_LocationChanged(object sender, EventArgs e)
		{
			UpdateChildFormLocation(InsTextWindow);
		}
		/// <summary>
		/// 更新跟随窗口位置
		/// </summary>
		/// <param name="obj"></param>
		private void UpdateChildFormLocation(Window obj)
		{
			obj.Left = this.Left;
			obj.Top = this.Top - InsTextWindow.Height + MarginDis;
		}
		/// <summary>
		/// 处理窗口拖动
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			base.DragMove();

		}
		/// <summary>
		/// 关闭窗口以及关联的子窗口
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Btn_Close_Click(object sender, RoutedEventArgs e)
		{
			//关闭OBS
			PipeOut.Close();
			PipeIn.Close();
			OBSProcess.CloseMainWindow();
			OBSProcess.WaitForExit(1000);
			if (!OBSProcess.HasExited)
				OBSProcess.Kill();

			//关闭弹幕
			InsTextWindow.Close();
			//退出
			this.Close();
		}

		private void Check_TxtPop_Checked(object sender, RoutedEventArgs e)
		{
			if (InsTextWindow != null && !InsTextWindow.IsVisible)
				InsTextWindow.Show();
		}

		private void Check_TxtPop_Unchecked(object sender, RoutedEventArgs e)
		{
			if (InsTextWindow != null && InsTextWindow.IsVisible)
				InsTextWindow.Hide();
		}
		/// <summary>
		/// PPT场景
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Check_Switchsence_Checked(object sender, RoutedEventArgs e)
		{
			if(PipeOut != null)
				PipeOut.WriteLine("SwitchSence");
		}
		/// <summary>
		/// 即时画板场景
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Check_Switchsence_Unchecked(object sender, RoutedEventArgs e)
		{
			if(PipeOut != null)
				PipeOut.WriteLine("SwitchSence");
			
		}
	}
}
