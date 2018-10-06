namespace ETModel
{
    /// <summary>
    /// AB包地址 和 服务器地址
    /// </summary>
	public class GlobalProto
	{
		public string AssetBundleServerUrl;
		public string Address;

        /// <summary>
        /// 获得AB包地址
        /// </summary>
        /// <returns></returns>
		public string GetUrl()
		{
			string url = this.AssetBundleServerUrl;
#if UNITY_ANDROID
			url += "Android/";
#elif UNITY_IOS
			url += "IOS/";
#elif UNITY_WEBGL
			url += "WebGL/";
#elif UNITY_STANDALONE_OSX
			url += "MacOS/";
#else
			url += "PC/";
#endif
			Log.Debug(url);
			return url;
		}
	}
}
