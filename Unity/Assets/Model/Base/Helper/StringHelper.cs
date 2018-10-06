using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ETModel
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringHelper
	{
        /// <summary>
		/// 字符串转为字节迭代接口
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="str">String.</param>
		public static IEnumerable<byte> ToBytes(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}

        /// <summary>
		/// 字符串转为字节数组
		/// </summary>
		/// <returns>The byte array.</returns>
		/// <param name="str">String.</param>
		public static byte[] ToByteArray(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}

        /// <summary>
		/// 字符串转为UTF8字节数组.
		/// </summary>
		/// <returns>The UTF8.</returns>
		/// <param name="str">String.</param>
	    public static byte[] ToUtf8(this string str)
	    {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

        /// <summary>
        /// 字符串转16位字节数组（两个字节）
        /// </summary>
        /// <returns>The to bytes.</returns>
        public static byte[] HexToBytes(this string hexString)
		{
			if (hexString.Length % 2 != 0)  //判断是否为双数 1个字母为1个字节 8位  这里的byte为2个字节16位
            {
				throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
			}

			var hexAsBytes = new byte[hexString.Length / 2];
			for (int index = 0; index < hexAsBytes.Length; index++)
			{
				string byteValue = "";
				byteValue += hexString[index * 2];
				byteValue += hexString[index * 2 + 1];
				hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return hexAsBytes;
		}

        /// <summary>
		/// 格式化字符串
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="args">Arguments.</param>
		public static string Fmt(this string text, params object[] args)
		{
			return string.Format(text, args);
		}

		public static string ListToString<T>(this List<T> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach (T t in list)
			{
				sb.Append(t);
				sb.Append(",");
			}
			return sb.ToString();
		}
	}
}