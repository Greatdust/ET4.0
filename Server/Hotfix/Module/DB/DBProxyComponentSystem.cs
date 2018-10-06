using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace ETHotfix
{
	[ObjectSystem]
	public class DbProxyComponentSystem : AwakeSystem<DBProxyComponent>
	{
		public override void Awake(DBProxyComponent self)
		{
			self.Awake();
		}
	}
	
	/// <summary>
	/// 用来与数据库操作代理
	/// </summary>
	public static class DBProxyComponentEx
	{
        /// <summary>
        /// 初始化时得到数据库地址
        /// </summary>
        /// <param name="self"></param>
        public static void Awake(this DBProxyComponent self)
		{
			StartConfig dbStartConfig = StartConfigComponent.Instance.DBConfig;
			self.dbAddress = dbStartConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

        /// <summary>
        /// 发送消息将组件存到数据库
        /// </summary>
        /// <param name="self"></param>
        /// <param name="component">需保持的组件</param>
        /// <param name="needCache">是否缓存</param>
        /// <returns></returns>
        public static async Task Save(this DBProxyComponent self, ComponentWithId component, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component, NeedCache = needCache});
		}

        /// <summary>
        /// 发送消息将组件批量存到数据库
        /// </summary>
        /// <param name="self"></param>
        /// <param name="components">需保持的组件链表</param>
        /// <param name="needCache">是否缓存</param>
        /// <returns></returns>
        public static async Task SaveBatch(this DBProxyComponent self, List<ComponentWithId> components, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveBatchRequest { Components = components, NeedCache = needCache});
		}

		public static async Task Save(this DBProxyComponent self, ComponentWithId component, bool needCache, CancellationToken cancellationToken)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component, NeedCache = needCache}, cancellationToken);
		}

        /// <summary>
        /// 保存到LOG  不需要缓存
        /// </summary>
        /// <param name="self"></param>
        /// <param name="component"></param>
        public static async void SaveLog(this DBProxyComponent self, ComponentWithId component)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component,  NeedCache = false, CollectionName = "Log" });
		}

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <param name="needCache"></param>
        /// <returns></returns>
		public static async Task<T> Query<T>(this DBProxyComponent self, long id, bool needCache = true) where T: ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryResponse dbQueryResponse = (DBQueryResponse)await session.Call(new DBQueryRequest { CollectionName = typeof(T).Name, Id = id, NeedCache = needCache });
			return (T)dbQueryResponse.Component;
		}
		
		/// <summary>
		/// 根据查询表达式查询
		/// </summary>
		/// <param name="self"></param>
		/// <param name="exp"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static async Task<List<ComponentWithId>> Query<T>(this DBProxyComponent self, Expression<Func<T ,bool>> exp) where T: ComponentWithId
		{
			ExpressionFilterDefinition<T> filter = new ExpressionFilterDefinition<T>(exp);
			IBsonSerializerRegistry serializerRegistry = BsonSerializer.SerializerRegistry;
			IBsonSerializer<T> documentSerializer = serializerRegistry.GetSerializer<T>();
			string json = filter.Render(documentSerializer, serializerRegistry).ToJson();
			return await self.Query<T>(json);
		}
        /// <summary>
        /// 批量获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="ids"></param>
        /// <param name="needCache"></param>
        /// <returns></returns>
        public static async Task<List<ComponentWithId>> Query<T>(this DBProxyComponent self, List<long> ids, bool needCache = true) where T : ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryBatchResponse dbQueryBatchResponse = (DBQueryBatchResponse)await session.Call(new DBQueryBatchRequest { CollectionName = typeof(T).Name, IdList = ids, NeedCache = needCache});
			return dbQueryBatchResponse.Components;
		}

		/// <summary>
		/// 根据json查询条件查询
		/// </summary>
		/// <param name="self"></param>
		/// <param name="json"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static async Task<List<ComponentWithId>> Query<T>(this DBProxyComponent self, string json) where T : ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryJsonResponse dbQueryJsonResponse = (DBQueryJsonResponse)await session.Call(new DBQueryJsonRequest { CollectionName = typeof(T).Name, Json = json });
			return dbQueryJsonResponse.Components;
		}
	}
}