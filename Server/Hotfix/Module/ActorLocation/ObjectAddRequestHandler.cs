using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 注册位置到位置服务器处理
    /// </summary>
    [MessageHandler(AppType.Location)]
	public class ObjectAddRequestHandler : AMRpcHandler<ObjectAddRequest, ObjectAddResponse>
	{
		protected override void Run(Session session, ObjectAddRequest message, Action<ObjectAddResponse> reply)
		{
			ObjectAddResponse response = new ObjectAddResponse();
			try
            { 
                //把角色ID和map服务器ID注册到位置组件中
                Game.Scene.GetComponent<LocationComponent>().Add(message.Key, message.InstanceId);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply); //如果出错  回复错误
            }
		}
	}
}