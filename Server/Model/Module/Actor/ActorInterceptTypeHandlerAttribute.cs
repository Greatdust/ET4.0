using System;

namespace ETModel
{
    /// <summary>
    /// Actor消息特性
    /// </summary>
    public class ActorInterceptTypeHandlerAttribute : BaseAttribute
	{
		public AppType Type { get; }

		public string ActorType { get; }

		public ActorInterceptTypeHandlerAttribute(AppType appType, string actorType)
		{
			this.Type = appType;
			this.ActorType = actorType;
		}
	}
}