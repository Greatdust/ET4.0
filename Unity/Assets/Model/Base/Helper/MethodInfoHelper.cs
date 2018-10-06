using System.Reflection;

namespace ETModel
{
	public static class MethodInfoHelper
	{
		public static void Run(this MethodInfo methodInfo, object obj, params object[] param)
		{
            //如果是静态方法 把obj插入到param参数中一起传递
            if (methodInfo.IsStatic) //这个方法是静态方法
            {
				object[] p = new object[param.Length + 1];
				p[0] = obj;
				for (int i = 0; i < param.Length; ++i)
				{
					p[i + 1] = param[i];
				}
				methodInfo.Invoke(null, p); //反射执行
			}
            else//不是静态方法
            {
				methodInfo.Invoke(obj, param); //反射执行
            }
		}
	}
}