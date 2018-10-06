using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	public abstract class LocationTask: Component
	{
		public abstract void Run();
	}

    /// <summary>
    /// 在创建LocationQueryTask时  框架会调用这个类 用来初始化LocationQueryTask
    /// </summary>
    [ObjectSystem]
	public class LocationQueryTaskAwakeSystem : AwakeSystem<LocationQueryTask, long>
	{
		public override void Awake(LocationQueryTask self, long key)
		{
			self.Key = key;
			self.Tcs = new TaskCompletionSource<long>();
		}
	}

	public sealed class LocationQueryTask : LocationTask
	{
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Key;

		public TaskCompletionSource<long> Tcs;

		public Task<long> Task
		{
			get
			{
				return this.Tcs.Task;
			}
		}
        /// <summary>
        /// 解锁后 被搁置的位置查询请求都需要回应 这个时候 locationComponent 里面字典的内容都已经更新 所有可以直接使用
        /// </summary>
        public override void Run()
		{
			try
			{
				LocationComponent locationComponent = this.GetParent<LocationComponent>();
				long location = locationComponent.Get(this.Key);
				this.Tcs.SetResult(location);  //从字典中获得此KEY的的值 设置Tcs
            }
			catch (Exception e)
			{
				this.Tcs.SetException(e);
			}
		}
	}
    /// <summary>
    /// 位置组件
    /// </summary>
    public class LocationComponent : Component
	{
        /// <summary>
        /// 位置字典 保存了 角色ID 和 服务器ID  
        /// </summary>
        private readonly Dictionary<long, long> locations = new Dictionary<long, long>();
        /// <summary>
        /// 锁字典
        /// </summary>
        private readonly Dictionary<long, long> lockDict = new Dictionary<long, long>();
        /// <summary>
        /// Task堆栈字典 用来保存当位置正在转换时 发出的位置请求
        /// </summary>
		private readonly Dictionary<long, Queue<LocationTask>> taskQueues = new Dictionary<long, Queue<LocationTask>>();
        /// <summary>
        /// 添加到位置字典中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="instanceId"></param>
        public void Add(long key, long instanceId)
		{
			this.locations[key] = instanceId;

			Log.Info($"location add key: {key} instanceId: {instanceId}");

			// 更新db
			//await Game.Scene.GetComponent<DBProxyComponent>().Save(new Location(key, address));
		}
        /// <summary>
        /// 移除字典中key项的地址
        /// </summary>
        /// <param name="key"></param>
        public void Remove(long key)
		{
			Log.Info($"location remove key: {key}");
			this.locations.Remove(key);
		}
        /// <summary>
        /// 获得地址
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long Get(long key)
		{
			this.locations.TryGetValue(key, out long instanceId);
			return instanceId;
		}
        /// <summary>
        /// 锁住一个实体的地址  有可能这个实体的地址正在转换 所以在转换的时候 这个地址是不可以用的
        /// </summary>
        /// <param name="key"></param>
        /// <param name="instanceId"></param>
        /// <param name="time">超出这个时间  就自动解锁</param>
        public async void Lock(long key, long instanceId, int time = 0)
		{
			if (this.lockDict.ContainsKey(key)) //已经锁住了 出错
            {
				Log.Error($"不可能同时存在两次lock, key: {key} InstanceId: {instanceId}");
				return;
			}

			Log.Info($"location lock key: {key} InstanceId: {instanceId}");

			if (!this.locations.TryGetValue(key, out long saveInstanceId))
			{
				Log.Error($"actor没有注册, key: {key} InstanceId: {instanceId}");
				return;
			}

			if (saveInstanceId != instanceId)  //传递来的instanceId是否和locations字典中此Key的值是否相同
            {
				Log.Error($"actor注册的instanceId与lock的不一致, key: {key} InstanceId: {instanceId} saveInstanceId: {saveInstanceId}");
				return;
			}

			this.lockDict.Add(key, instanceId);  //加锁  

            // 超时则解锁
            if (time > 0)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(time);  //异步延时

                if (!this.lockDict.ContainsKey(key))  //如果此时已经解锁了就跳出
                {
					return;
				}
				Log.Info($"location timeout unlock key: {key} time: {time}");
				this.UnLock(key); //从字典中移除
            }
		}
        /// <summary>
        /// 对这个实体的地址进行解锁，更新新的地址
        /// </summary>
        /// <param name="key">角色id</param>
        /// <param name="oldInstanceId">旧的MAP服务器ID</param>
        /// <param name="instanceId">新的MAP服务器ID</param>
        public void UnLockAndUpdate(long key, long oldInstanceId, long instanceId)
		{
			this.lockDict.TryGetValue(key, out long lockInstanceId); //取出字典中旧的map服务器的id
            if (lockInstanceId != oldInstanceId)  //进行效验
            {
				Log.Error($"unlock appid is different {lockInstanceId} {oldInstanceId}" );
			}
			Log.Info($"location unlock key: {key} oldInstanceId: {oldInstanceId} new: {instanceId}");
			this.locations[key] = instanceId; //更新位置字典中的值
            this.UnLock(key);                       //移除lockDict字典中此项
        }
        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="key"></param>
        private void UnLock(long key)
		{
			this.lockDict.Remove(key);
            //加锁后所有获取地址的请求全部都搁置，等待玩家转换完成后再处理。
            //这样，获取的地址才是正确的。而搁置的请求，则会以LocationTask储存在taskQueues字典里

            //遍历Task堆栈字典字典taskQueues中  回复之前的位置请求
            if (!this.taskQueues.TryGetValue(key, out Queue<LocationTask> tasks))
			{
				return;
			}

			while (true)
			{
				if (tasks.Count <= 0) //执行完就跳出
                {
					this.taskQueues.Remove(key);
					return;
				}
				if (this.lockDict.ContainsKey(key))
				{
					return;
				}

				LocationTask task = tasks.Dequeue();
				try
				{
					task.Run();  //将此key下堆栈中的Task任务全部执行
                }
				catch (Exception e)
				{
					Log.Error(e);
				}
				task.Dispose();  //释放
            }
		}
        /// <summary>
        /// 异步请求位置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<long> GetAsync(long key)
		{
			if (!this.lockDict.ContainsKey(key))//如果这个位置没有在锁状态
            {
				this.locations.TryGetValue(key, out long instanceId);   //获取字典此key的值
                Log.Info($"location get key: {key} {instanceId}");
				return Task.FromResult(instanceId);  //创建一个带返回值的、已完成的Task。
            }

			LocationQueryTask task = ComponentFactory.CreateWithParent<LocationQueryTask, long>(this, key);
			this.AddTask(key, task);  //添加到字典中缓存 待这个地址解锁后 会自动返回其新地址
            return task.Task; //等待位置转换完成 返回新的位置
        }
        /// <summary>
        /// 添加到taskQueues字典中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="task"></param>
        public void AddTask(long key, LocationTask task)
		{
            //可能会有多个请求得到一个角色的服务器ID
            if (!this.taskQueues.TryGetValue(key, out Queue<LocationTask> tasks))
			{
				tasks = new Queue<LocationTask>();
				this.taskQueues[key] = tasks;
			}
			tasks.Enqueue(task); //入栈
        }

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
			
			this.locations.Clear();
			this.lockDict.Clear();
			this.taskQueues.Clear();
		}
	}
}