namespace ETModel
{
	[ObjectSystem]
	public class PlayerSystem : AwakeSystem<Player, string>
	{
		public override void Awake(Player self, string a)
		{
			self.Awake(a);
		}
	}
    /// <summary>
    /// 用户
    /// </summary>
    public sealed class Player : Entity
    {
        /// <summary>
        /// 帐号
        /// </summary>
		public string Account { get; private set; }
        /// <summary>
        /// 角色单元
        /// </summary>
        public long UnitId { get; set; }

		public void Awake(string account)
		{
			this.Account = account;
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