using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class LocationProxyComponentSystem : AwakeSystem<LocationProxyComponent>
	{
		public override void Awake(LocationProxyComponent self)
		{
			self.Awake();
		}
	}
    /// <summary>
    /// LocationProxyComponent扩展方法
    /// </summary>
    public static class LocationProxyComponentEx
	{
        /// <summary>
        /// 在初始化LocationProxyComponent组件时  会把位置服务器的IP地址读取出来 保存在LocationAddress中
        /// </summary>
        /// <param name="self"></param>
        public static void Awake(this LocationProxyComponent self)
        {
            //获得启动配置
            StartConfigComponent startConfigComponent = StartConfigComponent.Instance;
            //拿到里面的位置服务器地址
            StartConfig startConfig = startConfigComponent.LocationConfig;
            //保存位置服务器的IP端口
            self.LocationAddress = startConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

        /// <summary>
        /// 把位置注册到位置服务器中
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key">角色ID</param>
        /// <param name="instanceId">map服务器ID</param>
        /// <returns></returns>
        public static async Task Add(this LocationProxyComponent self, long key, long instanceId)
        { 
            //获取一个Session用来和位置服务器会话
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectAddRequest() { Key = key, InstanceId = instanceId });
		}


        /// <summary>
        /// 对一个实体的map位置在位置服务器中进行加锁 ，在加锁期间 ，申请位置的请求是不会立即返回 ，会先缓存，待解锁（时间到）后会应答缓存的位置请求
        /// </summary>
        /// <param name="self">类的扩展方法写法（C#语法）</param>
        /// <param name="key">角色ID</param>
        /// <param name="instanceId">map服务器ID</param>
        /// <param name="time">加锁时间</param>
        /// <returns></returns>
        public static async Task Lock(this LocationProxyComponent self, long key, long instanceId, int time = 1000)
		{
            //获取一个Session用来和位置服务器会话
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
		}

        /// <summary>
        /// 在位置服务器中进行解锁
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key">角色ID</param>
        /// <param name="oldInstanceId">旧的map服务器ID</param>
        /// <param name="instanceId">新的map服务器ID</param>
        /// <returns></returns>
        public static async Task UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId});
		}

        /// <summary>
        /// 移除一个角色id在位置服务器注册的信息
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"> 角色ID</param>
        /// <returns></returns>
        public static async Task Remove(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectRemoveRequest() { Key = key });
		}

        /// <summary>
        /// 传入角色单元的ID  得到服务器的ID
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<long> Get(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			ObjectGetResponse response = (ObjectGetResponse)await session.Call(new ObjectGetRequest() { Key = key });
			return response.InstanceId;
		}
	}
}