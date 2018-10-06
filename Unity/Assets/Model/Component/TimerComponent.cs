using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// Timer.
    /// </summary>
    public struct Timer
	{
        /// <summary>
        /// 定时器ID
        /// </summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }
        /// <summary>
        /// 定时的时间
        /// </summary>
        /// <value>The time.</value>
        public long Time { get; set; }
        /// <summary>
        /// 一个有Bool返回值的异步方法
        /// </summary>
        public TaskCompletionSource<bool> tcs;
	}

	[ObjectSystem]
	public class TimerComponentUpdateSystem : UpdateSystem<TimerComponent>
	{
		public override void Update(TimerComponent self)
		{
			self.Update();
		}
	}

	public class TimerComponent : Component
	{
		private readonly Dictionary<long, Timer> timers = new Dictionary<long, Timer>();

        /// <summary>
        /// 保存定时器ID->定时器
        /// </summary>
        private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();
        /// <summary>
        /// key: 定时的时间, value: 当前定时时间的所有定时器的ID
        /// </summary>
        private readonly Queue<long> timeOutTime = new Queue<long>();
        /// <summary>
        /// 到时间的定时器的ID的到期时间
        /// </summary>
        private readonly Queue<long> timeOutTimerIds = new Queue<long>();

        /// <summary>
        ///  记录所有定时器中最小的定时时间（这样可以节省计算）不用每次都去MultiMap取第一个值
        /// </summary>
        private long minTime;

		public void Update()
		{
            //没有  退出
            if (this.timeId.Count == 0)
			{
				return;
			}
            //现在时间
            long timeNow = TimeHelper.Now();

            //说明还没有一个任何定时器到时间了（最小的都没到时间）
            if (timeNow < this.minTime)
			{
				return;
			}
            //遍历MultiMap字典
            foreach (KeyValuePair<long, List<long>> kv in this.timeId.GetDictionary())
			{
				long k = kv.Key;
				if (k > timeNow)  //最小的定时器的时间到了
                {
					minTime = k;   //重新赋值minTime
                    break;
				} 
				this.timeOutTime.Enqueue(k);   //把到期的定时器的时间记录到timeOutTime
            }

			while(this.timeOutTime.Count > 0)
			{
				long time = this.timeOutTime.Dequeue();
				foreach(long timerId in this.timeId[time])
				{
					this.timeOutTimerIds.Enqueue(timerId);	
				}
				this.timeId.Remove(time);
			}

            //查看这个到期时间有多少个定时器
            while (this.timeOutTimerIds.Count > 0)
			{
				long timerId = this.timeOutTimerIds.Dequeue();  //取得一个到期的定时器的ID
                Timer timer;
				if (!this.timers.TryGetValue(timerId, out timer)) //得到Timer
                {
					continue;
				}
				this.timers.Remove(timerId);  //到期了就移除这个定时器Timer
                timer.tcs.SetResult(true);    //告诉定时器 时间到了
            }
		}

        /// <summary>
        /// 移除一个定时器
        /// </summary>
        /// <param name="id">Identifier.</param>
        private void Remove(long id)
		{
			Timer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(id);
		}

        /// <summary>
        /// 设置一个定时器 （定时事件是直接给的时间，而不是多少秒后）
        /// </summary>
        /// <returns>The till async.</returns>
        /// <param name="tillTime">Till time.</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public Task WaitTillAsync(long tillTime, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
            //监视取消请求。
            cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;  //如果要返回tcs.Task 那么就需要tcs.SetResult(true) 设置了结果
        }

        /// <summary>
		/// 设置一个定时器 （定时事件是直接给的时间，而不是多少秒后）
		/// </summary>
		/// <returns>The till async.</returns>
		/// <param name="tillTime">Till time.</param>
		public Task WaitTillAsync(long tillTime)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = tillTime, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			return tcs.Task;
		}

        /// <summary>
        /// 设置一个定时器 （多少毫秒后）
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="time">Time.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task WaitAsync(long time, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.Now() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			cancellationToken.Register(() => { this.Remove(timer.Id); });
			return tcs.Task;
		}

        /// <summary>
        /// 设置一个定时器 （多少毫秒后）
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="time">Time.</param>
        public Task WaitAsync(long time)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Timer timer = new Timer { Id = IdGenerater.GenerateId(), Time = TimeHelper.Now() + time, tcs = tcs };
			this.timers[timer.Id] = timer;
			this.timeId.Add(timer.Time, timer.Id);
			if (timer.Time < this.minTime)
			{
				this.minTime = timer.Time;
			}
			return tcs.Task;
		}
	}
}