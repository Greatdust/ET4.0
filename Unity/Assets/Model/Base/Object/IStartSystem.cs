using System;

namespace ETModel
{
    /// <summary>
    /// Start接口
    /// </summary>
    public interface IStartSystem
	{
		Type Type();
		void Run(object o);
	}


    /// <summary>
    /// Start  调用里面Run 执行Start函数
    /// </summary>
    public abstract class StartSystem<T> : IStartSystem
	{
		public void Run(object o)
		{
			this.Start((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Start(T self);
	}
}
