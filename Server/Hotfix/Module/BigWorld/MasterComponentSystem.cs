using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	public static class MasterComponentEx
	{
        /// <summary>
        /// 把IP添加到链表中
        /// </summary>
        /// <param name="self"></param>
        /// <param name="address"></param>
        public static void AddGhost(this MasterComponent self, IPEndPoint address)
		{
			self.ghostsAddress.Add(address);
		}
        /// <summary>
        /// 从链表中移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="address"></param>
        public static void RemoveGhost(this MasterComponent self, IPEndPoint address)
		{
			self.ghostsAddress.Remove(address);
		}
        /// <summary>
        /// 锁一个IP，如果当前有锁的地址 将其放入缓存中，待当前锁的IP释放后，在锁它
        /// </summary>
        /// <param name="self"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Task<bool> Lock(this MasterComponent self, IPEndPoint address)
		{
			if (self.lockedAddress == null) //如当前没有锁IP
            {
				self.lockedAddress = address;  //设置当前的锁IP地址
                return Task.FromResult(true);  //返回true
            }
            //下面就是说当前有锁IPlockedAddress！=空格
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			LockInfo lockInfo = new LockInfo(address, tcs);
			self.queue.Enqueue(lockInfo); //加入到堆栈 等到 堆栈返回tcs.Task的值
            return tcs.Task;
		}
        /// <summary>
        /// 解锁当前锁住的IP地址
        /// </summary>
        /// <param name="self"></param>
        /// <param name="address"></param>
        public static void Release(this MasterComponent self, IPEndPoint address)
		{
			if (!self.lockedAddress.Equals(address))  //看当前锁定的地址是否和解锁的地址匹配
            {
				Log.Error($"解锁地址与锁地址不匹配! {self.lockedAddress} {address}");
				return;
			}
			if (self.queue.Count == 0)  //如果没有待加锁的地址  退出
            { 
				self.lockedAddress = null;
				return;
			}
			LockInfo lockInfo = self.queue.Dequeue(); //从堆栈中取出待加锁的地址 加锁
            self.lockedAddress = lockInfo.Address;
			lockInfo.Tcs.SetResult(true);
		}
	}
}