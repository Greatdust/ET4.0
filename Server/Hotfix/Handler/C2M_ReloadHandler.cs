using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 管理一个服务器进行热更新处理（给需要热更新的服务器发送热更命令）
    /// </summary>
    [MessageHandler(AppType.Manager)]
	public class C2M_ReloadHandler: AMRpcHandler<C2M_Reload, M2C_Reload>//C2M_Reload发送的类型  M2C_Reload回复的类型
    {
		protected override async void Run(Session session, C2M_Reload message, Action<M2C_Reload> reply)
		{
			M2C_Reload response = new M2C_Reload();
			if (message.Account != "panda" && message.Password != "panda")
			{
				Log.Error($"error reload account and password: {MongoHelper.ToJson(message)}");
				return;
			}
			try
			{
                //获得启动配置
                StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
                //拿到内网通讯组件
                NetInnerComponent netInnerComponent = Game.Scene.GetComponent<NetInnerComponent>();
                //遍历所有的服务器配置
                foreach (StartConfig startConfig in startConfigComponent.GetAll())
				{
					InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();  //拿到当前服务器的ip端口
                    Session serverSession = netInnerComponent.Get(innerConfig.IPEndPoint);  //得到内部会话serverSession
                    await serverSession.Call(new M2A_Reload()); //发送RPC消息等待完成
                }
				reply(response);    //回复RPC消息
            }
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}