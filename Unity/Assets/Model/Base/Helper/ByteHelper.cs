using System;
using System.Text;

namespace ETModel
{
    /// <summary>
    /// 字节工具类
    /// </summary>
    public static class ByteHelper
	{
        /// <summary>
		/// 字节转换为字符串
		/// </summary>
		/// <returns>The hex.</returns>
		/// <param name="b">The blue component.</param>
		public static string ToHex(this byte b)
		{
			return b.ToString("X2");
		}

        /// <summary>
		/// 字节数组转换字符串
		/// </summary>
		/// <returns>The hex.</returns>
		/// <param name="bytes">Bytes.</param>
		public static string ToHex(this byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}

        /// <summary>
		/// 字节数组转换字符串
		/// </summary>
		/// <returns>The hex.</returns>
		/// <param name="bytes">Bytes.</param>
		/// <param name="format">格式.</param>
		public static string ToHex(this byte[] bytes, string format)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString(format));
			}
			return stringBuilder.ToString();
		}


        /// <summary>
        /// 字节数组转换字符串
        /// </summary>
        /// <returns>The hex.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">起始位置.</param>
        /// <param name="count">长度.</param>
        public static string ToHex(this byte[] bytes, int offset, int count)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = offset; i < offset + count; ++i)
			{
				stringBuilder.Append(bytes[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}


        /// <summary>
        /// 字节数组转换字符串
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="bytes">Bytes.</param>
        public static string ToStr(this byte[] bytes)
		{
			return Encoding.Default.GetString(bytes);
		}

        /// <summary>
		/// 字节数组转换字符串
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="bytes">Bytes.</param>
		/// <param name="index">起始位置.</param>
		/// <param name="count">长度.</param>
		public static string ToStr(this byte[] bytes, int index, int count)
		{
			return Encoding.Default.GetString(bytes, index, count);
		}

        /// <summary>
        /// 字节数组转换UTF8字符串
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="bytes">Bytes.</param>
        public static string Utf8ToStr(this byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}
        /// <summary>
        /// 字节数组转换Utf8字符串
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="index">起始位置.</param>
        /// <param name="count">长度.</param>
        public static string Utf8ToStr(this byte[] bytes, int index, int count)
		{
			return Encoding.UTF8.GetString(bytes, index, count);
		}

        /// <summary>
		/// Uint转换为字节数组
		/// </summary>
		/// <param name="bytes">保存地址.</param>
		/// <param name="offset">字节数组的起始位置.</param>
		/// <param name="num">需转换的UINT值.</param>
		public static void WriteTo(this byte[] bytes, int offset, uint num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
			bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
			bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
		}

        /// <summary>
        /// int转换为字节数组
        /// </summary>
        /// <param name="bytes">保存地址.</param>
        /// <param name="offset">字节数组的起始位置.</param>
        /// <param name="num">需转换的UINT值.</param>
        public static void WriteTo(this byte[] bytes, int offset, int num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
			bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
			bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
		}

        /// <summary>
        /// 字节转换为字节数组
        /// </summary>
        /// <param name="bytes">保存地址.</param>
        /// <param name="offset">字节数组的起始位置.</param>
        /// <param name="num">需转换的UINT值.</param>
        public static void WriteTo(this byte[] bytes, int offset, byte num)
		{
			bytes[offset] = num;
		}

        /// <summary>
        /// short int转换为字节数组
        /// </summary>
        /// <param name="bytes">保存地址.</param>
        /// <param name="offset">字节数组的起始位置.</param>
        /// <param name="num">需转换的UINT值.</param>
        public static void WriteTo(this byte[] bytes, int offset, short num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
		}

        /// <summary>
        /// ushort int转换为字节数组
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="num"></param>
        public static void WriteTo(this byte[] bytes, int offset, ushort num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
		}
	}
}