namespace ETModel
{
	public static class ObjectHelper
	{
        /// <summary>
		/// 交换
		/// </summary>
		/// <param name="t1">T1.</param>
		/// <param name="t2">T2.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void Swap<T>(ref T t1, ref T t2)
		{
			T t3 = t1;
			t1 = t2;
			t2 = t3;
		}
	}
}