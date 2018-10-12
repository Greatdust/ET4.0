using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;

#if SERVER
using System.Runtime.InteropServices;
#endif

namespace ETModel
{
    /// <summary>
    /// Kcp 协议模式  连接  确认 断开
    /// </summary>
    public static class KcpProtocalType
	{
		public const byte SYN = 1;
		public const byte ACK = 2;
		public const byte FIN = 3;
		public const byte MSG = 4;
	}

	public sealed class KService : AService
	{
		public static KService Instance { get; private set; }
        /// <summary>
        /// ID生成器
        /// </summary>
        private uint IdGenerater = 1000;

		// KService创建的时间
		public long StartTime;
		// 当前时间 - KService创建的时间
		public uint TimeNow { get; private set; }
        /// <summary>
        /// 客户端服务socket
        /// </summary>
        private Socket socket;
        /// <summary>
        /// 保存了所有的远程连接 socket
        /// </summary>
        private readonly Dictionary<long, KChannel> localConnChannels = new Dictionary<long, KChannel>();
        /// <summary>
        /// 缓存区
        /// </summary>
        private readonly byte[] cache = new byte[8192];
        /// <summary>
        /// 待移除远程连接 socket ID的堆栈
        /// </summary>
        private readonly Queue<long> removedChannels = new Queue<long>();

        /// <summary>
        ///  优化的StreamManager流
        /// </summary>
        public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

		#region 连接相关
		// 记录等待连接的channel，10秒后或者第一个消息过来才会从这个dict中删除
		private readonly Dictionary<uint, KChannel> waitConnectChannels = new Dictionary<uint, KChannel>();
		#endregion

		#region 定时器相关
		// 下帧要更新的channel
		private readonly HashSet<long> updateChannels = new HashSet<long>();
        /// <summary>
        /// 下次时间更新的channel(socket) （保存 时间  ID）
        /// </summary>
        private readonly MultiMap<long, long> timeId = new MultiMap<long, long>();
        /// <summary>
        /// 到了时间需要更新的channel(socket)的ID
        /// </summary>
        private readonly List<long> timeOutTime = new List<long>();
		// 记录最小时间，不用每次都去MultiMap取第一个值
		private long minTime;
        #endregion

        /// <summary>
        /// IP缓存
        /// </summary>
        private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        /// <summary>
        /// 初始化客户端服务器
        /// </summary>
        /// <param name="ipEndPoint">Ip end point.</param>
        public KService(IPEndPoint ipEndPoint, Action<AChannel> acceptCallback)
		{
			this.InstanceId = ETModel.IdGenerater.GenerateId();
			
			this.AcceptCallback += acceptCallback;
			
			this.StartTime = TimeHelper.ClientNow();
			this.TimeNow = (uint)(TimeHelper.ClientNow() - this.StartTime);
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//this.socket.Blocking = false;
			this.socket.Bind(ipEndPoint);
#if SERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				const uint IOC_IN = 0x80000000;
				const uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				this.socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
			}
#endif
			Instance = this;
		}

		public KService()
		{
			this.InstanceId = ETModel.IdGenerater.GenerateId();
			
			this.StartTime = TimeHelper.ClientNow();
			this.TimeNow = (uint)(TimeHelper.ClientNow() - this.StartTime);
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//this.socket.Blocking = false;
			this.socket.Bind(new IPEndPoint(IPAddress.Any, 0));
#if SERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				const uint IOC_IN = 0x80000000;
				const uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				this.socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
			}
#endif
			Instance = this;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (KeyValuePair<long, KChannel> keyValuePair in this.localConnChannels)
			{
				keyValuePair.Value.Dispose();
			}

