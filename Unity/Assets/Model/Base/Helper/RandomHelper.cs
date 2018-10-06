using System;

namespace ETModel
{
    /// <summary>
    /// 随机数
    /// </summary>
	public static class RandomHelper
	{
		private static readonly Random random = new Random();
        /// <summary>
		/// 获得一个随机64为Uint	
		/// </summary>
		/// <returns>The U int64.</returns>
		public static UInt64 RandUInt64()
		{
			var bytes = new byte[8];
			random.NextBytes(bytes);

            return BitConverter.ToUInt64(bytes, 0);
		}
        /// <summary>
        ///  获得一个随机64为int	
        /// </summary>
        /// <returns></returns>
		public static Int64 RandInt64()
		{
			var bytes = new byte[8];
			random.NextBytes(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}

		/// <summary>
		/// 获取lower与Upper之间的随机数
		/// </summary>
		/// <param name="lower"></param>
		/// <param name="upper"></param>
		/// <returns></returns>
		public static int RandomNumber(int lower, int upper)
		{
			int value = random.Next(lower, upper);
			return value;
		}
	}
}