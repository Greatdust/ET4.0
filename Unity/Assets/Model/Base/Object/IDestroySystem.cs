using System;

namespace ETModel
{
    /// <summary>
    /// destroy接口
    /// </summary>
    public interface IDestroySystem
	{
		Type Type();
		void Run(object o);
	}
    /// <summary>
	/// Destroy 调用Run 执行 Destroy函数
	/// </summary>
	public abstract class DestroySystem<T> : IDestroySystem
	{
		public void Run(object o)
		{
			this.Destroy((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Destroy(T self);
	}
}
