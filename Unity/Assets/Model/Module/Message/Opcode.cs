namespace ETModel
{
    /// <summary>
    /// Opcode.操作码
    /// </summary>
    public static partial class Opcode
	{
		public const ushort ActorResponse = 1;  //Actor请求消息
        public const ushort FrameMessage = 2; //帧同步消息
		public const ushort OneFrameMessage = 3; //一条帧同步消息
    }
}
