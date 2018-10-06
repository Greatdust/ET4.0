using System;

namespace ETModel
{
    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEvent
	{
        /// <summary>
        /// 处理
        /// </summary>
        void Handle();
		void Handle(object a);
		void Handle(object a, object b);
		void Handle(object a, object b, object c);
	}

    /// <summary>
    /// 无参事件 调用Handle方法会调用Run方法
    /// </summary>
    public abstract class AEvent : IEvent
	{
		public void Handle()
		{
			this.Run();
		}

        // 这里是无参的 所以 带参的几个接口 是不用管理的 使用会抛出异常
        public void Handle(object a)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b, object c)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// Run this instance.（运行）
        /// </summary>
        public abstract void Run();
	}

    /// <summary>
    /// 带一个参的事件
    /// </summary>
    public abstract class AEvent<A>: IEvent
	{
		public void Handle()
		{
			throw new NotImplementedException();
		}

        // 这里是一个参的 所以 其它的几个接口  使用会抛出异常
        public void Handle(object a)
		{
			this.Run((A)a);
		}

		public void Handle(object a, object b)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b, object c)
		{
			throw new NotImplementedException();
		}

		public abstract void Run(A a);
	}

    /// <summary>
    /// 带两个参的事件
    /// </summary>
    public abstract class AEvent<A, B>: IEvent
	{
		public void Handle()
		{
			throw new NotImplementedException();
		}

		public void Handle(object a)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b)
		{
			this.Run((A)a, (B)b);
		}

		public void Handle(object a, object b, object c)
		{
			throw new NotImplementedException();
		}

		public abstract void Run(A a, B b);
	}


    /// <summary>
    /// 带三个参的事件
    /// </summary>
    public abstract class AEvent<A, B, C>: IEvent
	{
		public void Handle()
		{
			throw new NotImplementedException();
		}

		public void Handle(object a)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b, object c)
		{
			this.Run((A)a, (B)b, (C)c);
		}

		public abstract void Run(A a, B b, C c);
	}
}