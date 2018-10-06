namespace ETModel
{
	[ObjectSystem]
	public class UnitGateComponentAwakeSystem : AwakeSystem<UnitGateComponent, long>
	{
		public override void Awake(UnitGateComponent self, long a)
		{
			self.Awake(a);
		}
	}
    /// <summary>
    /// 挂载了这个组件 就能获得对应的MAP服务器的会话通道ActorMessageSender
    /// </summary>
    public class UnitGateComponent : Component, ISerializeToEntity
	{
        /// <summary>
        /// Gate服务器中这个会话的ActorId（对应的MAP服务器的ID）
        /// </summary>
        public long GateSessionActorId;

		public bool IsDisconnect;

		public void Awake(long gateSessionId)
		{
			this.GateSessionActorId = gateSessionId;
		}
	}
}