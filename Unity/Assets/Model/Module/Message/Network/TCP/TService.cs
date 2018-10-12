using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.IO;

namespace ETModel
{
    /// <summary>
    ///  关键的方法有 TService()初始化时绑定了自己的端口 等待远程端的连接，如果是客户端，其实是没有
    ///  远程端连接来的 只是为了共用，客户端主要用到ConnectChannel（）方法，用来创建一个 TChannel（Socket）和服务器连接
    /// </summary>
	public sealed class TService : AService
	{
        /// <summary>
        /// 保存了所有的远程连接  （如果是客户端 那么只会有一个远程连接 即服务器）
        /// </summary>
        private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();
        /// <summary>
        /// 用来接收远程连接的Socket
        /// </summary>
        private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        /// <summary>
        /// 本地Socket
        /// </summary>
        private Socket acceptor;
		/// <summary>
        /// 流管理器 从里面获取流   节省GC
        /// </summary>
		public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
		/// <summary>
        /// 可以开始发送消息的信道的ID
        /// </summary>
		public HashSet<long> needStartSendChannel = new HashSet<long>();


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ipEndPoint">本地IP</param>
        /// <param name="acceptCallback">用于处理接收到消息数据的方法</param>
        public TService(IPEndPoint ipEndPoint, Action<AChannel> acceptCallback)
		{
			this.InstanceId = IdGenerater.GenerateId();
			this.AcceptCallback += acceptCallback; //处理接收到消息的函数
			
			this.acceptor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 端口复用。ReuseAddress选项设置为True将允许将套接字绑定到已在使用中的地址。 

            /*端口复用真正的用处主要在于服务器编程：当服务器需要重启时，经常会碰到端口尚未完全关闭的情况，这时如果不设置端口复用，则无法完成绑定，因为端口还处于被别的套接口绑定的状态之中。
             * socket1.Bind(localEP);   socket2.Bind(localEP);
             * 这样Socket1和Socket2便绑定在同一个端口上了。
             * */
            this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.innArgs.Completed += this.OnComplete;
			
			this.acceptor.Bind(ipEndPoint);//绑定本地IP
            this.acceptor.Listen(1000);  //监听远程端数量

            this.AcceptAsync(); //开始监听
        }

		public TService()
		{
			this.InstanceId = IdGenerater.GenerateId();
		}

        /// <summary>
        /// 回收
        /// </summary>
        public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (long id in this.idChannels.Keys.ToArray()) //回收所有远程端Socket
            {
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}
			this.acceptor?.Close();  //关闭本地服务端
            this.acceptor = null;
			this.innArgs.Dispose();
		}


        /// <summary>
        /// 监听远程端的连接
        /// </summary>
        public void AcceptAsync()
		{
			this.innArgs.AcceptSocket = null;
			if (this.acceptor.AcceptAsync(this.innArgs))   //连接远程服务器得到远程端的Socket
            {
				return;
			}
			OnAcceptComplete(this.innArgs);  //连接服务器成功回调
        }
        /// <summary>
        /// SocketAsyncEventArgs 里面的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComplete(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    OneThreadSynchronizationContext.Instance.Post(this.OnAcceptComplete, e);
                    break;
                default:
                    throw new Exception($"socket error: {e.LastOperation}");
            }
        }

        /// <summary>
        /// 连接服务器成功回调
        /// </summary>
        /// <param name="o"></param>
		private void OnAcceptComplete(object o)
		{
			if (this.acceptor == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
			
			if (e.SocketError != SocketError.Success)
			{
				Log.Error($"accept error {e.SocketError}");
				return;
			}
			TChannel channel = new TChannel(e.AcceptSocket, this);//根据远程端的Socket 得到和它的通讯信道TChannel
            this.idChannels[channel.Id] = channel;//注册到字典

            try
			{
				this.OnAccept(channel);  //把这个远程端Socket（这里就是服务器Socket）抛到上层 
            }
			catch (Exception exception)
			{
				Log.Error(exception);
			}

			if (this.acceptor == null)
			{
				return;
			}
			
			this.AcceptAsync();    //开始下一个远程端的监听
        }

        /// <summary>
        /// 根据ID得到一个远程端连接
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="id">Identifier.</param>
        public override AChannel GetChannel(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

        /// <summary>
        /// 根据IP地址创建新的远程端channel（就是远程端Socket） 但是这时并没有连接服务器成功，需要在channel初始化Start()
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="ipEndPoint">远程端IP.</param>
        public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			TChannel channel = new TChannel(ipEndPoint, this);
			this.idChannels[channel.Id] = channel;

			return channel;
		}

        /// <summary>
        /// 根据IP地址创建新的远程端channel
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
		public override AChannel ConnectChannel(string address)
		{
			IPEndPoint ipEndPoint = NetworkHelper.ToIPEndPoint(address);
			return this.ConnectChannel(ipEndPoint);
		}

		public void MarkNeedStartSend(long id)
		{
			this.needStartSendChannel.Add(id);
		}

        /// <summary>
        /// 移除一个远程端连接
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="id">Identifier.</param>
        public override void Remove(long id)
		{
			TChannel channel;
			if (!this.idChannels.TryGetValue(id, out channel))
			{
				return;
			}
			if (channel == null)
			{
				return;
			}
			this.idChannels.Remove(id);
			channel.Dispose();
		}

		public override void Update()
		{
            //不是一直发消息的 ，如果消息缓存中的发完了 ，就会停止发消息，
            //这个时候就不会一直查询缓存中是否有消息，只有再次发消息后，标记进了字典 才开始新的一轮发送
			foreach (long id in this.needStartSendChannel)
			{
				TChannel channel;
				if (!this.idChannels.TryGetValue(id, out channel))
				{
					continue;
				}

				if (channel.IsSending)
				{
					continue;
				}

				try
				{
					channel.StartSend();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
			
			this.needStartSendChannel.Clear();
		}
	}
}