﻿using System;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{    
     /// <summary>
     /// 角色Unit  转移map服务器位置 （从这个map服务器发出去）
     /// </summary>
    [ActorMessageHandler(AppType.Map)]
	public class Actor_TransferHandler : AMActorRpcHandler<Unit, Actor_TransferRequest, Actor_TransferResponse>
	{
		protected override async Task Run(Unit unit, Actor_TransferRequest message, Action<Actor_TransferResponse> reply)
		{
			Actor_TransferResponse response = new Actor_TransferResponse();

			try
			{
				long unitId = unit.Id;

				// 先在location锁住unit的地址
				await Game.Scene.GetComponent<LocationProxyComponent>().Lock(unitId, unit.InstanceId);

				// 删除unit,让其它进程发送过来的消息找不到actor，重发
				Game.EventSystem.Remove(unitId);
				
				long instanceId = unit.InstanceId; //保存旧的map端的ID

                int mapIndex = message.MapIndex;//新的mapID

                StartConfigComponent startConfigComponent = StartConfigComponent.Instance;

				// 考虑AllServer情况
				if (startConfigComponent.Count == 1)
				{
					mapIndex = 0;
				}

				// 传送到map
				StartConfig mapConfig = startConfigComponent.MapConfigs[mapIndex]; //新的map地址
                IPEndPoint address = mapConfig.GetComponent<InnerConfig>().IPEndPoint; //新的map地址
                Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(address);//新的通讯

                // 只删除不disponse否则M2M_TrasferUnitRequest无法序列化Unit
                Game.Scene.GetComponent<UnitComponent>().RemoveNoDispose(unitId);
				M2M_TrasferUnitResponse m2m_TrasferUnitResponse = (M2M_TrasferUnitResponse)await session.Call(new M2M_TrasferUnitRequest() { Unit = unit });
				unit.Dispose();

				// 解锁unit的地址,并且更新unit的instanceId
				await Game.Scene.GetComponent<LocationProxyComponent>().UnLock(unitId, instanceId, m2m_TrasferUnitResponse.InstanceId);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}