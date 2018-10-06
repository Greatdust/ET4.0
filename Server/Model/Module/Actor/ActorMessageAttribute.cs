using System;

namespace ETModel
{
    /// <summary>
    ///  Actor消息特性
    /// </summary>
	public class ActorMessageAttribute : Attribute
	{
		public ushort Opcode { get; private set; }

		public ActorMessageAttribute(ushort opcode)
		{
			this.Opcode = opcode;
		}
	}
}