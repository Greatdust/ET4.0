using System.Net;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 外网网IP配置信息
    /// </summary>
    [BsonIgnoreExtraElements]
	public class OuterConfig: AConfigComponent
	{
        /// <summary>
        /// IP地址1
        /// </summary>
		public string Address { get; set; }
        /// <summary>
        /// IP地址2
        /// </summary>
        public string Address2 { get; set; }
	}
}