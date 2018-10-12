using System;
using System.Net;

namespace ETModel
{
    /// <summary>
    ///  网络通信协议 TCP ？UDP？WebSocket
    /// </summary>
    public enum NetworkProtocol
	{
		KCP,
		TCP,
		WebSocket,
	}
    /// <summary>
    /// 服务中心
    /// </summary>
    public abstract class AService: Component
	{
        /// <summary>
		/// 通道
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="id">Identifier.</param>
		public abstract AChannel GetChannel(long id);
        /// <summary>
        /// 接收通道 返回所有连接了服务器的客户端的Channel（这里是服务器时才有用）
        /// </summary>
        /// <returns>The channel.</returns>
        private Action<AChannel> acceptCallback;
        /// <summary>
        /// 接收通道
        /// </summary>
		public event Action<AChannel> AcceptCallback
		{
			add
			{
				this.acceptCallback += value;
			}
			remove
			{
				this.acceptCallback -= value;
			}
		}
        /// <summary>
        /// 调用接收通道 
        /// </summary>
        /// <param name="channel"></param>
        protected void OnAccept(AChannel channel)
		{
			this.acceptCallback.Invoke(channel);
		}
        /// <summary>
        /// 连接服务器(这个才是客户端才会用到的)
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="ipEndPoint">Ip end point.</param>
        public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);
        /// <summary>
        /// 连接服务器(这个才是客户端才会用到的)
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="ipEndPoint">Ip end point.</param>
        public abstract AChannel ConnectChannel(string address);
        /// <summary>
        /// 移除一个信道
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        public abstract void Remove(long channelId);

		public abstract void Update();
	}
}