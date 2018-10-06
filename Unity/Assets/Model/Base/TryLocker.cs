using System;
using System.Threading;

namespace ETModel
{
    /// <summary>
    /// 尝试加锁
    /// </summary>
    public class TryLock : IDisposable
	{
		private object locked;

		public bool HasLock { get; private set; }

		public TryLock(object obj)
		{
			if (!Monitor.TryEnter(obj))  //加锁 没有成功就退出
            {
				return;
			}

			this.HasLock = true;    //加锁成功
            this.locked = obj;    //加锁对象
        }

		public void Dispose()
		{
			if (!this.HasLock)  //如果没有加锁 就退出
            {
				return;
			}

			Monitor.Exit(this.locked);   //有加锁 就强制退出锁状态
            this.locked = null;
			this.HasLock = false;
		}
	}
}
