using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 启动服务器配置
    /// </summary>
	[BsonIgnoreExtraElements]
	public class RunServerConfig: AConfigComponent
	{
		public string IP = "";
	}
}