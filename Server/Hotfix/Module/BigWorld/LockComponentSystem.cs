using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class LockComponentAwakeSystem : AwakeSystem<LockComponent, IPEndPoint>
	{
		public override void Awake(LockComponent self, IPEndPoint a)
		{
			self.Awake(a);
		}
	}

	/// <summary>
	/// 分布式锁组件,Unit对象可能在不同进程上有镜像,访问该对象的时候需要对他加锁
	/// </summary>
	public static class LockComponentEx
	{
		public static void Awake(this LockComponent self, IPEndPoint addr)	
		{
			self.address = addr;
		}

        /// <summary>
        /// 加锁  只有成功后，才会有 返回值  
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async Task Lock(this LockComponent self)
		{
			++self.lockCount; //加锁次数++

            if (self.status == LockStatus.Locked)  //如果已经锁定 跳出
            {
				return;
			}
			if (self.status == LockStatus.LockRequesting)  //如过当前是待加锁状态
            {
				await self.WaitLock();
				return;
			}
			
			self.status = LockStatus.LockRequesting;  //待加锁状态

            // 真身直接本地请求锁,镜像需要调用Rpc获取锁
            MasterComponent masterComponent = self.Entity.GetComponent<MasterComponent>();
			if (masterComponent != null)
			{
				await masterComponent.Lock(self.address);
			}
			else
			{
				self.RequestLock();
				await self.WaitLock();
			}
		}

        /// <summary>
        /// 等待加锁
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        private static Task<bool> WaitLock(this LockComponent self)
		{
			if (self.status == LockStatus.Locked)
			{
				return Task.FromResult(true);
			}

			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			self.queue.Enqueue(tcs);  //加入到待加锁的字典中 
            return tcs.Task;   //等待返回
        }

        /// <summary>
        /// 请求加锁
        /// </summary>
        /// <param name="self"></param>
        private static async void RequestLock(this LockComponent self)
		{
			try
            {
                ///这个需要加锁的角色的IP
                Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.address);
				string serverAddress = StartConfigComponent.Instance.StartConfig.ServerIP;
				G2G_LockRequest request = new G2G_LockRequest { Id = self.Entity.Id, Address = serverAddress };
				await session.Call(request);

				self.status = LockStatus.Locked;
                //所有的请求都确定  加锁成功
                foreach (TaskCompletionSource<bool> taskCompletionSource in self.queue)
				{
					taskCompletionSource.SetResult(true);
				}
				self.queue.Clear();
			}
			catch (Exception e)
			{
				Log.Error($"获取锁失败: {self.address} {self.Entity.Id} {e}");
			}
		}
        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async Task Release(this LockComponent self)
		{
			--self.lockCount;
			if (self.lockCount != 0)
			{
				return;
			}

			self.status = LockStatus.LockedNot; //设置为不加锁状态
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.address);  //获取他的地址
            G2G_LockReleaseRequest request = new G2G_LockReleaseRequest();
			await session.Call(request);  //发送解锁消息
        }
	}
}