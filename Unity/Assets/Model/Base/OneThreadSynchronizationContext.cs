using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ETModel
{
    /// <summary>
    /// 把所有socket回调线程放在栈中 ，由主线程顺序执行 
    /// </summary>
    public class OneThreadSynchronizationContext : SynchronizationContext
	{
		public static OneThreadSynchronizationContext Instance { get; } = new OneThreadSynchronizationContext();

        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
        //ConcurrentQueue是高效无锁队列
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

		private Action a;

		public void Update()
		{
			while (true)
			{
				if (!this.queue.TryDequeue(out a))
				{
					return;
				}
				a();
			}
		}

		public override void Post(SendOrPostCallback callback, object state)
		{
			this.queue.Enqueue(() => { callback(state); });
		}
	}
}
