using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Text;
using ControlPanel.Json;

namespace ControlPanel
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private readonly String ProfileSettingDir = System.Configuration.ConfigurationManager.AppSettings["OBSProfileDir"];
		private readonly String SceneSettingDir = System.Configuration.ConfigurationManager.AppSettings["OBSSceneSettingDir"];

		private const String BakFile = ".bak";
		private const String JsonFile = ".json";
		private void Application_Startup(object sender, StartupEventArgs e)
		{

			if (e.Args.Length != 0)
			{
				String ProfileServiceFile = ProfileSettingDir + System.Configuration.ConfigurationManager.AppSettings["ProfileServiceFileName"];

				StreamReader sr = new StreamReader(ProfileServiceFile, Encoding.Default);
				String json = sr.ReadToEnd();
				sr.Close();
				using (FileStream fs = new FileStream(ProfileServiceFile, FileMode.Create))
				{
					StreamWriter sw = new StreamWriter(fs, Encoding.Default);

					OBSService service = JsonConvert.DeserializeObject<OBSService>(json);
					service.settings.key = e.Args[0];
					json = JsonConvert.SerializeObject(service);
					sw.Write(json);
					sw.Close();
				}

			}
			String SceneSettingFile = SceneSettingDir + System.Configuration.ConfigurationManager.AppSettings["SceneSettingFileName"];

			////替换文件
			//File.Delete(SceneSettingFile + JsonFile);
			//File.Copy(SceneSettingFile + BakFile, SceneSettingFile + JsonFile);

			MainWindow main = new ControlPanel.MainWindow();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{

		}
	}

}
