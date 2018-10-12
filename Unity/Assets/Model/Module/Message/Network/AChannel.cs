using System;
using System.IO;
using System.Net;

namespace ETModel
{
    /// <summary>
    /// 信道类型；通道类型 关键就两个方法  ReadCallback 接收消息 void Send(MemoryStream stream);发送消息
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
        /// 接收消息委托  解析到的消息 会从这里发出去
        /// </summary>
        private Action<MemoryStream> readCallback;
        /// <summary>
        /// 接收消息委托  供外部调用
        /// </summary>
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
        /// <summary>
        /// 接收到的消息会调用委托把消息抛出去
        /// </summary>
        /// <param name="memoryStream"></param>
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

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="service"></param>
        /// <param name="channelType"></param>
		protected AChannel(AService service, ChannelType channelType)
		{
			this.Id = IdGenerater.GenerateId();
			this.ChannelType = channelType;
			this.service = service;
		}

 
        public abstract void Start();

        /// <summary>
        /// 发送消息  供外部调用
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