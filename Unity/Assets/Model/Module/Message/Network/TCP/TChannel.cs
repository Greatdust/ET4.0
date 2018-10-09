using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;

namespace ETModel
{
	/// <summary>
	/// 封装Socket,将回调push到主线程处理
	/// </summary>
	public sealed class TChannel: AChannel
	{
		private Socket socket;
        //用于写入，读取和接受套接字操作
        private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        //用于写入，读取和接受套接字操作
        private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();
        /// <summary>
        /// 接收缓冲区
        /// </summary>
        private readonly CircularBuffer recvBuffer = new CircularBuffer();
        /// <summary>
        /// 发送缓冲区
        /// </summary>
        private readonly CircularBuffer sendBuffer = new CircularBuffer();
        /// <summary>
        /// 流
        /// </summary>
        private readonly MemoryStream memoryStream;
        /// <summary>
        /// 是否正在发送数据
        /// </summary>
        private bool isSending;
        /// <summary>
        /// 是否已经接收数据
        /// </summary>
        private bool isRecving;
        /// <summary>
        /// 是否已经连接服务器
        /// </summary>
        private bool isConnected;
        /// <summary>
        /// 解析器
        /// </summary>
        private readonly PacketParser parser;

        private readonly byte[] cache = new byte[Packet.SizeLength];
        /// <summary>
        /// 初始化远程端Socket  （这里只是给了远程端的IP，所以并没有和远程端建立连接）
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <param name="service"></param>
        public TChannel(IPEndPoint ipEndPoint, TService service): base(service, ChannelType.Connect)
		{
			this.InstanceId = IdGenerater.GenerateId();
            this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
			
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.socket.NoDelay = true; // Socket 使用 Nagle 算法
			this.parser = new PacketParser(this.recvBuffer, this.memoryStream);
			this.innArgs.Completed += this.OnComplete; //接收到数据的回调
            this.outArgs.Completed += this.OnComplete;   //接收到数据的回调

			this.RemoteAddress = ipEndPoint;

			this.isConnected = false;
			this.isSending = false;
		}

        /// <summary>
        /// 初始化远程端Socket ，已经连接了 所以传入的参数是Socket 而不是远程IP地址
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="service"></param>
        public TChannel(Socket socket, TService service): base(service, ChannelType.Accept)
		{
			this.InstanceId = IdGenerater.GenerateId();
			this.memoryStream = this.GetService().MemoryStreamManager.GetStream("message", ushort.MaxValue);
			
			this.socket = socket;
			this.socket.NoDelay = true;
			this.parser = new PacketParser(this.recvBuffer, this.memoryStream);
			this.innArgs.Completed += this.OnComplete;
			this.outArgs.Completed += this.OnComplete;

			this.RemoteAddress = (IPEndPoint)socket.RemoteEndPoint;
			
			this.isConnected = true;
			this.isSending = false;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
			
			this.socket.Close();
			this.innArgs.Dispose();
			this.outArgs.Dispose();
			this.innArgs = null;
			this.outArgs = null;
			this.socket = null;
			this.memoryStream.Dispose();
		}
		
		private TService GetService()
		{
			return (TService)this.service;
		}

		public override MemoryStream Stream
		{
			get
			{
				return this.memoryStream;
			}
		}

		public override void Start()
		{
			if (!this.isConnected)  //如果没有连接服务器   （使用远程IP初始化这个socket的）
            {
				this.ConnectAsync(this.RemoteAddress);
				return;
			}

			if (!this.isRecving)
			{
				this.isRecving = true;
				this.StartRecv();  //开始接收
            }

			this.GetService().MarkNeedStartSend(this.Id);  //开始发送
        }

