namespace ETModel
{
	public interface IMessage
	{
	}
    /// <summary>
    ///  回应
    /// </summary>
    public interface IRequest: IMessage
	{
		int RpcId { get; set; }
	}
    /// <summary>
    ///  回应
    /// </summary>
    public interface IResponse : IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
		int RpcId { get; set; }
	}
    /// <summary>
	/// 应答消息
	/// </summary>
	public class ResponseMessage : IResponse
	{
		public int Error { get; set; }
		public string Message { get; set; }
		public int RpcId { get; set; }
	}
}