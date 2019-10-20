using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ControlPanel.Json
{
	class DanMu
	{
		/// <summary>
		/// 用户名
		/// </summary>
		public String UserName
		{
			get;set;
		}
		/// <summary>
		/// 时间戳
		/// </summary>
		public long TimeTag
		{
			get;set;
		}
		/// <summary>
		/// 消息内容
		/// </summary>
		public String Message
		{
			get;set;
		}
	}
}
