using System;
using System.IO;
using Microsoft.IO;

namespace ETModel
{
    /// <summary>
    /// 数据包状态
    /// </summary>
    public enum ParserState
	{
        /// <summary>
		/// 包大小
		/// </summary>
		PacketSize,
        /// <summary>
		/// 包体
		/// </summary>
		PacketBody
    }


    /// <summary>
    /// 数据包
    /// </summary>
    public static class Packet
	{
        /// <summary>
        /// 数据包长度
        /// </summary>
        public const int SizeLength = 2;
        /// <summary>
        /// 数据包最小长度
        /// </summary>
        public const int MinSize = 3;
        /// <summary>
        /// 数据包最大长度
        /// </summary>
		public const int MaxSize = 60000;
        /// <summary>
        /// RPC标记
        /// </summary>
        public const int FlagIndex = 0;
        /// <summary>
        /// 操作码
        /// </summary>
        public const int OpcodeIndex = 1;
        /// <summary>
        /// 索引
        /// </summary>
		public const int MessageIndex = 3;
	}

	public class PacketParser
	{
        /// <summary>
        /// 数据缓存区
        /// </summary>
		private readonly CircularBuffer buffer;
        /// <summary>
        /// 包长度
        /// </summary>
        private ushort packetSize;
        /// <summary>
        /// 数据包状态（包大小，包体）
        /// </summary>
        private ParserState state;
        /// <summary>
        /// 流
        /// </summary>
		public MemoryStream memoryStream;

		private bool isOK;

		public PacketParser(CircularBuffer buffer, MemoryStream memoryStream)
		{
			this.buffer = buffer;
			this.memoryStream = memoryStream;
		}
        /// <summary>
        /// 解析完成一次数据包
        /// </summary>
        public bool Parse()
		{
			if (this.isOK)
			{
				return true;
			}

			bool finish = false;
			while (!finish)
			{
				switch (this.state)
				{
					case ParserState.PacketSize:
						if (this.buffer.Length < 2)
						{
							finish = true;
						}
						else
						{
							this.buffer.Read(this.memoryStream.GetBuffer(), 0, 2);
							packetSize = BitConverter.ToUInt16(this.memoryStream.GetBuffer(), 0);
							if (packetSize < Packet.MinSize || packetSize > Packet.MaxSize)
							{
								throw new Exception($"packet size error: {this.packetSize}");
							}
							this.state = ParserState.PacketBody;
						}
						break;
					case ParserState.PacketBody:
						if (this.buffer.Length < this.packetSize)
						{
							finish = true;
						}
						else
						{
							this.memoryStream.Seek(0, SeekOrigin.Begin);
							this.memoryStream.SetLength(this.packetSize);
							byte[] bytes = this.memoryStream.GetBuffer();
							this.buffer.Read(bytes, 0, this.packetSize);
							this.isOK = true;
							this.state = ParserState.PacketSize;
							finish = true;
						}
						break;
				}
			}
			return this.isOK;
		}
        /// <summary>
        /// 获得数据包
        /// </summary>
        /// <returns>The packet.</returns>
        public MemoryStream GetPacket()
		{
			this.isOK = false;
			return this.memoryStream;
		}
	}
}