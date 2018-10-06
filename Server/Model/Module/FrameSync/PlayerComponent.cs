using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	[ObjectSystem]
	public class PlayerComponentSystem : AwakeSystem<PlayerComponent>
	{
		public override void Awake(PlayerComponent self)
		{
			self.Awake();
		}
	}
    /// <summary>
    /// 用户管理组件
    /// </summary>
    public class PlayerComponent : Component
	{
		public static PlayerComponent Instance { get; private set; }

		public Player MyPlayer;
        /// <summary>
        /// 用户字典
        /// </summary>
        private readonly Dictionary<long, Player> idPlayers = new Dictionary<long, Player>();

		public void Awake()
		{
			Instance = this;
		}
        /// <summary>
        /// 添加用户到字典中
        /// </summary>
        /// <param name="player"></param>
        public void Add(Player player)
		{
			this.idPlayers.Add(player.Id, player);
		}
        /// <summary>
        /// 通过ID取得用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Player Get(long id)
		{
			this.idPlayers.TryGetValue(id, out Player gamer);
			return gamer;
		}
        /// <summary>
        /// 移除用户
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
		{
			this.idPlayers.Remove(id);
		}
        /// <summary>
        /// 所保存的用户数量
        /// </summary>
        public int Count
		{
			get
			{
				return this.idPlayers.Count;
			}
		}
        /// <summary>
        /// 获得全部用户
        /// </summary>
        /// <returns></returns>
        public Player[] GetAll()
		{
			return this.idPlayers.Values.ToArray();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (Player player in this.idPlayers.Values)
			{
				player.Dispose();
			}

			Instance = null;
		}
	}
}