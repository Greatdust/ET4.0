using System;
using System.IO;
using System.Threading;
using Google.Protobuf;
using UnityEngine;

namespace ETModel
{
	public class Init : MonoBehaviour
	{

        private async void Start()
		{
           
            try
			{ 
				if (!Application.unityVersion.StartsWith("2017.4"))
				{
					Log.Error($"新人请使用Unity2017.4版本,减少跑demo遇到的问题! 下载地址:\n https://unity3d.com/cn/unity/qa/lts-releases?_ga=2.227583646.282345691.1536717255-1119432033.1499739574");
				}

                //线程同步队列，对socket的回调进行排序调用
                SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

				DontDestroyOnLoad(gameObject); //这个类会一直保留
                
                //通过反射机制，获取特性得到DLL里面的类
                Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

                //全局配置组件 （热更新资源地址，服务器ID端口）
                Game.Scene.AddComponent<GlobalConfigComponent>();

                //网络通信组件（和服务器通信）
                Game.Scene.AddComponent<NetOuterComponent>();

                //资源管理组件 （热更资源的管理,AB包）
                Game.Scene.AddComponent<ResourcesComponent>();

                //玩家组建 DEMO使用的
                Game.Scene.AddComponent<PlayerComponent>();

                //DEMO使用的
                Game.Scene.AddComponent<UnitComponent>();

                //帧同步组件
                Game.Scene.AddComponent<ClientFrameComponent>();

                //UI组件
                Game.Scene.AddComponent<UIComponent>();

				// 下载ab包
				await BundleHelper.DownloadBundle();

                //加载热更新资源
                Game.Hotfix.LoadHotfixAssembly();

				// 加载配置
				Game.Scene.GetComponent<ResourcesComponent>().LoadBundle("config.unity3d");
				Game.Scene.AddComponent<ConfigComponent>();
				Game.Scene.GetComponent<ResourcesComponent>().UnloadBundle("config.unity3d");

                //协议类型组件
                Game.Scene.AddComponent<OpcodeTypeComponent>();

                //消息分发
                Game.Scene.AddComponent<MessageDispatherComponent>();

                //执行热更层的代码
                Game.Hotfix.GotoHotfix();

				Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private void Update()
		{
			OneThreadSynchronizationContext.Instance.Update();
			Game.Hotfix.Update?.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			Game.Hotfix.LateUpdate?.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Hotfix.OnApplicationQuit?.Invoke();
			Game.Close();
		}
	}
}