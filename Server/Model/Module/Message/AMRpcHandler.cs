using System;

namespace ETModel
{
    /// <summary>
    /// 普通RPC消息处理基类
    /// </summary>
    /// <typeparam name="Request">发送参数的类型</typeparam>
    /// <typeparam name="Response">回复参数的类型</typeparam>
    public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse 
	{
        /// <summary>
        /// 回复错误信息
        /// </summary>
        /// <param name="response"></param>
        /// <param name="e"></param>
        /// <param name="reply"></param>
        protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e);
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}
        //由派生类重写的具体功能逻辑处理
        protected abstract void Run(Session session, Request message, Action<Response> reply);

		public void Handle(Session session, object message)
		{
			try
			{
				Request request = message as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
				}

				int rpcId = request.RpcId;

				long instanceId = session.InstanceId;
				
				this.Run(session, request, response =>
				{
					// 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
					if (session.InstanceId != instanceId)
					{
						return;
					}

					response.RpcId = rpcId;
					session.Reply(response);
				});
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
			}
		}
        /// <summary>
        ///  用传递过来的参数的类型 确定由那个函数处理
        /// </summary>
        /// <returns></returns>
		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}