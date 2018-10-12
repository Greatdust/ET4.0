namespace ETModel
{
    /// <summary>
    /// Actor 普通消息接口 多了一个ActorId 这个消息是不需要回应的  基类没有RPC
    /// </summary>
    public interface IActorMessage: IMessage
	{
		long ActorId { get; set; }
	}

    /// <summary>
    /// Actor 请求消息 比普通消息 多了一个ActorId
    /// </summary>
	public interface IActorRequest : IRequest
	{
		long ActorId { get; set; }
	}
    /// <summary>
    /// Actor 响应消息 不需要 Actor Id 有RPC id 直接对应客户端
    /// </summary>
	public interface IActorResponse : IResponse
	{
	}

    /// <summary>
    /// 帧消息 多一个  帧ID
    /// </summary>
	public interface IFrameMessage : IMessage
	{
		long Id { get; set; }
	}
}