using System;
using System.ComponentModel;
using System.IO;
using Google.Protobuf;

namespace ETModel
{
    /// <summary>
    /// Protobuf 工具类
    /// </summary>
    public static class ProtobufHelper
	{
        /// <summary>
		/// PB序列化 OBJ 序列化为 byte[]
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="message">Message.</param>
		public static byte[] ToBytes(object message)
		{
			return ((Google.Protobuf.IMessage) message).ToByteArray();
		}
		

		public static void ToStream(object message, MemoryStream stream)
		{
			((Google.Protobuf.IMessage) message).WriteTo(stream);
		}

        /// <summary>
        /// PB反序列化	byte[] 反序列化为object 
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static object FromBytes(Type type, byte[] bytes, int index, int count)
		{
			object message = Activator.CreateInstance(type);
			((Google.Protobuf.IMessage)message).MergeFrom(bytes, index, count);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
		
		public static object FromBytes(object instance, byte[] bytes, int index, int count)
		{
			object message = instance;
			((Google.Protobuf.IMessage)message).MergeFrom(bytes, index, count);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
		
		public static object FromStream(Type type, MemoryStream stream)
		{
			object message = Activator.CreateInstance(type);
			((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
		
		public static object FromStream(object message, MemoryStream stream)
		{
			// 这个message可以从池中获取，减少gc
			((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
			ISupportInitialize iSupportInitialize = message as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return message;
			}
			iSupportInitialize.EndInit();
			return message;
		}
	}
}