namespace ETModel
{
	public static class Game
	{
        /// <summary>
        /// 场景组件
        /// </summary>
		private static Scene scene;
        /// <summary>
        /// 场景组件实例
        /// </summary>
        public static Scene Scene
		{
			get
			{
				if (scene != null)
				{
					return scene;
				}
				scene = new Scene();
				scene.AddComponent<TimerComponent>();
				return scene;
			}
		}

        /// <summary>
		/// 事件系统
		/// </summary>
		private static EventSystem eventSystem;

        /// <summary>
        /// 返回一个事件系统实例 
        /// </summary>
        /// <value>The event system.</value>
        public static EventSystem EventSystem
		{
			get
			{
				return eventSystem ?? (eventSystem = new EventSystem());
			}
		}

        #region 组件 pool

        /// <summary>
        ///  组件 pool.
        /// </summary>
        private static ObjectPool objectPool;
        /// <summary>
        ///  组件 pool.
        /// </summary>
		public static ObjectPool ObjectPool
        {
            get
            {
                return objectPool ?? (objectPool = new ObjectPool());
            }
        } 
        #endregion

        private static Hotfix hotfix;

		public static Hotfix Hotfix
		{
			get
			{
				return hotfix ?? (hotfix = new Hotfix());
			}
		}

		public static void Close()
		{
			scene.Dispose();
			eventSystem = null;
			scene = null;
			objectPool = null;
			hotfix = null;
		}
	}
}