using System;

namespace ETModel
{
    /// <summary>
    /// LateUpdate接口
    /// </summary>
    public interface ILateUpdateSystem
	{
		Type Type();
		void Run(object o);
	}
    /// <summary>
    /// LateUpdate 调用Run 执行LateUpdate（）函数
    /// </summary>
    public abstract class LateUpdateSystem<T> : ILateUpdateSystem
	{
		public void Run(object o)
		{
			this.LateUpdate((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void LateUpdate(T self);
	}
}
