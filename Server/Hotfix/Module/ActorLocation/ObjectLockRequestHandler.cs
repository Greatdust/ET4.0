using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 对一个实体的位置进行加锁 ，在加锁期间 ，申请位置的请求是不会立即返回 ，会先缓存，待解锁（时间到）后会应答缓存的位置请求
    /// </summary>
    [MessageHandler(AppType.Location)]
	public class ObjectLockRequestHandler : AMRpcHandler<ObjectLockRequest, ObjectLockResponse>
	{
		protected override void Run(Session session, ObjectLockRequest message, Action<ObjectLockResponse> reply)
		{
			ObjectLockResponse response = new ObjectLockResponse();
			try
			{
                //对角色的ID  MAP服务器ID 进行加锁  并制定加锁时间
                Game.Scene.GetComponent<LocationComponent>().Lock(message.Key, message.InstanceId, message.Time);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}