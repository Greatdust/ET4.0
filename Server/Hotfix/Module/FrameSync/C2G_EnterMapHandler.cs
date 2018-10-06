using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 进入map服务器，并创建角色
    /// </summary>
    [MessageHandler(AppType.Gate)]
	public class C2G_EnterMapHandler : AMRpcHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async void Run(Session session, C2G_EnterMap message, Action<G2C_EnterMap> reply)
		{
			G2C_EnterMap response = new G2C_EnterMap();
			try
			{
				Player player = session.GetComponent<SessionPlayerComponent>().Player;
				// 在map服务器上创建战斗Unit
				IPEndPoint mapAddress = StartConfigComponent.Instance.MapConfigs[0].GetComponent<InnerConfig>().IPEndPoint;
				Session mapSession = Game.Scene.GetComponent<NetInnerComponent>().Get(mapAddress);//和map服务器的会话通道
                M2G_CreateUnit createUnit = (M2G_CreateUnit)await mapSession.Call(new G2M_CreateUnit() { PlayerId = player.Id, GateSessionId = session.InstanceId });
				player.UnitId = createUnit.UnitId;
				response.UnitId = createUnit.UnitId;
				response.Count = createUnit.Count;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}