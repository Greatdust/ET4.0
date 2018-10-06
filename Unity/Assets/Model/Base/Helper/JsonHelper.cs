using System;
using System.ComponentModel;
using LitJson;

namespace ETModel
{
    /// <summary>
	/// Json 工具类
	/// </summary>
	public static class JsonHelper
	{
        /// <summary>
		/// json序列化
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="obj">Object.</param>
		public static string ToJson(object obj)
		{
			return JsonMapper.ToJson(obj);
		}

        /// <summary>
		/// json.反序列号(泛型)
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="str">String.</param>
		/// <typeparam name="T">范序列化类型.</typeparam>
		public static T FromJson<T>(string str)
		{
			T t = JsonMapper.ToObject<T>(str);
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

        /// <summary>
		/// json.反序列号	
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="type">Type.</param>
		/// <param name="str">String.</param>
		public static object FromJson(Type type, string str)
		{
			object t = JsonMapper.ToObject(type, str);
			ISupportInitialize iSupportInitialize = t as ISupportInitialize;
			if (iSupportInitialize == null)
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

        /// <summary>
        /// 克隆
        /// </summary>
        /// <param name="t">T.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Clone<T>(T t)
		{
			return FromJson<T>(ToJson(t));
		}
	}
}