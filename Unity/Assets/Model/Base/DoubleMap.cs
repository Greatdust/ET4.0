using System;
using System.Collections.Generic;

namespace ETModel
{
    /// <summary>
    /// 可双向读取的字典；根据键读取值，值读取键
    /// </summary>
    public class DoubleMap<K, V>
	{
		private readonly Dictionary<K, V> kv = new Dictionary<K, V>();
		private readonly Dictionary<V, K> vk = new Dictionary<V, K>();

		public DoubleMap()
		{
		}

		public DoubleMap(int capacity)
		{
			kv = new Dictionary<K, V>(capacity);
			vk = new Dictionary<V, K>(capacity);
		}

        /// <summary>
		/// 传入一个委托 用MAP里面的值遍历委托、
		/// </summary>
		/// <param name="action">Action.</param>
		public void ForEach(Action<K, V> action)
		{
			if (action == null)
			{
				return;
			}
			Dictionary<K, V>.KeyCollection keys = kv.Keys;
			foreach (K key in keys)
			{
				action(key, kv[key]);
			}
		}

        /// <summary>
        /// 返回所有的Keys
        /// </summary>
        /// <value>The keys.</value>
        public List<K> Keys
		{
			get
			{
				return new List<K>(kv.Keys);
			}
		}

        /// <summary>
        /// 返回所有的Values
        /// </summary>
        /// <value>The keys.</value>
        public List<V> Values
		{
			get
			{
				return new List<V>(vk.Keys);
			}
		}

        /// <summary>
        /// 添加一个元素到字典中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
		public void Add(K key, V value)
		{
			if (key == null || value == null || kv.ContainsKey(key) || vk.ContainsKey(value))
			{
				return;
			}
			kv.Add(key, value);
			vk.Add(value, key);
		}

        /// <summary>
        /// 根据key取得value值 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public V GetValueByKey(K key)
		{
			if (key != null && kv.ContainsKey(key))
			{
				return kv[key];
			}
			return default(V);
		}

        /// <summary>
        /// 根据value取得key值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public K GetKeyByValue(V value)
		{
			if (value != null && vk.ContainsKey(value))
			{
				return vk[value];
			}
			return default(K);
		}

        /// <summary>
        /// 根据key移除一个元素
        /// </summary>
        /// <param name="key">key</param>
        public void RemoveByKey(K key)
		{
			if (key == null)
			{
				return;
			}
			V value;
			if (!kv.TryGetValue(key, out value))
			{
				return;
			}

			kv.Remove(key);
			vk.Remove(value);
		}

        /// <summary>
        /// 根据value移除一个元素
        /// </summary>
        /// <param name="value"></param>
		public void RemoveByValue(V value)
		{
			if (value == null)
			{
				return;
			}

			K key;
			if (!vk.TryGetValue(value, out key))
			{
				return;
			}

			kv.Remove(key);
			vk.Remove(value);
		}

        /// <summary>
        /// 清除字典
        /// </summary>
		public void Clear()
		{
			kv.Clear();
			vk.Clear();
		}

        /// <summary>
        /// 字典中是否包含Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public bool ContainsKey(K key)
		{
			if (key == null)
			{
				return false;
			}
			return kv.ContainsKey(key);
		}

        /// <summary>
        /// 字典中是否包含value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool ContainsValue(V value)
		{
			if (value == null)
			{
				return false;
			}
			return vk.ContainsKey(value);
		}

        /// <summary>
        /// 是否包含key 和 value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
		public bool Contains(K key, V value)
		{
			if (key == null || value == null)
			{
				return false;
			}
			return kv.ContainsKey(key) && vk.ContainsKey(value);
		}
	}
}