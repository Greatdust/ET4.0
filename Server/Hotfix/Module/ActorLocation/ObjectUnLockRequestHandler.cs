using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 对角色的ID和它所在的map服务器ID在位置服务器记录的解锁处理
    /// </summary>
    [MessageHandler(AppType.Location)]
	public class ObjectUnLockRequestHandler : AMRpcHandler<ObjectUnLockRequest, ObjectUnLockResponse>
	{
		protected override void Run(Session session, ObjectUnLockRequest message, Action<ObjectUnLockResponse> reply)
		{
			ObjectUnLockResponse response = new ObjectUnLockResponse();
			try
			{
				Game.Scene.GetComponent<LocationComponent>().UnLockAndUpdate(message.Key, message.OldInstanceId, message.InstanceId);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}