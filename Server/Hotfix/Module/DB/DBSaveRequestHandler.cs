using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 存储组件处理
    /// </summary>
    [MessageHandler(AppType.DB)]
	public class DBSaveRequestHandler : AMRpcHandler<DBSaveRequest, DBSaveResponse>
	{
		protected override async void Run(Session session, DBSaveRequest message, Action<DBSaveResponse> reply)
		{
			DBSaveResponse response = new DBSaveResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				if (string.IsNullOrEmpty(message.CollectionName))  //如果传递来类型的空的
                {
					message.CollectionName = message.Component.GetType().Name;  //直接取组件的类型名
                }
                //如果需要加入到缓存中
                if (message.NeedCache)
				{
					dbCacheComponent.AddToCache(message.Component, message.CollectionName);
				}
				await dbCacheComponent.Add(message.Component, message.CollectionName); // 添加到数据库
                reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}