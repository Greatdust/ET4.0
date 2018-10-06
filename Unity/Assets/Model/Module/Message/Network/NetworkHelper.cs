using System.Net;

namespace ETModel
{
	public static class NetworkHelper
	{
        /// <summary>
        /// 把字符串转换成IP格式
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
		public static IPEndPoint ToIPEndPoint(string host, int port)
		{
			return new IPEndPoint(IPAddress.Parse(host), port);
		}
        /// <summary>
        ///  把字符串转换成IP格式
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
		public static IPEndPoint ToIPEndPoint(string address)
		{
			int index = address.LastIndexOf(':');
			string host = address.Substring(0, index);
			string p = address.Substring(index + 1);
			int port = int.Parse(p);
			return ToIPEndPoint(host, port);
		}
	}
}