			this.socket.Close();
			this.socket = null;
			Instance = null;
		}

		public void Recv()
        {
            //如果当前使用的是非阻止 Socket，一种较好的做法是在调用 Receive 之前使用 Available 
            //来确定数据是否排队等待读取。可用的数据即网络缓冲区中排队等待读取的全部数据。
            //如果在网络缓冲区中没有排队的数据，则 Available 返回 0。
            if (this.socket == null)
			{
				return;
			}

			while (socket != null && this.socket.Available > 0)
			{
				int messageLength = 0;
				try
                {
                    //接收到通讯数据   ref this.ipEndPoint 保存远程端IP
                    messageLength = this.socket.ReceiveFrom(this.cache, ref this.ipEndPoint);
				}
				catch (Exception e)
				{
					Log.Error(e);
					continue;
				}

				// 长度小于1，不是正常的消息
				if (messageLength < 1)
				{
					continue;
				}
				// accept
				byte flag = this.cache[0];
				
				// conn从1000开始，如果为1，2，3则是特殊包
				uint remoteConn = 0;
				uint localConn = 0;
				KChannel kChannel = null;
				switch (flag)
				{
					case KcpProtocalType.SYN:  // accept //收到连接请求
                         // 长度!=5，不是accpet消息
                        if (messageLength != 5)
						{
							break;
						}

						IPEndPoint acceptIpEndPoint = (IPEndPoint)this.ipEndPoint;
						this.ipEndPoint = new IPEndPoint(0, 0);

						remoteConn = BitConverter.ToUInt32(this.cache, 1);

						// 如果已经收到连接，则忽略
						if (this.waitConnectChannels.TryGetValue(remoteConn, out kChannel))
						{
							break;
						}

						localConn = ++this.IdGenerater;
						kChannel = new KChannel(localConn, remoteConn, this.socket, acceptIpEndPoint, this);
						this.localConnChannels[kChannel.LocalConn] = kChannel;
						this.waitConnectChannels[remoteConn] = kChannel;

						this.OnAccept(kChannel);

						break;
					case KcpProtocalType.ACK:  // connect返回  //2次握手
                                               // 长度!=9，不是connect消息
                        if (messageLength != 9)
						{
							break;
						}
						remoteConn = BitConverter.ToUInt32(this.cache, 1);   //请求的KChannel ID
                        localConn = BitConverter.ToUInt32(this.cache, 5);

						kChannel = this.GetKChannel(localConn);     //如果字典里面没有 说明没有连接过 没必要2次握手 退出不处理
                        if (kChannel != null)
						{
							kChannel.HandleConnnect(remoteConn);
						}
						break;
					case KcpProtocalType.FIN:  // 断开  //收到断开请求
                                               // 长度!=13，不是DisConnect消息
                        if (messageLength != 13)
						{
							break;
						}

						remoteConn = BitConverter.ToUInt32(this.cache, 1);
						localConn = BitConverter.ToUInt32(this.cache, 5);

						// 处理chanel
						kChannel = this.GetKChannel(localConn);
						if (kChannel != null)
						{
							// 校验remoteConn，防止第三方攻击
							if (kChannel.RemoteConn == remoteConn)
							{
								kChannel.Disconnect(ErrorCode.ERR_PeerDisconnect);
							}
						}
						break;
					case KcpProtocalType.MSG:  // 断开？
						// 长度<9，不是Msg消息
						if (messageLength < 9)
						{
							break;
						}
						// 处理chanel
						remoteConn = BitConverter.ToUInt32(this.cache, 1);
						localConn = BitConverter.ToUInt32(this.cache, 5);

						this.waitConnectChannels.Remove(remoteConn);
						
						kChannel = this.GetKChannel(localConn);
						if (kChannel != null)
						{
							// 校验remoteConn，防止第三方攻击
							if (kChannel.RemoteConn == remoteConn)
							{
                                //处理普通消息
                                kChannel.HandleRecv(this.cache, 5, messageLength - 5);
							}
						}
						break;
				}
			}
		}

        /// <summary>
        /// 根据ID得到远程端Socket  ？
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="id">Identifier.</param>
        public KChannel GetKChannel(long id)
		{
			AChannel aChannel = this.GetChannel(id);
			if (aChannel == null)
			{
				return null;
			}

			return (KChannel)aChannel;
		}

		public override AChannel GetChannel(long id)
		{
			if (this.removedChannels.Contains(id))
			{
				return null;
			}
			KChannel channel;
			this.localConnChannels.TryGetValue(id, out channel);
			return channel;
		}

		public static void Output(IntPtr bytes, int count, IntPtr user)
		{
			if (Instance == null)
			{
				return;
			}
			AChannel aChannel = Instance.GetChannel((uint)user);
			if (aChannel == null)
			{
				Log.Error($"not found kchannel, {(uint)user}");
				return;
			}

			KChannel kChannel = aChannel as KChannel;
			kChannel.Output(bytes, count);
		}

        /// <summary>
        /// 根据IP地址创建新的远程端channel
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="ipEndPoint">Ip end point.</param>
        /// <param name="remoteEndPoint">Remote end point.</param>
        public override AChannel ConnectChannel(IPEndPoint remoteEndPoint)
		{
			uint localConn = (uint)RandomHelper.RandomNumber(1000, int.MaxValue);
			KChannel oldChannel;
			if (this.localConnChannels.TryGetValue(localConn, out oldChannel))
			{
				this.localConnChannels.Remove(oldChannel.LocalConn);
				oldChannel.Dispose();
			}

			KChannel channel = new KChannel(localConn, this.socket, remoteEndPoint, this);
			this.localConnChannels[channel.LocalConn] = channel;
			return channel;
		}

        /// <summary>
        /// 根据IP地址创建新的远程端channel
        /// </summary>
        /// <returns>The channel.</returns>
        /// <param name="ipEndPoint">Ip end point.</param>
        /// <param name="remoteEndPoint">Remote end point.</param>
        public override AChannel ConnectChannel(string address)
		{
			IPEndPoint ipEndPoint2 = NetworkHelper.ToIPEndPoint(address);
			return this.ConnectChannel(ipEndPoint2);
		}

        /// <summary>
        /// 移除一个远程端Socket
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="id">Identifier.</param>
        public override void Remove(long id)
		{
			KChannel channel;
			if (!this.localConnChannels.TryGetValue(id, out channel))
			{
				return;
			}
			if (channel == null)
			{
				return;
			}
			this.removedChannels.Enqueue(id);

			// 删除channel时检查等待连接状态的字典是否要清除
			KChannel waitConnectChannel;
			if (this.waitConnectChannels.TryGetValue(channel.RemoteConn, out waitConnectChannel))
			{
				if (waitConnectChannel.LocalConn == channel.LocalConn)
				{
					this.waitConnectChannels.Remove(channel.RemoteConn);
				}
			}
		}

