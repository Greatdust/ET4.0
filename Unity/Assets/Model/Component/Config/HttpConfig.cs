using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// Http配置信息
    /// </summary>
	[BsonIgnoreExtraElements]
	public class HttpConfig: AConfigComponent
	{
		public string Url { get; set; } = "";
		public int AppId { get; set; }
		public string AppKey { get; set; } = "";
		public string ManagerSystemUrl { get; set; } = "";
	}
}