        /// <summary>
        /// 发送消息 对外接口
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="index">Index.</param>
        /// <param name="length">Length.</param>
        public override void Send(MemoryStream stream)
		{
			if (this.IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}

            switch (Packet.SizeLength)
            {
                case 4:
                    this.cache.WriteTo(0, (int)stream.Length);
                    break;
                case 2:
                    this.cache.WriteTo(0, (ushort)stream.Length);
                    break;
                default:
                    throw new Exception("packet size must be 2 or 4!");
            }

            this.sendBuffer.Write(this.cache, 0, this.cache.Length);
            this.sendBuffer.Write(stream);

            this.GetService().MarkNeedStartSend(this.Id);
		}

		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Connect: //连接成功
                    OneThreadSynchronizationContext.Instance.Post(this.OnConnectComplete, e);
					break;
				case SocketAsyncOperation.Receive: //接收成功
                    OneThreadSynchronizationContext.Instance.Post(this.OnRecvComplete, e);
					break;
				case SocketAsyncOperation.Send:  //发送成功
                    OneThreadSynchronizationContext.Instance.Post(this.OnSendComplete, e);
					break;
				case SocketAsyncOperation.Disconnect:   //断开
                    OneThreadSynchronizationContext.Instance.Post(this.OnDisconnectComplete, e);
					break;
				default:
					throw new Exception($"socket error: {e.LastOperation}");
			}
		}

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ipEndPoint">Ip end point.</param>
        public void ConnectAsync(IPEndPoint ipEndPoint)
		{
			this.outArgs.RemoteEndPoint = ipEndPoint;
			if (this.socket.ConnectAsync(this.outArgs))
			{
				return;
			}
			OnConnectComplete(this.outArgs);
		}

        /// <summary>
        /// 连接完成回调
        /// </summary>
        /// <param name="o">O.</param>
        private void OnConnectComplete(object o)
		{
			if (this.socket == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) o;
			
			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);	
				return;
			}

			e.RemoteEndPoint = null;
			this.isConnected = true;
			
			this.Start();
		}
        /// <summary>
        /// 断开完成回调
        /// </summary>
        /// <param name="o"></param>
		private void OnDisconnectComplete(object o)
		{
			SocketAsyncEventArgs e = (SocketAsyncEventArgs)o;
			this.OnError((int)e.SocketError);    //抛出错误信息
        }

        /// <summary>
        /// 开启接收数据
        /// </summary>
        private void StartRecv()
		{
			int size = this.recvBuffer.ChunkSize - this.recvBuffer.LastIndex;
			this.RecvAsync(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);
		}

		public void RecvAsync(byte[] buffer, int offset, int count)
		{
			try
			{
				this.innArgs.SetBuffer(buffer, offset, count);
			}
			catch (Exception e)
			{
				throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
			}
			
			if (this.socket.ReceiveAsync(this.innArgs))
			{
				return;
			}
			OnRecvComplete(this.innArgs);
		}

        /// <summary>
        /// 接收数据完成回调
        /// </summary>
        /// <param name="o">O.</param>
        private void OnRecvComplete(object o)
		{
			if (this.socket == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) o;

			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);
				return;
			}

			if (e.BytesTransferred == 0)  //没有接收到数据
            {
				this.OnError(ErrorCode.ERR_PeerDisconnect);
				return;
			}

			this.recvBuffer.LastIndex += e.BytesTransferred;   //写入坐标+接收数据的大小
            if (this.recvBuffer.LastIndex == this.recvBuffer.ChunkSize)  //如果缓存区写满了
            {
				this.recvBuffer.AddLast();    //添加一块新的缓存区
                this.recvBuffer.LastIndex = 0; //写入坐标置0
            }

            // 收到消息回调 // 开始解析数据  一次把环形接收区里的完整包全部解析出来 发出去
            while (true)
			{
				if (!this.parser.Parse()) //解析了一个数据包就往下执行 ，如果没有成功解析到数据包  就跳出
                {
					break;
				}

				MemoryStream stream = this.parser.GetPacket(); //得到数据包
                try
				{
					this.OnRead(stream);  //通过委托，把包抛出去
                }
				catch (Exception exception)
				{
					Log.Error(exception);
				}
			}

			if (this.socket == null)
			{
				return;
			}
			
			this.StartRecv();    //重新开始接收数据
        }

		public bool IsSending => this.isSending;

        /// <summary>
        /// 开启发送   （会将缓存区数据全部发送出）
        /// </summary>
        public void StartSend()
		{
			if(!this.isConnected)
			{
				return;
			}
			
			// 没有数据需要发送
			if (this.sendBuffer.Length == 0)
			{
				this.isSending = false;
				return;
			}

			this.isSending = true;
            //缓存区大小 
            int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
            //如果环形缓存器里只有一块缓冲区 只需要发送已写入的数据大小 
            //如果sendSize< this.sendBuffer.Length  说明环形缓存器里有两块以上缓冲区  
            //这个时候不需要计算  先把第一块缓存区全部发送出去
            if (sendSize > this.sendBuffer.Length)
			{
				sendSize = (int)this.sendBuffer.Length;
			}

			this.SendAsync(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
		}

		public void SendAsync(byte[] buffer, int offset, int count)
		{
			try
			{
				this.outArgs.SetBuffer(buffer, offset, count); //填充缓存区
            }
			catch (Exception e)
			{
				throw new Exception($"socket set buffer error: {buffer.Length}, {offset}, {count}", e);
			}
			if (this.socket.SendAsync(this.outArgs))  //！！！！发送
            {
				return;
			}
			OnSendComplete(this.outArgs);  //完成回调
        }


        /// <summary>
        /// 发送完成回调
        /// </summary>
        /// <param name="o">O.</param>
        private void OnSendComplete(object o)
		{
			if (this.socket == null)
			{
				return;
			}
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) o;

			if (e.SocketError != SocketError.Success)
			{
				this.OnError((int)e.SocketError);
				return;
			}
			
			if (e.BytesTransferred == 0)
			{
				this.OnError(ErrorCode.ERR_PeerDisconnect);
				return;
			}
			
			this.sendBuffer.FirstIndex += e.BytesTransferred; //开始读取坐标+已发送的数据大小
            if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize)    //如果发送到了最后
            {
				this.sendBuffer.FirstIndex = 0;  //从头开始
                this.sendBuffer.RemoveFirst();   //移除这块内存（其实是放入缓存池了）
            }
			
			this.StartSend();   //又重新开启发送
        }
	}
}