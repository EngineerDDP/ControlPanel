using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlPanel.Json
{
	public class Setting
	{
		public String key
		{
			get; set;
		}
		public String server
		{
			get; set;
		}
	}
	public class OBSService
	{
		public Setting settings
		{
			get;set;
		}
		public String type
		{
			get;set;
		}
	}
}
