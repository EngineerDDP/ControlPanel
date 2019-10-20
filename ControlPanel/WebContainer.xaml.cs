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
using System.Configuration;

namespace ControlPanel
{
	/// <summary>
	/// WebContainer.xaml 的交互逻辑
	/// </summary>
	public partial class WebContainer : Window
	{
		public event EventHandler<object> OnStart;
        private string StartStreamLinkTxt = System.Configuration.ConfigurationManager.AppSettings["StartStreamLinkTxt"];

        public WebContainer()
		{
			InitializeComponent();

			Web_Main.Source = new Uri(ConfigurationManager.AppSettings["TeacherInfoURL"]);
		}

		private void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			//if (e.Uri.ToString().EndsWith("#"))
				OnStart?.Invoke(this, e);
		}
	}
}
