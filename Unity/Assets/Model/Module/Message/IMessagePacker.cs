using System;
using System.IO;

namespace ETModel
{
    /// <summary>
    /// 消息包解析工具
    /// </summary>
    public interface IMessagePacker
	{
		byte[] SerializeTo(object obj);
		void SerializeTo(object obj, MemoryStream stream);
        /// <summary>
        /// 反序列化
        /// </summary>
        object DeserializeFrom(Type type, byte[] bytes, int index, int count);
		object DeserializeFrom(object instance, byte[] bytes, int index, int count);
		object DeserializeFrom(Type type, MemoryStream stream);
		object DeserializeFrom(object instance, MemoryStream stream);
	}
}
