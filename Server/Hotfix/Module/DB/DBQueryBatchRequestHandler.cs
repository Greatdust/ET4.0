using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 查询获取批量组件处理
    /// </summary>
    [MessageHandler(AppType.DB)]
	public class DBQueryBatchRequestHandler : AMRpcHandler<DBQueryBatchRequest, DBQueryBatchResponse>
	{
		protected override async void Run(Session session, DBQueryBatchRequest message, Action<DBQueryBatchResponse> reply)
		{
			DBQueryBatchResponse response = new DBQueryBatchResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				List<ComponentWithId> components = await dbCacheComponent.GetBatch(message.CollectionName, message.IdList);

				response.Components = components;

				if (message.NeedCache) //从数据库得到组件后  判断是否需要加载到缓存中
                {
					foreach (ComponentWithId component in components)
					{
						dbCacheComponent.AddToCache(component, message.CollectionName);
					}
				}

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}