using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ETModel
{
	public static class DllHelper
	{
        /// <summary>
        /// 加载热更新的dll
        /// </summary>
        /// <returns></returns>
		public static Assembly GetHotfixAssembly()
		{
			byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
			byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
			Assembly assembly = Assembly.Load(dllBytes, pdbBytes);
			return assembly;
		}
	}
}
