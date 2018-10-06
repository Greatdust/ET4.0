namespace ETModel
{
     /// <summary>
     /// 消息派发类接口
     /// </summary>
    public interface IMessageDispatcher
	{
        /// <summary>
		/// 派发消息
		/// </summary>
		void Dispatch(Session session, ushort opcode, object message);
	}
}
