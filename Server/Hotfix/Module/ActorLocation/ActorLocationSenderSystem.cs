using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorLocationSenderAwakeSystem : AwakeSystem<ActorLocationSender, long>
    {
        public override void Awake(ActorLocationSender self, long id)
        {
            self.LastSendTime = TimeHelper.Now();
	        self.Id = id;
            self.Tcs = null;
            self.FailTimes = 0;
            self.ActorId = 0;
        }
    }

    [ObjectSystem]
    public class ActorLocationSenderStartSystem : StartSystem<ActorLocationSender>
    {
        public override async void Start(ActorLocationSender self)
        {
            //在位置服务器查询 角色单元ID在那个服务器中 
            self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
            //获得服务器的地址
            self.Address = StartConfigComponent.Instance
                    .Get(IdGenerater.GetAppIdFromId(self.ActorId))
                    .GetComponent<InnerConfig>().IPEndPoint;

            self.UpdateAsync();
        }
    }
	
    [ObjectSystem]
    public class ActorLocationSenderDestroySystem : DestroySystem<ActorLocationSender>
    {
        public override void Destroy(ActorLocationSender self)
        {
	        self.RunError(ErrorCode.ERR_ActorRemove);
	        
            self.Id = 0;
            self.LastSendTime = 0;
            self.Address = null;
            self.ActorId = 0;
            self.FailTimes = 0;
            self.Tcs = null;
        }
    }
    
    public static class ActorLocationSenderHelper
    {
        /// <summary>
        /// 添加Actor消息到缓存中，等待发送
        /// </summary>
        /// <param name="self"></param>
        /// <param name="task"></param>
    	private static void Add(this ActorLocationSender self, ActorTask task)
		{
			if (self.IsDisposed)
			{
				throw new Exception("ActorLocationSender Disposed! dont hold ActorMessageSender");
			}

			self.WaitingTasks.Enqueue(task);
			// failtimes > 0表示正在重试，这时候不能加到正在发送队列
			if (self.FailTimes == 0) 
			{
				self.AllowGet(); //查看是否可以取堆栈里面缓存的消息
            }
		}

	    public static void RunError(this ActorLocationSender self, int errorCode)
	    {
		    while (self.WaitingTasks.Count > 0)
		    {
			    ActorTask actorTask = self.WaitingTasks.Dequeue();
			    actorTask.Tcs?.SetException(new RpcException(errorCode, ""));
		    }
		    self.WaitingTasks.Clear();
	    }

        /// <summary>
        /// /检测当前是否可以取堆栈里面缓存的消息
        /// </summary>
        /// <param name="self"></param>
        private static void AllowGet(this ActorLocationSender self)
		{
            ///没有缓存消息 跳出  表示不可以取棧里数据
            if (self.Tcs == null || self.WaitingTasks.Count <= 0)
			{
				return;
			}

			ActorTask task = self.WaitingTasks.Peek();   //取出第一个 但不移除

            var t = self.Tcs;
			self.Tcs = null;
			t.SetResult(task);  //设置了值 说明可以取了
        }

        /// <summary>
        /// 从队列里面取出一个消息进行处理
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        private static Task<ActorTask> GetAsync(this ActorLocationSender self)
		{
			if (self.WaitingTasks.Count > 0)
			{
				ActorTask task = self.WaitingTasks.Peek();
				return Task.FromResult(task);   //创建一个带返回值的、已完成的异步任务，其参数是取出的Task。
            }

			self.Tcs = new TaskCompletionSource<ActorTask>();  //如果棧里面有消息 这里是不会执行的，只有当站里面没有消息，程序就会在这里等待 知道加入了新的消息 继续
            return self.Tcs.Task;    //检测当前是否可以取堆栈里面缓存的消息
        }


        /// <summary>
        ///  Update   循环从堆栈总取出一个消息进行处理
        /// </summary>
        /// <param name="self"></param>
        public static async void UpdateAsync(this ActorLocationSender self)
		{
			try
			{
				long instanceId = self.InstanceId;
				while (true)
				{
					if (self.InstanceId != instanceId)
					{
						return;
					}
					ActorTask actorTask = await self.GetAsync();
					
					if (self.InstanceId != instanceId)
					{
						return;
					}
					if (actorTask.ActorRequest == null)
					{
						return;
					}

					await self.RunTask(actorTask);//处理
                }
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

        /// <summary>
        /// 处理发送消息
        /// </summary>
        /// <param name="self"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        private static async Task RunTask(this ActorLocationSender self, ActorTask task)
		{
            //获得和其他服务器通讯的会话
            ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.ActorId);
            //发送一个rpc消息
            IActorResponse response = await actorMessageSender.Call(task.ActorRequest);
			
			// 发送成功
			switch (response.Error)
			{
				case ErrorCode.ERR_NotFoundActor:
					// 如果没找到Actor,重试
					++self.FailTimes;

					// 失败MaxFailTimes次则清空actor发送队列，返回失败
					if (self.FailTimes > ActorLocationSender.MaxFailTimes)
					{
						// 失败直接删除actorproxy
						Log.Info($"actor send message fail, actorid: {self.Id}");
						self.RunError(response.Error);
						self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);//从字典中移除这个会话通道
                        return;
					}

					// 等待0.5s再发送
					await Game.Scene.GetComponent<TimerComponent>().WaitAsync(500);
					self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id); //重新获得map服务器地址
                    self.Address = StartConfigComponent.Instance
							.Get(IdGenerater.GetAppIdFromId(self.ActorId))
							.GetComponent<InnerConfig>().IPEndPoint; //得到IP
                    self.AllowGet();
					return;
				
				case ErrorCode.ERR_ActorNoMailBoxComponent:
					self.RunError(response.Error);
					self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);
					return;
				
				default:  //发送成功
                    self.LastSendTime = TimeHelper.Now(); //记录最后发送时间
                    self.FailTimes = 0;
					self.WaitingTasks.Dequeue();  //出栈

                    if (task.Tcs == null)
					{
						return;
					}
					
					IActorLocationResponse actorLocationResponse = response as IActorLocationResponse;
					if (actorLocationResponse == null)
					{
						task.Tcs.SetException(new Exception($"actor location respose is not IActorLocationResponse, but is: {response.GetType().Name}"));
					}
					task.Tcs.SetResult(actorLocationResponse);  //返回PRC消息回应
                    return;
			}
		}

        /// <summary>
        /// Actor发送消息 加入到棧中 等待发送  
        /// </summary>
        /// <param name="self"></param>
        /// <param name="message"></param>
        public static void Send(this ActorLocationSender self, IActorLocationMessage request)
	    {
		    if (request == null)
		    {
			    throw new Exception($"actor location send message is null");
		    }
		    ActorTask task = new ActorTask(request);
		    self.Add(task);
	    }

        /// <summary>
        /// Actor PRC消息发送
        /// </summary>
        /// <param name="self"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Task<IActorLocationResponse> Call(this ActorLocationSender self, IActorLocationRequest request)
		{
			if (request == null)
			{
				throw new Exception($"actor location call message is null");
			}
			TaskCompletionSource<IActorLocationResponse> tcs = new TaskCompletionSource<IActorLocationResponse>();
			ActorTask task = new ActorTask(request, tcs);
			self.Add(task);
			return task.Tcs.Task;
		}
    }
}