using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ETModel;
using NLog;

namespace App
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// 异步方法全部会回掉到主线程
			SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
			
			try
			{			
				Game.EventSystem.Add(DLLType.Model, typeof(Game).Assembly);
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());
                //取出应用程序参数args 填充 Options 类
                Options options = Game.Scene.AddComponent<OptionComponent, string[]>(args).Options;
                 //读取配置文件
				StartConfig startConfig = Game.Scene.AddComponent<StartConfigComponent, string, int>(options.Config, options.AppId).StartConfig;

				if (!options.AppType.Is(startConfig.AppType))
				{
					Log.Error("命令行参数apptype与配置不一致");
					return;
				}

				IdGenerater.AppId = options.AppId;

				LogManager.Configuration.Variables["appType"] = startConfig.AppType.ToString();
				LogManager.Configuration.Variables["appId"] = startConfig.AppId.ToString();
				LogManager.Configuration.Variables["appTypeFormat"] = $"{startConfig.AppType,-8}";
				LogManager.Configuration.Variables["appIdFormat"] = $"{startConfig.AppId:D3}";

				Log.Info($"server start........................ {startConfig.AppId} {startConfig.AppType}");

				Game.Scene.AddComponent<OpcodeTypeComponent>();
				Game.Scene.AddComponent<MessageDispatherComponent>();

				// 根据不同的AppType添加不同的组件
				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				ClientConfig clientConfig = startConfig.GetComponent<ClientConfig>();
				
				switch (startConfig.AppType)
				{
					case AppType.Manager:
                        //连接客户端的外网和连接内部服务器的内网，对服务器进程进行管理，自动检测和启动服务器进程。
                        //加载有内网组件NetInnerComponent，外网组件NetOuterComponent，服务器进程管理组件。
                        //自动启动突然停止运行的服务器，保证此服务器管理的其它服务器崩溃后能及时自动启动运行。
                        Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						break;
					case AppType.Realm:
                        //对ActorMessage消息进行管理（添加、移除、分发等），连接内网和外网，对内网服务器进程进行操作，
                        //随机分配Gate服务器地址。加载有ActorMessage消息分发组件ActorMessageDispatherComponent，
                        //ActorManager消息管理组件ActorManagerComponent，内网组件NetInnerComponent，外网组件NetOuterComponent，
                        //服务器进程管理组件LocationProxyComponent，Gate服务器随机分发组件。
                        //客户端登录时连接的第一个服务器，也可称为登录服务器。
                        Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						break;
					case AppType.Gate:
                        //对玩家进行管理，对ActorMessage消息进行管理（添加、移除、分发等），连接内网和外网，
                        //对内网服务器进程进行操作，随机分配Gate服务器地址，对Actor消息进程进行管理，
                        //对玩家ID登录后的Key进行管理。加载有玩家管理组件PlayerComponent，ActorMessage消息分发组件ActorMessageDispatherComponent，
                        //ActorManager消息管理组件ActorManagerComponent，内网组件NetInnerComponent，外网组件NetOuterComponent，
                        //服务器进程管理组件LocationProxyComponent，Actor消息管理组件ActorProxyComponent，
                        //管理登陆时联网的Key组件GateSessionKeyComponent。对客户端的登录信息进行验证和客户端登录后连接的服务器，
                        //登录后通过此服务器进行消息互动，也可称为验证服务器。
                        Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorLocationSenderComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						break;
					case AppType.Location:
                        //连接内网，服务器进程状态集中管理（Actor消息IP管理服务器）。
                        //加载有内网组件NetInnerComponent，服务器消息处理状态存储组件LocationComponent。
                        Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<LocationComponent>();
						break;
					case AppType.Map:
                        //连接内网，对ActorMessage消息进行管理（添加、移除、分发等），
                        //对场景内现在活动物体存储管理，对内网服务器进程进行操作，对Actor消息进程进行管理，
                        //对ActorMessage消息进行管理（添加、移除、分发等），服务器帧率管理。
                        //ActorMessage消息分发组件ActorMessageDispatherComponent，ActorManager消息管理组件ActorManagerComponent，
                        //内网组件NetInnerComponent，服务器进程管理组件LocationProxyComponent，服务器帧率管理组件ServerFrameComponent。
                        Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorLocationSenderComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						break;
					case AppType.AllServer:
                        //将以上服务器功能集中合并成一个服务器。另外增加DB连接组件DBComponent，DB管理组件DBProxyComponent。
                        Game.Scene.AddComponent<ActorMessageSenderComponent>();
						Game.Scene.AddComponent<ActorLocationSenderComponent>();
						Game.Scene.AddComponent<PlayerComponent>();
						Game.Scene.AddComponent<UnitComponent>();
						Game.Scene.AddComponent<DBComponent>();
						Game.Scene.AddComponent<DBProxyComponent>();
						Game.Scene.AddComponent<DBCacheComponent>();
						Game.Scene.AddComponent<LocationComponent>();
						Game.Scene.AddComponent<ActorMessageDispatherComponent>();
						Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						Game.Scene.AddComponent<LocationProxyComponent>();
						Game.Scene.AddComponent<AppManagerComponent>();
						Game.Scene.AddComponent<RealmGateAddressComponent>();
						Game.Scene.AddComponent<GateSessionKeyComponent>();
						Game.Scene.AddComponent<ConfigComponent>();
						Game.Scene.AddComponent<ServerFrameComponent>();
						// Game.Scene.AddComponent<HttpComponent>();
						break;
					case AppType.Benchmark:
                        //连接内网和测试服务器承受力。加载有内网组件NetInnerComponent，服务器承受力测试组件BenchmarkComponent。
                        Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<BenchmarkComponent, string>(clientConfig.Address);
						break;
					case AppType.BenchmarkWebsocketServer:
						Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
						break;
					case AppType.BenchmarkWebsocketClient:
						Game.Scene.AddComponent<NetOuterComponent>();
						Game.Scene.AddComponent<WebSocketBenchmarkComponent, string>(clientConfig.Address);
						break;
					default:
						throw new Exception($"命令行参数没有设置正确的AppType: {startConfig.AppType}");
				}

				while (true)
				{
					try
					{
						Thread.Sleep(1);
						OneThreadSynchronizationContext.Instance.Update();
						Game.EventSystem.Update();
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
