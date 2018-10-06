using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 热更新处理
    /// </summary>
    [MessageHandler(AppType.AllServer)]
	public class M2A_ReloadHandler : AMRpcHandler<M2A_Reload, A2M_Reload>
	{
		protected override void Run(Session session, M2A_Reload message, Action<A2M_Reload> reply)
		{
			A2M_Reload response = new A2M_Reload();
			try
			{
				Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly()); //重新加载热更新dll
                reply(response);
			}
			catch (Exception e)
			{
				response.Error = ErrorCode.ERR_ReloadFail;
				StartConfig myStartConfig = StartConfigComponent.Instance.StartConfig;
				InnerConfig innerConfig = myStartConfig.GetComponent<InnerConfig>();
				response.Message = $"{innerConfig.IPEndPoint} reload fail, {e}";   //重载失败
                reply(response);
			}
		}
	}
}