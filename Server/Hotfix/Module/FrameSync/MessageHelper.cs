using ETModel;

namespace ETHotfix
{
	public static class MessageHelper
	{
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="message"></param>
        public static void Broadcast(IActorMessage message)
		{
            //获得角色管理组件中所有的角色
            Unit[] units = Game.Scene.GetComponent<UnitComponent>().GetAll();
            //获得Actor会话管理组件
            ActorMessageSenderComponent actorLocationSenderComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
			foreach (Unit unit in units) //遍历所有的角色
            {
				UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();  //获取每个角色的会话通道
                if (unitGateComponent.IsDisconnect)
				{
					continue;
				}
                //发送消息
                ActorMessageSender actorMessageSender = actorLocationSenderComponent.Get(unitGateComponent.GateSessionActorId);
				actorMessageSender.Send(message);
			}
		}
	}
}
