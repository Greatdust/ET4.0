using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 从位置服务器获得角色ID所在的map服务器地址
    /// </summary>
    [MessageHandler(AppType.Location)]
	public class ObjectGetRequestHandler : AMRpcHandler<ObjectGetRequest, ObjectGetResponse>
	{
		protected override async void Run(Session session, ObjectGetRequest message, Action<ObjectGetResponse> reply)
		{
			ObjectGetResponse response = new ObjectGetResponse();
			try
            {
                //得到这个ID所在的服务器ID
                long instanceId = await Game.Scene.GetComponent<LocationComponent>().GetAsync(message.Key);
				if (instanceId == 0)
				{
					response.Error = ErrorCode.ERR_ActorLocationNotFound;
				}
				response.InstanceId = instanceId;  //服务器ID
                reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}