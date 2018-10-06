using System;
using System.IO;
using System.Net;

namespace ETModel
{
    /// <summary>
    /// 信道类型；通道类型
    /// </summary>
    public enum ChannelType
	{
        /// <summary>
		/// 连接
		/// </summary>
		Connect,
        /// <summary>
        /// 接收
        /// </summary>
        Accept,
	}

    /// <summary>
    /// 信道
    /// </summary>
    public abstract class AChannel: ComponentWithId
	{
        /// <summary>
        ///获取信道的类型。
        /// </summary>
        /// <value>The type of the channel.</value>
        public ChannelType ChannelType { get; }
        /// <summary>
        /// 它所属的服务中心
        /// </summary>
        protected AService service;
  
        public abstract MemoryStream Stream { get; }
		
		public int Error { get; set; }
        /// <summary>
        /// 远程地址
        /// </summary>
        /// <value>The remote address.</value>
        public IPEndPoint RemoteAddress { get; protected set; }
        /// <summary>
        /// 错误回调委托定义
        /// </summary>
        private Action<AChannel, int> errorCallback;

        /// <summary>
        /// 发生错误回调时委托
        /// </summary>
        public event Action<AChannel, int> ErrorCallback
		{
			add
			{
				this.errorCallback += value;
			}
			remove
			{
				this.errorCallback -= value;
			}
		}
        /// <summary>
        /// 接收消息
        /// </summary>
        private Action<MemoryStream> readCallback;

		public event Action<MemoryStream> ReadCallback
		{
			add
			{
				this.readCallback += value;
			}
			remove
			{
				this.readCallback -= value;
			}
		}
		
		protected void OnRead(MemoryStream memoryStream)
		{
			this.readCallback.Invoke(memoryStream);
		}


        /// <summary>
        ///引发错误事件。
        /// </summary>
        /// <param name="error">Error.</param>
        protected void OnError(int e)
		{
			this.Error = e;
			this.errorCallback?.Invoke(this, e);
		}

		protected AChannel(AService service, ChannelType channelType)
		{
			this.Id = IdGenerater.GenerateId();
			this.ChannelType = channelType;
			this.service = service;
		}

        /// <summary>
        /// 发送消息
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// 发送消息
        /// </summary>
        public abstract void Send(MemoryStream stream);
        /// <summary>
        /// 处理 卸载
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="ETModel.AChannel"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="ETModel.AChannel"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="ETModel.AChannel"/> so the garbage
        /// collector can reclaim the memory that the <see cref="ETModel.AChannel"/> was occupying.</remarks>
        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.service.Remove(this.Id);
		}
	}
}