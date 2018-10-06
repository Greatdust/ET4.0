using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 内网IP配置信息
    /// </summary>
	[BsonIgnoreExtraElements]
	public class InnerConfig: AConfigComponent
	{  
        /// <summary>
        /// IP地址加端口
        /// </summary>
		[BsonIgnore]
		public IPEndPoint IPEndPoint { get; private set; }
		/// <summary>
        /// IP地址
        /// </summary>
		public string Address { get; set; }

		public override void EndInit()
		{
			this.IPEndPoint = NetworkHelper.ToIPEndPoint(this.Address);
		}
	}
}