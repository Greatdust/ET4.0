using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 开始配置
    /// </summary>
    public class StartConfig: Entity
	{
		public int AppId { get; set; }

		[BsonRepresentation(BsonType.String)]
		public AppType AppType { get; set; } //服务器类型

        public string ServerIP { get; set; } //服务器IP
    }
}