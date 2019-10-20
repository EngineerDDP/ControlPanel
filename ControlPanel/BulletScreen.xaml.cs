using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace ControlPanel
{
	/// <summary>
	/// BulletScreen.xaml 的交互逻辑
	/// </summary>
	public partial class BulletScreen : Window
	{
		private readonly String GetMsgUrl = System.Configuration.ConfigurationManager.AppSettings["GetMessageURL"];
		/// <summary>
		/// 用于标记本地最旧消息事件戳
		/// </summary>
		private Int64 Update;
		/// <summary>
		/// 用于Http请求的客户端
		/// </summary>
		private HttpClient Client;
		/// <summary>
		/// 用于定时刷新的UI线程计时器
		/// </summary>
		private DispatcherTimer Timer;
		public BulletScreen()
		{
			InitializeComponent();
			Client = new HttpClient();

			Timer = new DispatcherTimer();
			Timer.Interval = TimeSpan.FromSeconds(2);
			Timer.Tick += Timer_Tick;
			Update = 0;

			Timer.Start();
		}

		private async void Timer_Tick(object sender, EventArgs e)
		{
			ControlPanel.Style.TextMessage text = new ControlPanel.Style.TextMessage();
			DateTime index = new DateTime(1970, 1, 1, 0, 0, 0);
			ControlPanel.Style.TextMessage item = null;

            try
            {
                var response = await Client.GetAsync(GetMsgUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    String json = await response.Content.ReadAsStringAsync();
                    List<Json.DanMu> list = JsonConvert.DeserializeObject<List<Json.DanMu>>(json);

                    if (list != null)
                        foreach (Json.DanMu i in list)
                        {
                            if (i.TimeTag > Update)
                            {
                                item = new ControlPanel.Style.TextMessage();
                                item.Txt_Tag.Text = i.UserName + "  " + (index + TimeSpan.FromMilliseconds(i.TimeTag)).ToShortTimeString();
                                item.Txt_Content.Text = i.Message;
                                List_Message.Items.Add(item);
                                if (List_Message.Items.Count > 10)
                                    List_Message.Items.RemoveAt(0);
                            }
                        }

                    if (item != null)
                        List_Message.ScrollIntoView(item);
                }
                Timer.Interval = TimeSpan.FromSeconds(2);
            }
            catch(HttpRequestException he)
            {
                System.Diagnostics.Debug.WriteLine(he);
                Timer.Stop();
                Timer.Interval = TimeSpan.FromSeconds(Timer.Interval.Seconds + 2);
                Timer.Start();
            }
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Timer.Stop();
		}
	}
}
