using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 客户端配置
    /// </summary>
	public class ClientConfig: AConfigComponent
	{
        /// <summary>
        /// 客户端IP地址
        /// </summary>
		public string Address { get; set; }
	}
}