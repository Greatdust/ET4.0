using System;
using System.Net;

namespace ETModel
{
    /// <summary>
    /// Actor消息会话管理组件
    /// </summary>
    public class ActorMessageSenderComponent: Component
	{
        /// <summary>
        /// 根据ID 从字典得到一个会话ActorMessageSender 如果没有会新建
        /// </summary>
        /// <param name="id">角色ID 会根据这个冲位置服务器得到服务器ID</param>
        /// <returns></returns>
        public ActorMessageSender Get(long actorId)
		{
			if (actorId == 0)
			{
				throw new Exception($"actor id is 0");
			}
            //这里没有指定actorId 那么在初始化ActorMessageSender 会从位置服务器查找actorIdID 对其赋值
            IPEndPoint ipEndPoint = StartConfigComponent.Instance.GetInnerAddress(IdGenerater.GetAppIdFromId(actorId));
			ActorMessageSender actorMessageSender = new ActorMessageSender(actorId, ipEndPoint);
			return actorMessageSender;
		}
	}
}
