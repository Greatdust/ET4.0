using System.ComponentModel;

namespace ETModel
{
    /// <summary>
    /// Object.基类 继承接口ISupportInitialize 提供BeginInit EndInit接口
    /// </summary>
    public abstract class Object: ISupportInitialize
	{
		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}

		public override string ToString()
		{
			return JsonHelper.ToJson(this);
		}
	}
}