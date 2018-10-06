namespace ETModel
{
    /// <summary>
    /// 场景类型
    /// </summary>
	public enum SceneType
	{
		Share,
		Game,
		Login,
		Lobby,
		Map,
		Launcher,
		Robot,
		BehaviorTreeScene,
		RobotClient,

		Realm
	}
	
    /// <summary>
    /// 场景组件
    /// </summary>
	public sealed class Scene: Entity
	{
		public string Name { get; set; }

		public Scene()
		{
			this.InstanceId = IdGenerater.GenerateId();
		}

		public Scene(long id): base(id)
		{
			this.InstanceId = IdGenerater.GenerateId();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}