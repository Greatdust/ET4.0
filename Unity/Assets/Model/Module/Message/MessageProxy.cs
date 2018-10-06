using System;

namespace ETModel
{
    /// <summary>
	/// 消息处理函数代理
	/// </summary>
	public class MessageProxy: IMHandler
	{
		private readonly Type type;
		private readonly Action<Session, object> action;   //在调用这个处理消息的函数时，会调用委托

        public MessageProxy(Type type, Action<Session, object> action)
		{
			this.type = type;
			this.action = action;
		}
		
		public void Handle(Session session, object message)
		{
			this.action.Invoke(session, message);
		}

		public Type GetMessageType()
		{
			return this.type;
		}
	}
}
