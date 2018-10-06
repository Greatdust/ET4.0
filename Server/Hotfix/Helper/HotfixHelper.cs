using System;
using System.Reflection;

namespace ETHotfix
{
    /// <summary>
    /// 热更新操作
    /// </summary>
    public static class HotfixHelper
	{
        /// <summary>
        /// 输入旧的类，获取新的类
        /// </summary>
		public static object Create(object old)
		{
			Assembly assembly = typeof(HotfixHelper).Assembly; //加载热更新程序集
            string objectName = old.GetType().FullName;   //拿到旧的类的名字
            return Activator.CreateInstance(assembly.GetType(objectName)); //返回新的类
        }
	}
}
