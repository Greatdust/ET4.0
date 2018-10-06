using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// DB数据库配置
    /// </summary>
	[BsonIgnoreExtraElements]
	public class DBConfig : AConfigComponent
	{
        /// <summary>
        /// 数据库连接地址
        /// </summary>
		public string ConnectionString { get; set; }
        /// <summary>
        /// 数据库名字
        /// </summary>
		public string DBName { get; set; }
	}
}