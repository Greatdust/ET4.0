using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 将传递来的消息中的IP加锁
    /// </summary>
    [MessageHandler(AppType.Gate)]
	public class G2G_LockRequestHandler : AMRpcHandler<G2G_LockRequest, G2G_LockResponse>
	{
		protected override async void Run(Session session, G2G_LockRequest message, Action<G2G_LockResponse> reply)
		{
			G2G_LockResponse response = new G2G_LockResponse();
			try
			{
                //根据传递来的ID 得到这个服务器上的角色
                Unit unit = Game.Scene.GetComponent<UnitComponent>().Get(message.Id);
				if (unit == null)
				{
					response.Error = ErrorCode.ERR_NotFoundUnit;
					reply(response);
					return;
				}
                //把这个地址加锁
                await unit.GetComponent<MasterComponent>().Lock(NetworkHelper.ToIPEndPoint(message.Address));

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}