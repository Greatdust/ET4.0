using System.Net;

namespace ETModel
{
	/// <summary>
	/// 用来与数据库操作代理
	/// </summary>
	public class DBProxyComponent: Component
	{
        /// <summary>
        /// 数据库地址
        /// </summary>
        public IPEndPoint dbAddress;
	}
}