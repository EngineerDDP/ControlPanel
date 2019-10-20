using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Threading;
using Microsoft.Win32;

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
		/// 文件对话框
		/// </summary>
		private const String FileDialogTitle = "选择课件";
		private const String FileFilter = "PowerPoint 演示(*.ppt,*pptx)|*.ppt;*pptx";
		/// <summary>
		/// obs目录
		/// </summary>
		private readonly String OBSDirectory = System.Configuration.ConfigurationManager.AppSettings["OBSExecutableDir"];
		private readonly String OBSExecute = System.Configuration.ConfigurationManager.AppSettings["OBSExecute"];
		/// <summary>
		/// 画板目录
		/// </summary>
		private readonly String DBDirectory = System.Configuration.ConfigurationManager.AppSettings["DBDirectory"];
		private readonly String DBExecute = System.Configuration.ConfigurationManager.AppSettings["DBExecute"];

		private readonly String FocusServlet = System.Configuration.ConfigurationManager.AppSettings["GetFocusURL"];
		private readonly String StudentNumServlet = System.Configuration.ConfigurationManager.AppSettings["GetStudentNumServletURL"];
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
		private Process DrawingBroadProcess;
		/// <summary>
		/// 用于刷新消息的定时器
		/// </summary>
		private DispatcherTimer Timer;
		/// <summary>
		/// Web请求客户端
		/// </summary>
		private HttpClient Client;
		/// <summary>
		/// 显示教师个人信息
		/// </summary>
		private WebContainer WebPage;

		public MainWindow()
		{
			InitializeComponent();
			Timer = new DispatcherTimer();
			Client = new HttpClient();
			InsTextWindow = new BulletScreen();
			WebPage = new WebContainer();
			WebPage.Show();

			WebPage.OnStart += WebPage_OnStart;
			this.WindowStartupLocation = WindowStartupLocation.Manual;
			this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth - this.Width - MarginDis);
			this.Top = (System.Windows.SystemParameters.PrimaryScreenHeight - SpaceDis + MarginDis - this.Height);
			this.LocationChanged += MainWindow_LocationChanged;

            var pricipal = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent());
            if (pricipal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                RegistryKey registrybrowser = Registry.LocalMachine.OpenSubKey
                    (@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                string myProgramName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var currentValue = registrybrowser.GetValue(myProgramName);
                if (currentValue == null || (int)currentValue != 0x00002af9)
                    registrybrowser.SetValue(myProgramName, 0x00002af9, RegistryValueKind.DWord);
            }
        }

		private void WebPage_OnStart(object sender, object e)
		{
			this.Init();
		}

		private void Init()
		{
			this.Show();
			InsTextWindow.Show();
			this.Topmost = true;
			InsTextWindow.Topmost = true;
			UpdateChildFormLocation(InsTextWindow);

			WebPage.Close();


			OpenFileDialog picker = new OpenFileDialog();
			picker.Title = FileDialogTitle;
			picker.Filter = FileFilter;
			picker.Multiselect = false;
			Boolean stat = picker.ShowDialog() == true;
			if (stat)
			{
				System.Diagnostics.Process.Start(picker.FileName);

				InitOBS();
				InitDrawingBroad();
				InitTimer();
			}
			else
			{
				ExitProgram();
			}
		}
		/// <summary>
		/// 初始化计时器
		/// </summary>
		private void InitTimer()
		{
			Timer.Interval = TimeSpan.FromSeconds(1);
			Timer.Tick += Timer_Tick;
			Timer.Start();
		}
		/// <summary>
		/// 计时器计划
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Timer_Tick(object sender, EventArgs e)
		{
            try
            {
                var responseViewCount = await Client.GetAsync(StudentNumServlet);
                if (responseViewCount.StatusCode == System.Net.HttpStatusCode.OK)
                    Txt_ViewerCount.Text = await (responseViewCount).Content.ReadAsStringAsync();
            }
            catch { }

            try
            {
                var responseAttraction = await Client.GetAsync(FocusServlet);
                if (responseAttraction.StatusCode == System.Net.HttpStatusCode.OK)
                    Txt_Attraction.Text = await (responseAttraction).Content.ReadAsStringAsync();
            }
            catch { }
		}

		/// <summary>
		/// 配置画板进程
		/// </summary>
		private async Task InitDrawingBroad()
		{
			DrawingBroadProcess = new Process();
			DrawingBroadProcess.StartInfo.FileName = DBDirectory + DBExecute;
			DrawingBroadProcess.EnableRaisingEvents = true;

			DrawingBroadProcess.Start();
			await Task.Delay(500);
			DrawingBroadProcess.CloseMainWindow();
			DrawingBroadProcess.Exited += DrawingBroadProcess_Exited;
		}
		/// <summary>
		/// 处理画板退出消息
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DrawingBroadProcess_Exited(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// 初始化OBS进程
		/// </summary>
		private void InitOBS()
		{
			OBSProcess = new Process();
			OBSProcess.StartInfo.FileName = OBSExecute;
			OBSProcess.StartInfo.UseShellExecute = false;
			OBSProcess.StartInfo.RedirectStandardInput = true;
			OBSProcess.StartInfo.RedirectStandardOutput = true;
			OBSProcess.EnableRaisingEvents = true;


			System.IO.Directory.SetCurrentDirectory(OBSDirectory);

			OBSProcess.Start();
			OBSProcess.Exited += OBSProcess_Exited;

			PipeOut = OBSProcess.StandardInput;
			PipeIn = OBSProcess.StandardOutput;
		}
		/// <summary>
		/// 配置OBS退出事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OBSProcess_Exited(object sender, EventArgs e)
		{
			PipeIn = null;
			PipeOut = null;

			ExitProgram();
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
			ExitProgram();
		}
		/// <summary>
		/// 程序完整退出
		/// </summary>
		private void ExitProgram()
		{
			//关闭OBS
			if (OBSProcess != null && !OBSProcess.HasExited)
			{
				PipeOut.Close();
				PipeIn.Close();
				OBSProcess.CloseMainWindow();
			}
			if (DrawingBroadProcess != null && !DrawingBroadProcess.HasExited)
			{
				DrawingBroadProcess.CloseMainWindow();
			}

			this.Dispatcher.Invoke(() =>
			{
				//停止刷新
				Timer.Stop();
				//关闭弹幕
				InsTextWindow.Close();
				//退出
				this.Close();
			});
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
			if(DrawingBroadProcess != null && !DrawingBroadProcess.HasExited)
			{
				DrawingBroadProcess.CloseMainWindow();
			}
		}
		/// <summary>
		/// 即时画板场景
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Check_Switchsence_Unchecked(object sender, RoutedEventArgs e)
		{
			if(DrawingBroadProcess != null && DrawingBroadProcess.HasExited) 
			{
				DrawingBroadProcess.Start();
				Task.Delay(200).Wait();
			}
			if (PipeOut != null)
				PipeOut.WriteLine("SwitchSence");
		}
	}
}
