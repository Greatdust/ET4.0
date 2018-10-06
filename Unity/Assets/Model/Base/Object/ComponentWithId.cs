using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 组件（带一个ID）
    /// </summary>
	[BsonIgnoreExtraElements]
	public abstract class ComponentWithId : Component
	{
		[BsonIgnoreIfDefault]
		[BsonDefaultValue(0L)]
		[BsonElement]
		[BsonId]
		public long Id { get; set; }

		protected ComponentWithId()
		{
		}

        /// <summary>
        /// 初始化组建 
        /// </summary>
        /// <param name="id">制定一个ID</param>
		protected ComponentWithId(long id)
		{
			this.Id = id;
		}

        /// <summary>
        /// 回收
        /// </summary>
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}