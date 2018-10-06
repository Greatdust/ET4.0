using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class SessionPlayerComponentDestroySystem : DestroySystem<SessionPlayerComponent>
	{
		public override void Destroy(SessionPlayerComponent self)
		{
            // 发送断线消息 根据账号里的角色单元ID  获得 与MAP服务器通讯的 会话ActorMessageSender
            ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(self.Player.UnitId);
			actorLocationSender.Send(new G2M_SessionDisconnect());   //给map服务器发送断线消息
            Game.Scene.GetComponent<PlayerComponent>()?.Remove(self.Player.Id); //移除用户管理中的账号
        }
	}
}