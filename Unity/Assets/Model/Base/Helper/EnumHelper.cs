using System;

namespace ETModel
{
	public static class EnumHelper
	{
        /// <summary>
        /// value在<T>枚举类型中的索引  //这个是值的索引
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static int EnumIndex<T>(int value)
		{
			int i = 0;
			foreach (object v in Enum.GetValues(typeof (T)))
			{
				if ((int) v == value)
				{
					return i;
				}
				++i;
			}
			return -1;
		}

        /// <summary>
        /// enumType 中有没有str这个项  有就返回  //这个是KEY的索引
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="str">String.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T FromString<T>(string str)
		{
            if (!Enum.IsDefined(typeof(T), str))
            {
                return default(T);
            }
            return (T)Enum.Parse(typeof(T), str);
        }
    }
}