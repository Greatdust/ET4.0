using System.Collections.Generic;

namespace ETModel
{
    /// <summary>
    /// 堆栈字典 先进先出
    /// </summary>
    public class QueueDictionary<T, K>
	{
        /// <summary>
        /// 保存所有的KEY
        /// </summary>
        private readonly List<T> list = new List<T>();
		private readonly Dictionary<T, K> dictionary = new Dictionary<T, K>();

        /// <summary>
        /// 入栈的时候 由List记录顺序
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="k">K.</param>
        public void Enqueue(T t, K k)
		{
			this.list.Add(t);
			this.dictionary.Add(t, k);
		}

        /// <summary>
        /// 出站 获得List第一个KEY
        /// </summary>
        public void Dequeue()
		{
			if (this.list.Count == 0)
			{
				return;
			}
			T t = this.list[0];
			this.list.RemoveAt(0);
			this.dictionary.Remove(t);
		}

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="t">T.</param>
        public void Remove(T t)
		{
			this.list.Remove(t);
			this.dictionary.Remove(t);
		}

        /// <summary>
        /// 字典中是否有key
        /// </summary>
        /// <returns><c>true</c>, if key was containsed, <c>false</c> otherwise.</returns>
        /// <param name="t">T.</param>
        public bool ContainsKey(T t)
		{
			return this.dictionary.ContainsKey(t);
		}

        /// <summary>
        /// key的数量
        /// </summary>
        /// <value>The count.</value>
        public int Count
		{
			get
			{
				return this.list.Count;
			}
		}

        /// <summary>
        /// 棧中第一个key
        /// </summary>
        /// <value>The first key.</value>
        public T FirstKey
		{
			get
			{
				return this.list[0];
			}
		}

        /// <summary>
        /// 字典中第一个Value（由list确定顺序）
        /// </summary>
        /// <value>The first value.</value>
        public K FirstValue
		{
			get
			{
				T t = this.list[0];
				return this[t];
			}
		}

        /// <summary>
        /// 取出制定KEY的值
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
		public K this[T t]
		{
			get
			{
				return this.dictionary[t];
			}
		}

		public void Clear()
		{
			this.list.Clear();
			this.dictionary.Clear();
		}
	}
}