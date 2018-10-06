namespace ETModel
{
	[ObjectSystem]
	public class GlobalConfigComponentAwakeSystem : AwakeSystem<GlobalConfigComponent>
	{
		public override void Awake(GlobalConfigComponent t)
		{
			t.Awake();
		}
	}

	public class GlobalConfigComponent : Component
	{
        //单例
		public static GlobalConfigComponent Instance;
        //服务器和AB包地址
		public GlobalProto GlobalProto;

        //会自动执行 得到地址
		public void Awake()
		{
			Instance = this;
			string configStr = ConfigHelper.GetGlobal();
			this.GlobalProto = JsonHelper.FromJson<GlobalProto>(configStr);
		}
	}
}