using System.Collections.Generic;
using System.Net;

namespace ETModel
{
	public static class NetHelper
	{

        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        /// <returns>The address I ps.</returns>
        public static string[] GetAddressIPs()
		{
			//获取本地的IP地址
			List<string> addressIPs = new List<string>();
			foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (address.AddressFamily.ToString() == "InterNetwork")
				{
					addressIPs.Add(address.ToString());
				}
			}
			return addressIPs.ToArray();
		}
	}
}
