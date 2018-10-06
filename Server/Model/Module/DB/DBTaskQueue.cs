using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class DbTaskQueueAwakeSystem : AwakeSystem<DBTaskQueue>
	{
		public override void Awake(DBTaskQueue self)
		{
			self.queue.Clear();
		}
	}


    [ObjectSystem]
	public class DbTaskQueueStartSystem : StartSystem<DBTaskQueue>
	{
        /// <summary>
        /// Start时会把缓存里的任务全部都执行完
        /// </summary>
        /// <param name="self"></param>
        public override async void Start(DBTaskQueue self)
		{
			long instanceId = self.InstanceId;
			
			while (true)
			{
				if (self.InstanceId != instanceId)
				{
					return;
				}

				DBTask task = await self.Get();

				try
				{
					await task.Run();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}

				task.Dispose();
			}
		}
	}
    /// <summary>
    /// 任务管理类
    /// </summary>
    public sealed class DBTaskQueue : Component
	{
        /// <summary>
        /// 缓存的任务
        /// </summary>
        public Queue<DBTask> queue = new Queue<DBTask>();

		public TaskCompletionSource<DBTask> tcs;

        /// <summary>
        /// 添加任务到缓存中
        /// </summary>
        /// <param name="task"></param>
        public void Add(DBTask task)
		{
			if (this.tcs != null)
			{
				var t = this.tcs;
				this.tcs = null;
				t.SetResult(task);
				return;
			}
			
			this.queue.Enqueue(task);
		}

		public Task<DBTask> Get()
		{
			if (this.queue.Count > 0)
			{
				DBTask task = this.queue.Dequeue();
				return Task.FromResult(task);
			}

			TaskCompletionSource<DBTask> t = new TaskCompletionSource<DBTask>();
			this.tcs = t;
			return t.Task; //任务执行完成后  this.tcs =null
        }
	}
}