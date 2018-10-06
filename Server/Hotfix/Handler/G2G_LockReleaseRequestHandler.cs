using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 解锁当前锁住的IP
    /// </summary>
    [MessageHandler(AppType.Gate)]
	public class G2G_LockReleaseRequestHandler : AMRpcHandler<G2G_LockReleaseRequest, G2G_LockReleaseResponse>
	{
		protected override void Run(Session session, G2G_LockReleaseRequest message, Action<G2G_LockReleaseResponse> reply)
		{
			G2G_LockReleaseResponse g2GLockReleaseResponse = new G2G_LockReleaseResponse();

			try
			{
                //根据传递来的ID 得到这个服务器上的角色
                Unit unit = Game.Scene.GetComponent<UnitComponent>().Get(message.Id);
				if (unit == null)
				{
					g2GLockReleaseResponse.Error = ErrorCode.ERR_NotFoundUnit;
					reply(g2GLockReleaseResponse);
					return;
				}
                //把当前加锁的IP解锁
                unit.GetComponent<MasterComponent>().Release(NetworkHelper.ToIPEndPoint(message.Address));
				reply(g2GLockReleaseResponse);
			}
			catch (Exception e)
			{
				ReplyError(g2GLockReleaseResponse, e, reply);
			}
		}
	}
}