namespace ETModel
{
    /// <summary>
    /// 唯一ID生成器
    /// </summary>
	public static class IdGenerater
	{
        /// <summary>
        /// 生成ID的随机种子
        /// </summary>
		public static long AppId { private get; set; }
        /// <summary>
        /// 生成ID的随机种子2
        /// </summary>
        private static ushort value;

        /// <summary>
        /// 生成唯一ID 由时间截控制 
        /// </summary>
        /// <returns></returns>
		public static long GenerateId()
		{
			long time = TimeHelper.ClientNowSeconds();

			return (AppId << 48) + (time << 16) + ++value;
		}

        /// <summary>
        /// 也是生产一串ID  暂时没有发现那里有用到
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public static int GetAppIdFromId(long id)
		{
			return (int)(id >> 48);
		}
	}
}