#if !SERVER
		// 客户端channel很少,直接每帧update所有channel即可,这样可以消除TimerOut方法的gc
		public void AddToUpdateNextTime(long time, long id)
		{
		}
		
		public override void Update()
		{
			this.TimeNow = (uint) (TimeHelper.ClientNow() - this.StartTime);

			this.Recv();

			foreach (var kv in this.localConnChannels)
			{
				kv.Value.Update();
			}
			
			while (this.removedChannels.Count > 0)
			{
				long id = this.removedChannels.Dequeue();
				KChannel channel;
				if (!this.localConnChannels.TryGetValue(id, out channel))
				{
					continue;
				}
				this.localConnChannels.Remove(id);
				channel.Dispose();
			}
		}
#else
		// 服务端需要看channel的update时间是否已到
		public void AddToUpdateNextTime(long time, long id)
		{
			if (time == 0)
			{	
				this.updateChannels.Add(id);
				return;
			}
			if (time < this.minTime)
			{
				this.minTime = time;
			}
			this.timeId.Add(time, id);
		}

		public override void Update()
		{
			this.TimeNow = (uint)(TimeHelper.ClientNow() - this.StartTime);
			
			this.Recv();
			
			this.TimerOut();

			foreach (long id in updateChannels)
			{
				KChannel kChannel = this.GetKChannel(id);
				if (kChannel == null)
				{
					continue;
				}
				if (kChannel.Id == 0)
				{
					continue;
				}
				kChannel.Update();
			}
			this.updateChannels.Clear();

			while (this.removedChannels.Count > 0)
			{
				long id = this.removedChannels.Dequeue();
				KChannel channel;
				if (!this.localConnChannels.TryGetValue(id, out channel))
				{
					continue;
				}
				this.localConnChannels.Remove(id);
				channel.Dispose();
			}
		}

		// 计算到期需要update的channel
		private void TimerOut()
		{
			if (this.timeId.Count == 0)
			{
				return;
			}

			uint timeNow = this.TimeNow;

			if (timeNow < this.minTime)
			{
				return;
			}
			this.timeOutTime.Clear();

			foreach (KeyValuePair<long, List<long>> kv in this.timeId.GetDictionary())
			{
				long k = kv.Key;
				if (k > timeNow)
				{
					minTime = k;
					break;
				}
				this.timeOutTime.Add(k);
			}
			foreach (long k in this.timeOutTime)
			{
				foreach (long v in this.timeId[k])
				{
					this.updateChannels.Add(v);
				}

				this.timeId.Remove(k);
			}
		}
#endif
	}
}