using ETModel;
using System.Collections.Generic;
namespace ETModel
{
    /// <summary>
    /// 传送unit请求 MAP->MAP服务器
    /// </summary>
    [Message(InnerOpcode.M2M_TrasferUnitRequest)]
	public partial class M2M_TrasferUnitRequest: IRequest
	{
		public int RpcId { get; set; }

		public Unit Unit { get; set; }

	}
    /// <summary>
    /// 传送unit回应 MAP->MAP服务器
    /// </summary>
    [Message(InnerOpcode.M2M_TrasferUnitResponse)]
	public partial class M2M_TrasferUnitResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long InstanceId { get; set; }

	}
    /// <summary>
    /// Manager->all 热更新
    /// </summary>
    [Message(InnerOpcode.M2A_Reload)]
	public partial class M2A_Reload: IRequest
	{
		public int RpcId { get; set; }

	}
    /// <summary>
    /// all->Manager  热更新
    /// </summary>
    [Message(InnerOpcode.A2M_Reload)]
	public partial class A2M_Reload: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}
    /// <summary>
    /// 加锁请求
    /// </summary>
    [Message(InnerOpcode.G2G_LockRequest)]
	public partial class G2G_LockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string Address { get; set; }

	}
    /// <summary>
    /// 加锁的回复
    /// </summary>
    [Message(InnerOpcode.G2G_LockResponse)]
	public partial class G2G_LockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}
    /// <summary>
    /// 解锁请求
    /// </summary>
    [Message(InnerOpcode.G2G_LockReleaseRequest)]
	public partial class G2G_LockReleaseRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string Address { get; set; }

	}
    /// <summary>
    /// 解锁回复
    /// </summary>
    [Message(InnerOpcode.G2G_LockReleaseResponse)]
	public partial class G2G_LockReleaseResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}
    /// <summary>
    /// 存储消息请求
    /// </summary>
    [Message(InnerOpcode.DBSaveRequest)]
	public partial class DBSaveRequest: IRequest
	{
		public int RpcId { get; set; }

		public bool NeedCache { get; set; }

		public string CollectionName { get; set; }

		public ComponentWithId Component { get; set; }

	}
    /// <summary>
    /// 批量保存组件回复
    /// </summary>
    [Message(InnerOpcode.DBSaveBatchResponse)]
	public partial class DBSaveBatchResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}
    /// <summary>
    /// 批量保存组件请求
    /// </summary>
    [Message(InnerOpcode.DBSaveBatchRequest)]
	public partial class DBSaveBatchRequest: IRequest
	{
		public int RpcId { get; set; }

		public bool NeedCache { get; set; }

		public string CollectionName { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}
    /// <summary>
    /// 存储消息回复
    /// </summary>
    [Message(InnerOpcode.DBSaveResponse)]
	public partial class DBSaveResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

    /// <summary>
    /// 组件查询获取请求
    /// </summary>
    [Message(InnerOpcode.DBQueryRequest)]
	public partial class DBQueryRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id { get; set; }

		public string CollectionName { get; set; }

		public bool NeedCache { get; set; }

    }
    /// <summary>
    /// 组件查询获取回复
    /// </summary>
	[Message(InnerOpcode.DBQueryResponse)]
	public partial class DBQueryResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public ComponentWithId Component { get; set; }

	}

    /// <summary>
    /// 组件批量查询获取请求
    /// </summary>
    [Message(InnerOpcode.DBQueryBatchRequest)]
	public partial class DBQueryBatchRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public List<long> IdList = new List<long>();

		public bool NeedCache { get; set; }

	}
    /// <summary>
    /// 组件批量查询获取回应
    /// </summary>
    [Message(InnerOpcode.DBQueryBatchResponse)]
	public partial class DBQueryBatchResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}
    /// <summary>
    /// 通过Json请求批量得到组件数据请求
    /// </summary>
    [Message(InnerOpcode.DBQueryJsonRequest)]
	public partial class DBQueryJsonRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName { get; set; }

		public string Json { get; set; }

	}
    /// <summary>
    /// 回应得到组件数据链表
    /// </summary>
    [Message(InnerOpcode.DBQueryJsonResponse)]
	public partial class DBQueryJsonResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}
    /// <summary>
    ///  注册位置到位置服务器请求
    /// </summary>
    [Message(InnerOpcode.ObjectAddRequest)]
	public partial class ObjectAddRequest: IRequest
	{
		public int RpcId { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Key { get; set; }
        /// <summary>
        /// Map服务器ID
        /// </summary>
        public long InstanceId { get; set; }

	}
    /// <summary>
    /// 注册位置到位置服务器应答
    /// </summary>
    [Message(InnerOpcode.ObjectAddResponse)]
	public partial class ObjectAddResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

    /// <summary>
    /// 移除一个角色id在位置服务器注册的信息请求
    /// </summary>
    [Message(InnerOpcode.ObjectRemoveRequest)]
	public partial class ObjectRemoveRequest: IRequest
	{
		public int RpcId { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Key { get; set; }

	}
    /// <summary>
    /// 移除一个角色id在位置服务器注册的信息回复
    /// </summary>
    [Message(InnerOpcode.ObjectRemoveResponse)]
	public partial class ObjectRemoveResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}
    /// <summary>
    /// 请求在位置服务器的地址进行加锁
    /// </summary>
    [Message(InnerOpcode.ObjectLockRequest)]
	public partial class ObjectLockRequest: IRequest
	{
		public int RpcId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
		public long Key { get; set; }
        /// <summary>
        /// MAP服务器ID
        /// </summary>
        public long InstanceId { get; set; }
        /// <summary>
        /// 加锁时间
        /// </summary>
        public int Time { get; set; }

	}
    /// <summary>
    /// 回应在位置服务器地址加锁处理结果
    /// </summary>
    [Message(InnerOpcode.ObjectLockResponse)]
	public partial class ObjectLockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

    /// <summary>
    /// 请求在位置服务器地址解锁
    /// </summary>
    [Message(InnerOpcode.ObjectUnLockRequest)]
	public partial class ObjectUnLockRequest: IRequest
	{
		public int RpcId { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public long Key { get; set; }
        /// <summary>
        /// 旧的MAP服务器ID
        /// </summary>
        public long OldInstanceId { get; set; }
        /// <summary>
        /// 新的map服务器ID
        /// </summary>
        public long InstanceId { get; set; }

	}
    /// <summary>
    /// 回应在位置服务器地址解锁处理结果
    /// </summary>
    [Message(InnerOpcode.ObjectUnLockResponse)]
	public partial class ObjectUnLockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}
    /// <summary>
    /// 请求得到ID的mpa服务器地址
    /// </summary>
    [Message(InnerOpcode.ObjectGetRequest)]
	public partial class ObjectGetRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key { get; set; }

	}
    /// <summary>
    /// 回复得到ID的mpa服务器地址
    /// </summary>
    [Message(InnerOpcode.ObjectGetResponse)]
	public partial class ObjectGetResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long InstanceId { get; set; }

	}

    /// <summary>
    /// 获得登录KEY请求   Realm->Gate
    /// </summary>
    [Message(InnerOpcode.R2G_GetLoginKey)]
	public partial class R2G_GetLoginKey: IRequest
	{
		public int RpcId { get; set; }

		public string Account { get; set; }

	}
    /// <summary>
    ///  获得登录KEY回应 Gate->Realm
    /// </summary>
    [Message(InnerOpcode.G2R_GetLoginKey)]
	public partial class G2R_GetLoginKey: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long Key { get; set; }

	}
    /// <summary>
    /// 创建角色单元请求   Gate->map
    /// </summary>
    [Message(InnerOpcode.G2M_CreateUnit)]
	public partial class G2M_CreateUnit: IRequest
	{
		public int RpcId { get; set; }

		public long PlayerId { get; set; }

		public long GateSessionId { get; set; }

	}
    /// <summary>
    ///  创建角色单元回应 map->Gate
    /// </summary>
    [Message(InnerOpcode.M2G_CreateUnit)]
	public partial class M2G_CreateUnit: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long UnitId { get; set; }

		public int Count { get; set; }

	}
    /// <summary>
    /// 会话断开消息   Gate->map
    /// </summary>
    [Message(InnerOpcode.G2M_SessionDisconnect)]
	public partial class G2M_SessionDisconnect: IActorLocationMessage
	{
		public int RpcId { get; set; }

		public long ActorId { get; set; }

	}

}
