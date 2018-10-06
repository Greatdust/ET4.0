using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class SessionAwakeSystem : AwakeSystem<Session, AChannel>
	{
		public override void Awake(Session self, AChannel b)
		{
			self.Awake(b);
		}
	}

	public sealed class Session : Entity
	{
        /// <summary>
        /// 获取或设置rpc标识符。
        /// </summary>
        private static int RpcId { get; set; }
        /// <summary>
        /// 通讯socket
        /// </summary>
        private AChannel channel;
        /// <summary>
        /// RPC消息字典
        /// </summary>
        private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();
        /// <summary>
        /// 其实就是一个有三个byte数组的链表，new byte[0]意思是这个没有分配内存空间 ，可以用于数据交换，作为坐标来使用
        /// </summary>
        private readonly List<byte[]> byteses = new List<byte[]>() { new byte[1], new byte[2] };

		public NetworkComponent Network
		{
			get
			{
				return this.GetParent<NetworkComponent>();
			}
		}

		public int Error
		{
			get
			{
				return this.channel.Error;
			}
			set
			{
				this.channel.Error = value;
			}
		}

		public void Awake(AChannel aChannel)
		{
			this.channel = aChannel;
			this.requestCallback.Clear();
			long id = this.Id;
            //错误回调  会在NetworkComponent中报错并移除这个会话Session
            channel.ErrorCallback += (c, e) =>
			{
				this.Network.Remove(id); 
			};
			channel.ReadCallback += this.OnRead;  //收到通讯数据回调
        }
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();
			
			foreach (Action<IResponse> action in this.requestCallback.Values.ToArray())  //删除这个会话的RPC
            {
				action.Invoke(new ResponseMessage { Error = this.Error });
			}

			//int error = this.channel.Error;
			//if (this.channel.Error != 0)
			//{
			//	Log.Trace($"session dispose: {this.Id} ErrorCode: {error}, please see ErrorCode.cs!");
			//}
			
			this.channel.Dispose();
			this.Network.Remove(id);
			this.requestCallback.Clear();
		}

		public void Start()
		{
			this.channel.Start();
		}

        /// <summary>
        /// 获得远程地址
        /// </summary>
        public IPEndPoint RemoteAddress
		{
			get
			{
				return this.channel.RemoteAddress;
			}
		}


        /// <summary>
        /// 信道类型 连接？ 接受？
        /// </summary>
        public ChannelType ChannelType
		{
			get
			{
				return this.channel.ChannelType;
			}
		}

		public MemoryStream Stream
		{
			get
			{
				return this.channel.Stream;
			}
		}
        /// <summary>
        /// 收到通讯数据
        /// </summary>
        /// <param name="packet">Packet.</param>
        public void OnRead(MemoryStream memoryStream)
		{
			try
			{
				this.Run(memoryStream);  //处理
            }
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

        /// <summary>
        /// 处理收到的通讯数据
        /// </summary>
        /// <param name="packet">Packet.</param>
        private void Run(MemoryStream memoryStream)
		{
			memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
			byte flag = memoryStream.GetBuffer()[Packet.FlagIndex];
			ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
			
#if !SERVER  
			if (OpcodeHelper.IsClientHotfixMessage(opcode))
			{
				this.GetComponent<SessionCallbackComponent>().MessageCallback.Invoke(this, flag, opcode, memoryStream);
				return;
			}
#endif
            
            object message;
			try
			{
				OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>(); //操作码和类的映射关系
                object instance = opcodeTypeComponent.GetInstance(opcode); //根据操作码得到类
                message = this.Network.MessagePacker.DeserializeFrom(instance, memoryStream);   //将包里面的数据根据操作码找到的类型反序列化为object
                                                                                                //Log.Debug($"recv: {JsonHelper.ToJson(message)}");
            }
			catch (Exception e)
			{
				// 出现任何消息解析异常都要断开Session，防止客户端伪造消息
				Log.Error($"opcode: {opcode} {this.Network.Count} {e} ");
				this.Error = ErrorCode.ERR_PacketParserError;
				this.Network.Remove(this.Id);
				return;
			}

			// flag第一位为1表示这是rpc返回消息,否则交由MessageDispatcher分发
			if ((flag & 0x01) == 0)
			{
				this.Network.MessageDispatcher.Dispatch(this, opcode, message);
				return;
			}
				
			IResponse response = message as IResponse;
			if (response == null)
			{
				throw new Exception($"flag is response, but message is not! {opcode}");
			}
			Action<IResponse> action;   //如果回来的是RPC消息  查看RPC消息列表
            if (!this.requestCallback.TryGetValue(response.RpcId, out action))
			{
				return;
			}
			this.requestCallback.Remove(response.RpcId); //从字典中移除这个RPC消息

            action(response);   //处理这个消息 RPC收到消息
        }

        /// <summary>
        /// 发送一个RPC消息
        /// </summary>
        /// <param name="request">Request.</param>
        public Task<IResponse> Call(IRequest request)
		{
			int rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();
            //将RPC放到RPC字典中
            this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (ErrorCode.IsRpcNeedThrowException(response.Error))
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
				}
			};

			request.RpcId = rpcId;
			this.Send(0x00, request);  //发送消息
            return tcs.Task; //等待应答
        }

        /// <summary>
        /// 发送一个RPC消息  这个消息可以终止
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<IResponse> Call(IRequest request, CancellationToken cancellationToken)
		{
			int rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (ErrorCode.IsRpcNeedThrowException(response.Error))
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
				}
			};
            //终止
            cancellationToken.Register(() => this.requestCallback.Remove(rpcId));

			request.RpcId = rpcId;
			this.Send(0x00, request);
			return tcs.Task;
		}
        /// <summary>
        /// 发送消息 
        /// </summary>
        /// <param name="message">Message.</param>
        public void Send(IMessage message)
		{
			this.Send(0x00, message);
		}
        /// <summary>
        /// 回复（也是调用Send）
        /// </summary>
        /// <param name="message">Message.</param>
        public void Reply(IResponse message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}

			this.Send(0x01, message);
		}
        //发送消息 
        public void Send(byte flag, IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			
			Send(flag, opcode, message);
		}
        //发送消息 RPC  操作码  数据
        public void Send(byte flag, ushort opcode, object message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}

			MemoryStream stream = this.Stream;
			
			stream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
			stream.SetLength(Packet.MessageIndex);
			this.Network.MessagePacker.SerializeTo(message, stream);
			stream.Seek(0, SeekOrigin.Begin);
			
			this.byteses[0][0] = flag;
			this.byteses[1].WriteTo(0, opcode);
			int index = 0;
			foreach (var bytes in this.byteses)
			{
				Array.Copy(bytes, 0, stream.GetBuffer(), index, bytes.Length);
				index += bytes.Length;
			}

#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.Network.AppType == AppType.AllServer)
			{
				Session session = this.Network.Entity.GetComponent<NetInnerComponent>().Get(this.RemoteAddress);
				session.Run(stream);
				return;
			}
#endif

			this.Send(stream);
		}

		public void Send(MemoryStream stream)
		{
			channel.Send(stream);   //最终调用channel里面send
        }
	}
}