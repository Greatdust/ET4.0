using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ETModel
{
	public enum LockStatus
	{
        /// <summary>
        /// 不是锁定状态
        /// </summary>
        LockedNot,
        /// <summary>
        /// 待加锁状态
        /// </summary>
        LockRequesting,
        /// <summary>
        /// 锁定状态
        /// </summary>
		Locked,
    }

	/// <summary>
	/// 分布式锁组件,Unit对象可能在不同进程上有镜像,访问该对象的时候需要对他加锁
	/// </summary>
	public class LockComponent: Component
	{
        /// <summary>
        /// 锁的状态
        /// </summary>
        public LockStatus status = LockStatus.LockedNot;
        /// <summary>
        /// IP地址
        /// </summary>
        public IPEndPoint address;
        /// <summary>
        /// 锁定次数
        /// </summary>
        public int lockCount;
        /// <summary>
        /// 带加锁完成回调字典
        /// </summary>
        public readonly Queue<TaskCompletionSource<bool>> queue = new Queue<TaskCompletionSource<bool>>();
	}
}