using System;

namespace ETModel
{
    /// <summary>
    /// 事件特性标记
    /// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EntityEventAttribute: Attribute
	{
		public int ClassType;

		public EntityEventAttribute(int classType)
		{
			this.ClassType = classType;
		}
	}
}