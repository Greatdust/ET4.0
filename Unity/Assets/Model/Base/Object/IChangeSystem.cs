using System;

namespace ETModel
{
    /// <summary>
    /// Change接口
    /// </summary>
    public interface IChangeSystem
	{
		Type Type();
		void Run(object o);
	}

    /// <summary>
    /// Change system. 调用Run ->Change
    /// </summary>
    public abstract class ChangeSystem<T> : IChangeSystem
	{
		public void Run(object o)
		{
			this.Change((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Change(T self);
	}
}
