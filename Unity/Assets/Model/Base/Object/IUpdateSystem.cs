using System;

namespace ETModel
{
    /// <summary>
    /// Update接口  里面有RUN（）
    /// </summary>
    public interface IUpdateSystem
	{
		Type Type();
		void Run(object o);
	}

    /// <summary>
    /// Update 调用Run 执行里面Update函数
    /// </summary>
    public abstract class UpdateSystem<T> : IUpdateSystem
	{
		public void Run(object o)
		{
			this.Update((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Update(T self);
	}
}
