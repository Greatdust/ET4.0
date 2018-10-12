namespace ETModel
{
    /// <summary>
    ///  继承 Actor 请求消息 
    /// </summary>
	public interface IActorLocationMessage : IActorRequest
	{
	}

    /// <summary>
    /// 继承 Actor 请求消息 
    /// </summary>
	public interface IActorLocationRequest : IActorRequest
	{
	}

    /// <summary>
    /// 继承 Actor 响应消息 
    /// </summary>
    public interface IActorLocationResponse : IActorResponse
	{
	}
}