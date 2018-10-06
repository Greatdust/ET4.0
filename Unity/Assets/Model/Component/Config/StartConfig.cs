using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 服务器启动配置
    /// </summary>
	public class StartConfig: Entity
	{
        /// <summary>
        /// AppId
        /// </summary>
		public int AppId { get; set; }
        /// <summary>
        /// 服务器类型
        /// </summary>
		[BsonRepresentation(BsonType.String)]
		public AppType AppType { get; set; }
        /// <summary>
        /// 服务器IP
        /// </summary>
		public string ServerIP { get; set; }
	}
}