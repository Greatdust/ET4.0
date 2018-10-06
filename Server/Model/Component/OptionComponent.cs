using System;
using CommandLine;

namespace ETModel
{
	[ObjectSystem]
	public class OptionComponentSystem : AwakeSystem<OptionComponent, string[]>
	{
		public override void Awake(OptionComponent self, string[] a)
		{
			self.Awake(a);
		}
	}
    /// <summary>
    /// APP启动配置组件
    /// </summary>
    public class OptionComponent : Component
	{
        /// <summary>
        /// APP启动配置参数
        /// </summary>
        public Options Options { get; set; }

		public void Awake(string[] args)
		{
            //取出应用程序参数args 填充 Options 类
            Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误!"))
				.WithParsed(options => { Options = options; });
		}
	}
}
