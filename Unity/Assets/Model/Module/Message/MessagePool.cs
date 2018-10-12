using System;
using System.Collections.Generic;

namespace ETModel
{
    /// <summary>
    ///  客户端为了0GC需要消息池，服务端消息需要跨协程不需要消息池
    /// </summary>
    public class MessagePool
	{
		public static MessagePool Instance { get; } = new MessagePool();

#if !SERVER
		private readonly Dictionary<Type, Queue<object>> dictionary = new Dictionary<Type, Queue<object>>();
#endif
        /// <summary>
        /// 取出一个消息类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public object Fetch(Type type)
		{
#if !SERVER
			Queue<object> queue;
			if (!this.dictionary.TryGetValue(type, out queue))
			{
				queue = new Queue<object>();
				this.dictionary.Add(type, queue);
			}

			object obj;
			if (queue.Count > 0)
			{
				obj = queue.Dequeue();
			}
			else
			{
				obj = Activator.CreateInstance(type);
			}

			return obj;
#else
			return Activator.CreateInstance(type);
#endif
		}
        /// <summary>
        /// 取出一个消息类型泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		public T Fetch<T>() where T : class
		{
			T t = (T) this.Fetch(typeof (T));
			return t;
		}
        /// <summary>
        /// 回收消息类型
        /// </summary>
        /// <param name="obj"></param>
		public void Recycle(object obj)
		{
#if !SERVER
			Type type = obj.GetType();
			Queue<object> queue;
			if (!this.dictionary.TryGetValue(type, out queue))
			{
				queue = new Queue<object>();
				this.dictionary.Add(type, queue);
			}

			queue.Enqueue(obj);
#endif
		}
	}
}