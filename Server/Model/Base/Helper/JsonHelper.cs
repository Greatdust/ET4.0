using System;

namespace ETModel
{
	public static class JsonHelper
	{
        /// <summary>
        /// 将object序列化为string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
		public static string ToJson(object obj)
		{
			return MongoHelper.ToJson(obj);
		}

        /// <summary>
        /// 将string序列化为n<T>类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
		public static T FromJson<T>(string str)
		{
			return MongoHelper.FromJson<T>(str);
		}

        /// <summary>
        /// 传入string类型和需要转换的type 得到 object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
		public static object FromJson(Type type, string str)
		{
			return MongoHelper.FromJson(type, str);
		}


		public static T Clone<T>(T t)
		{
			return FromJson<T>(ToJson(t));
		}
	}
}