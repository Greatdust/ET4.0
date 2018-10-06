#if SERVER
using CommandLine;
#endif

namespace ETModel
{
    /// <summary>
    /// APP启动配置参数
    /// </summary>
    public class Options
	{
        //   // 短参数名称，是否是可选参数，默认值，帮助文本等
        [Option("appId", Required = false, Default = 1)]
		public int AppId { get; set; }

        /// <summary>
        /// 服务器类型
        /// </summary>
        // 没啥用，主要是在查看进程信息能区分每个app.exe的类型
        [Option("appType", Required = false, Default = AppType.Manager)]
		public AppType AppType { get; set; }
        /// <summary>
        /// 配置文件
        /// </summary>
		[Option("config", Required = false, Default = "../Config/StartConfig/LocalAllServer.txt")]
		public string Config { get; set; }
	}
}