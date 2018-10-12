namespace ETModel
{
    /// <summary>
    ///  普通消息
    /// </summary>
	public interface IMessage
	{
	}
    /// <summary>
    ///  请求 只有一个RPC id
    /// </summary>
    public interface IRequest: IMessage
	{
		int RpcId { get; set; }
	}
    /// <summary>
    ///  响应 有RPC id 错误代码 和 string消息
    /// </summary>
    public interface IResponse : IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
		int RpcId { get; set; }
	}
    /// <summary>
	/// 响应 有RPC id 错误代码 和 string消息 
	/// </summary>
	public class ResponseMessage : IResponse
	{
		public int Error { get; set; }
		public string Message { get; set; }
		public int RpcId { get; set; }
	}
}