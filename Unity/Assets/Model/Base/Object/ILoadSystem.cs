using System;

namespace ETModel
{
    /// <summary>
    /// Load 接口
    /// </summary>
    public interface ILoadSystem
	{
		Type Type();
		void Run(object o);
	}

    /// <summary>
    /// Load 调用Run 执行Load函数
    /// </summary>
    public abstract class LoadSystem<T> : ILoadSystem
	{
		public void Run(object o)
		{
			this.Load((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Load(T self);
	}
}
