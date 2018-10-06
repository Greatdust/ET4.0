using System.Net;

namespace ETModel
{
    /// <summary>
    /// 这个 Actor消息的map服务器的会话管理器 可以理解为 一个geta服务器和一个map服务器的会话通道（保存了map服务器地址和所有对此map服务器的 Actor消息缓存）
    /// 知道对方的instanceId，使用这个类发actor消息
    /// </summary>
    public struct ActorMessageSender
	{
		// actor的地址
		public IPEndPoint Address { get; }

		public long ActorId { get; }

		public ActorMessageSender(long actorId, IPEndPoint address)
		{
			this.ActorId = actorId;
			this.Address = address;
		}
	}
}