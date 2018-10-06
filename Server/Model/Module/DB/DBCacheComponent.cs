using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class DbCacheComponentSystem : AwakeSystem<DBCacheComponent>
	{
		public override void Awake(DBCacheComponent self)
		{
			self.Awake();
		}
	}

	/// <summary>
	/// 用来缓存数据
	/// </summary>
	public class DBCacheComponent : Component
	{
		public Dictionary<string, Dictionary<long, ComponentWithId>> cache = new Dictionary<string, Dictionary<long, ComponentWithId>>();
        /// <summary>
        /// 任务数量 =32
        /// </summary>
        public const int taskCount = 32;
        /// <summary>
        /// 任务管理类链表
        /// </summary>
        public List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
            //实例化32个任务管理类
            for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = ComponentFactory.Create<DBTaskQueue>();
				this.tasks.Add(taskQueue);
			}
		}
        /// <summary>
        /// 添加组件到数据库中
        /// </summary>
        /// <param name="component"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public Task<bool> Add(ComponentWithId component, string collectionName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			if (string.IsNullOrEmpty(collectionName)) //传递来的collectionName是null
            { 
				collectionName = component.GetType().Name;//得到传递来的component的类名
            }
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, ComponentWithId, string, TaskCompletionSource<bool>>(component.Id, component, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}
        /// <summary>
        /// 批量注册组件到数据库中
        /// </summary>
        /// <param name="components"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public Task<bool> AddBatch(List<ComponentWithId> components, string collectionName)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<ComponentWithId>, string, TaskCompletionSource<bool>>(components, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}
        /// <summary>
        /// 添加组件到缓存中
        /// </summary>
        /// <param name="component"></param>
        /// <param name="collectionName"></param>
        public void AddToCache(ComponentWithId component, string collectionName = "")
		{
			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			Dictionary<long, ComponentWithId> collection;//  保存组件ID 和组件
            if (!this.cache.TryGetValue(collectionName, out collection))  //如果字典中没有这个类型
            {
				collection = new Dictionary<long, ComponentWithId>();
				this.cache.Add(collectionName, collection);  //组件类型 和 嵌套的字典（ 保存组件ID 和组件）
            }
			collection[component.Id] = component; //写入字典
        }
        /// <summary>
        /// 从缓存中获得组件
        /// </summary>
        /// <param name="collectionName">组件类型</param>
        /// <param name="id">组件ID</param>
        /// <returns></returns>
        public ComponentWithId GetFromCache(string collectionName, long id)
		{
			Dictionary<long, ComponentWithId> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return null;
			}
			ComponentWithId result;
			if (!d.TryGetValue(id, out result))
			{
				return null;
			}
			return result;
		}
        /// <summary>
        /// 从缓存中移除组件
        /// </summary>
        /// <param name="collectionName">组件类型</param>
        /// <param name="id">组件ID</param>
        public void RemoveFromCache(string collectionName, long id)
		{
			Dictionary<long, ComponentWithId> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return;
			}
			d.Remove(id);
		}
        /// <summary>
        /// 查询获取组件，会把任务添加到任务列表中，异步执行
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ComponentWithId> Get(string collectionName, long id)
		{
			ComponentWithId component = GetFromCache(collectionName, id); //如果缓存中有就从缓存中获取
            if (component != null)
			{
				return Task.FromResult(component);
			}

			TaskCompletionSource<ComponentWithId> tcs = new TaskCompletionSource<ComponentWithId>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, TaskCompletionSource<ComponentWithId>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}
        /// <summary>
        /// 查询获取批量组件，会把任务添加到任务列表中，异步执行
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="idList"></param>
        /// <returns></returns>
        public Task<List<ComponentWithId>> GetBatch(string collectionName, List<long> idList)
		{
			List <ComponentWithId> components = new List<ComponentWithId>();
			bool isAllInCache = true;
			foreach (long id in idList) //如果缓存中有就从缓存中获取
            {
				ComponentWithId component = this.GetFromCache(collectionName, id);
				if (component == null)
				{
					isAllInCache = false;  //只要有一项获取失败 就全部从数据库获取
                    break;
				}
				components.Add(component);
			}

			if (isAllInCache)
			{
				return Task.FromResult(components);
			}

			TaskCompletionSource<List<ComponentWithId>> tcs = new TaskCompletionSource<List<ComponentWithId>>();
			DBQueryBatchTask dbQueryBatchTask = ComponentFactory.Create<DBQueryBatchTask, List<long>, string, TaskCompletionSource<List<ComponentWithId>>>(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}
        /// <summary>
        /// 获得Json
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public Task<List<ComponentWithId>> GetJson(string collectionName, string json)
		{
			TaskCompletionSource<List<ComponentWithId>> tcs = new TaskCompletionSource<List<ComponentWithId>>();
			
			DBQueryJsonTask dbQueryJsonTask = ComponentFactory.Create<DBQueryJsonTask, string, string, TaskCompletionSource<List<ComponentWithId>>>(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}
	}
}
