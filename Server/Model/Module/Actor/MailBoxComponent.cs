using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    /// 对Actor消息的封装 包含了会话通道 和 Actor消息体
    /// </summary>
    public struct ActorMessageInfo
	{
		public Session Session;
		public object Message;
	}

	/// <summary>
	/// 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
	/// </summary>
	public class MailBoxComponent: Component
	{
		// 拦截器类型，默认没有拦截器
		public string ActorInterceptType;

        /// <summary>
        /// Actor消息的封装队列处理
        /// </summary>
        public Queue<ActorMessageInfo> Queue = new Queue<ActorMessageInfo>();
        /// <summary>
        /// 异步任务返回值 参数类型为ActorMessageInfo
        /// </summary>
        public TaskCompletionSource<ActorMessageInfo> Tcs;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			var t = this.Tcs;
			this.Tcs = null;
			t?.SetResult(new ActorMessageInfo());
		}
	}
}