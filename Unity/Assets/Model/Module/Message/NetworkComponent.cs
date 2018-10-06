using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ETModel
{
	public abstract class NetworkComponent : Component
    {
        /// <summary>
	    /// 服务器类型
	    /// </summary>
		public AppType AppType;
        /// <summary>
        /// 服务
        /// </summary>
        private AService Service;
        /// <summary>
        /// 管理所有的会话
        /// </summary>
        private readonly Dictionary<long, Session> sessions = new Dictionary<long, Session>();
        /// <summary>
        /// 消息包解析方式接口
        /// </summary>
        /// <value>The message packer.</value>
        public IMessagePacker MessagePacker { get; set; }
        /// <summary>
        /// 消息派发接口
        /// </summary>
        /// <value>The message dispatcher.</value>
        public IMessageDispatcher MessageDispatcher { get; set; }

		public void Awake(NetworkProtocol protocol)
		{
			try
			{
				switch (protocol)
				{
					case NetworkProtocol.KCP:
						this.Service = new KService();
						break;
					case NetworkProtocol.TCP:
						this.Service = new TService();
						break;
					case NetworkProtocol.WebSocket:
						this.Service = new WService();
						break;
				}
			}
			catch (Exception e)
			{
				throw new Exception($"{e}");
			}
		}

		public void Awake(NetworkProtocol protocol, string address)
		{
			try
			{
				IPEndPoint ipEndPoint;
				switch (protocol)
				{
					case NetworkProtocol.KCP:
						ipEndPoint = NetworkHelper.ToIPEndPoint(address);
						this.Service = new KService(ipEndPoint, this.OnAccept);
						break;
					case NetworkProtocol.TCP:
						ipEndPoint = NetworkHelper.ToIPEndPoint(address);
						this.Service = new TService(ipEndPoint, this.OnAccept);
						break;
					case NetworkProtocol.WebSocket:
						string[] prefixs = address.Split(';');
						this.Service = new WService(prefixs, this.OnAccept);
						break;
				}
			}
			catch (Exception e)
			{
				throw new Exception($"NetworkComponent Awake Error {address}", e);
			}
		}
        /// <summary>
        /// 会话的数量
        /// </summary>
        /// <value>The count.</value>
        public int Count
		{
			get { return this.sessions.Count; }
		}


        /// <summary>
        /// 得到连接到的远程端
        /// </summary>
        /// <param name="channel">Channel.</param>
        public void OnAccept(AChannel channel)
		{
			Session session = ComponentFactory.CreateWithParent<Session, AChannel>(this, channel);
			this.sessions.Add(session.Id, session);
			session.Start();
		}

        /// <summary>
        /// 移除会话
        /// </summary>
        /// <param name="id">Identifier.</param>
        public virtual void Remove(long id)
		{
			Session session;
			if (!this.sessions.TryGetValue(id, out session))
			{
				return;
			}
			this.sessions.Remove(id);
			session.Dispose();
		}

        /// <summary>
        /// 得到一个会话
        /// </summary>
        /// <param name="id">Identifier.</param>
        public Session Get(long id)
		{
			Session session;
			this.sessions.TryGetValue(id, out session);
			return session;
		}

		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public Session Create(IPEndPoint ipEndPoint)
		{
			AChannel channel = this.Service.ConnectChannel(ipEndPoint);
			Session session = ComponentFactory.CreateWithParent<Session, AChannel>(this, channel);
			this.sessions.Add(session.Id, session);
			session.Start();
			return session;
		}
		
		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public Session Create(string address)
		{
			AChannel channel = this.Service.ConnectChannel(address);
			Session session = ComponentFactory.CreateWithParent<Session, AChannel>(this, channel);
			this.sessions.Add(session.Id, session);
			session.Start();
			return session;
		}

		public void Update()
		{
			if (this.Service == null)
			{
				return;
			}
			this.Service.Update();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (Session session in this.sessions.Values.ToArray())
			{
				session.Dispose();
			}

			this.Service.Dispose();
		}
	}
}