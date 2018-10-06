using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DbComponentSystem : AwakeSystem<DBComponent>
	{
		public override void Awake(DBComponent self)
		{
			self.Awake();
		}
	}

	/// <summary>
	/// 连接mongodb
	/// </summary>
	public class DBComponent : Component
	{
        /// <summary>
        /// 数据库客户端
        /// </summary>
        public MongoClient mongoClient;
        /// <summary>
        /// 数据库
        /// </summary>
        public IMongoDatabase database;

		public void Awake()
		{
            //获取数据库地址
            //DBConfig config = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.GetComponent<DBConfig>();
            //保存数据库地址
            //string connectionString = config.ConnectionString;
            //建立与数据库连接
            //mongoClient = new MongoClient(connectionString);
            //得到指定名字的数据库，如果数据库存在则直接返回，否则就创建该数据库并返回
            //this.database = this.mongoClient.GetDatabase(config.DBName);
        }
        /// <summary>
        /// 获取数据库中的表
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IMongoCollection<ComponentWithId> GetCollection(string name)
		{
			return this.database.GetCollection<ComponentWithId>(name);
		}
	}
}