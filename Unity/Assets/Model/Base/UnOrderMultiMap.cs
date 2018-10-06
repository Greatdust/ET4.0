using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    /// <summary>
	/// 这个是无序Dictionary优化的字典，在移除时会把List<K>回收到棧中，以便下一次添加的时候从棧中取值 节省GC  这里的K 其实是LIST<>
	/// </summary>
	public class UnOrderMultiMap<T, K>
	{
        //一个普通字典  这个字典在初始化的时候就必须赋值 是只读字典
        private readonly Dictionary<T, List<K>> dictionary = new Dictionary<T, List<K>>();

        /// <summary>
        /// 重用list 棧
        /// </summary>
        private readonly Queue<List<K>> queue = new Queue<List<K>>();

        /// <summary>
        /// 返回字典实例
        /// </summary>
        /// <returns>The dictionary.</returns>
        public Dictionary<T, List<K>> GetDictionary()
		{
			return this.dictionary;
		}

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="k">K.</param>
        public void Add(T t, K k)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				list = this.FetchList();
				this.dictionary[t] = list;
			}
			list.Add(k);
		}


        /// <summary>
        /// 返回字典中第一个实例？？
        /// </summary>
        public KeyValuePair<T, List<K>> First()
		{
			return this.dictionary.First();
		}


        /// <summary>
        /// 字典中元素的数量
        /// </summary>
        /// <value>The count.</value>
        public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}

        /// <summary>
        /// 得到一个空的LIST
        /// </summary>
        /// <returns>The list.</returns>
        private List<K> FetchList()
		{
			if (this.queue.Count > 0)
			{
				List<K> list = this.queue.Dequeue();
				list.Clear();
				return list;
			}
			return new List<K>();
		}


        /// <summary>
        /// 回收 list.
        /// </summary>
        /// <param name="list">List.</param>
        private void RecycleList(List<K> list)
		{
			// 防止暴涨
			if (this.queue.Count > 100)
			{
				return;
			}
			list.Clear();
			this.queue.Enqueue(list);
		}


        /// <summary>
        /// 回从字典中移除一个元素
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="k">K.</param>
        public bool Remove(T t, K k)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}
			if (!list.Remove(k))
			{
				return false;
			}
			if (list.Count == 0) //如果这个字典中这一项没有元素了 就把list回收到棧中
            {
				this.RecycleList(list);
				this.dictionary.Remove(t);//从字典里面移除这一项
            }
			return true;
		}


        /// <summary>
        /// 从字典中移除一个KEY
        /// </summary>
        /// <param name="t">T.</param>
        public bool Remove(T t)
		{
			List<K> list = null;
			this.dictionary.TryGetValue(t, out list);
			if (list != null)
			{
				this.RecycleList(list);
			}
			return this.dictionary.Remove(t);
		}

		/// <summary>
		/// 不返回内部的list,copy一份出来
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public K[] GetAll(T t)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return new K[0];
			}
			return list.ToArray();
		}

		/// <summary>
		/// 返回内部的list
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public List<K> this[T t]
		{
			get
			{
				List<K> list;
				this.dictionary.TryGetValue(t, out list);
				return list;
			}
		}

        /// <summary>
        /// 获得KEY中的一个元素
        /// </summary>
        /// <returns>The one.</returns>
        /// <param name="t">T.</param>
        public K GetOne(T t)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list != null && list.Count > 0)
			{
				return list[0];
			}
			return default(K);
		}


        /// <summary>
        /// 判断KEY中是否有此元素
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="k">K.</param>
        public bool Contains(T t, K k)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}
			return list.Contains(k);
		}


        /// <summary>
        /// 字典中是否有此KEY
        /// </summary>
        /// <returns><c>true</c>, if key was containsed, <c>false</c> otherwise.</returns>
        /// <param name="t">T.</param>
        public bool ContainsKey(T t)
		{
			return this.dictionary.ContainsKey(t);
		}

        /// <summary>
        /// 清空字典 把他们回收到棧中
        /// </summary>
        public void Clear()
		{
			foreach (KeyValuePair<T, List<K>> keyValuePair in this.dictionary)
			{
				this.RecycleList(keyValuePair.Value);
			}
			this.dictionary.Clear();
		}
	}
}