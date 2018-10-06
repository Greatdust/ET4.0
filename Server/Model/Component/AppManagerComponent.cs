using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ETModel
{
    /// <summary>
    /// 进程管理组件 防止服务器进程死掉 重启
    /// </summary>
	[ObjectSystem]
	public class AppManagerComponentAwakeSystem : AwakeSystem<AppManagerComponent>
	{
		public override void Awake(AppManagerComponent self)
		{
			self.Awake();
		}
	}

	public class AppManagerComponent: Component
	{
        /// <summary>
        /// 进程字典
        /// </summary>
        private readonly Dictionary<int, Process> processes = new Dictionary<int, Process>();

		public void Awake()
        {
            //获得本地IP
            string[] ips = NetHelper.GetAddressIPs();
			StartConfig[] startConfigs = StartConfigComponent.Instance.GetAll();
			
			foreach (StartConfig startConfig in startConfigs) //遍历所有的服务器配置
            {
				Game.Scene.GetComponent<TimerComponent>().WaitAsync(100);
                //如果配置文件中没有本机的IP 且 IP是*号就跳出 不启动进程  说明这个服务器不在本机上
                if (!ips.Contains(startConfig.ServerIP) && startConfig.ServerIP != "*")
				{
					continue;
				}

				if (startConfig.AppType.Is(AppType.Manager))  //不能是自己
                {
					continue;
				}

				StartProcess(startConfig.AppId);  //启动
            }

			this.WatchProcessAsync();
		}
        /// <summary>
        /// 启动线程
        /// </summary>
        /// <param name="appId"></param>
        private void StartProcess(int appId)
		{
            //APP启动参数
            OptionComponent optionComponent = Game.Scene.GetComponent<OptionComponent>();
			StartConfigComponent startConfigComponent = StartConfigComponent.Instance;
			string configFile = optionComponent.Options.Config; //得到配置文件地址
            StartConfig startConfig = startConfigComponent.Get(appId); //得到指定APPid的启动配置文件信息
            const string exe = "dotnet";
			string arguments = $"App.dll --appId={startConfig.AppId} --appType={startConfig.AppType} --config={configFile}";

			Log.Info($"{exe} {arguments}");
			try
			{
				bool useShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                ////启动信息
                ProcessStartInfo info = new ProcessStartInfo { FileName = exe, Arguments = arguments, CreateNoWindow = true, UseShellExecute = useShellExecute };
                //启动进程
                Process process = Process.Start(info);
                //加载到字典中
                this.processes.Add(startConfig.AppId, process);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		/// <summary>
		/// 监控启动的进程,如果进程挂掉了,重新拉起
		/// </summary>
		private async void WatchProcessAsync()
		{
			long instanceId = this.InstanceId;
			
			while (true)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(5000);//延时检测

                if (this.InstanceId != instanceId) 
                {
					return;
				}

				foreach (int appId in this.processes.Keys.ToArray())
				{
					Process process = this.processes[appId];
					if (!process.HasExited) //如果进程没有终止 退出
                    {
						continue;
					}
					this.processes.Remove(appId);   //进程终止了 移除它
                    process.Dispose();
					this.StartProcess(appId); //重新启动线程
                }
			}
		}
	}
}