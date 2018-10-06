using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 组件
    /// </summary>
    [BsonIgnoreExtraElements]
	public abstract class Component : Object, IDisposable, IComponentSerialize 
	{
        /// <summary>
        /// 生成的唯一ID  只有Game.EventSystem.Add方法中会设置该值，如果new出来的对象不想加入Game.EventSystem中，则需要自己在构造函数中设置
        /// </summary>
        [BsonIgnore]
		public long InstanceId { get; protected set; }

        /// <summary>
        /// 从组件PooL中生成
        /// </summary>
        [BsonIgnore]
		private bool isFromPool;

        /// <summary>
        /// 是从组件PooL中生成的  就会自动生成唯一的ID 并加入到组件字典中
        /// </summary>
		[BsonIgnore]
		public bool IsFromPool
		{
			get
			{
				return this.isFromPool;
			}
			set
			{
				this.isFromPool = value;

				if (!this.isFromPool)
				{
					return;
				}
                //生成一个唯一的ID
				this.InstanceId = IdGenerater.GenerateId();
				Game.EventSystem.Add(this); //！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
			}
		}

        /// <summary>
		/// 是否已经回收处理(判断组件ID==0)
		/// </summary>
		/// <value><c>true</c> if this instance is disposed; otherwise, <c>false</c>.</value>
		[BsonIgnore]
		public bool IsDisposed
		{
			get
			{
				return this.InstanceId == 0;
			}
		}
        /// <summary>
        /// 父组件
        /// </summary>
        /// <value>The parent.</value>
        [BsonIgnore]
		public Component Parent { get; set; }

        /// <summary>
		/// 获得父对象 泛型，可传入父组件类型
		/// </summary>
		/// <value>The entity.</value>
		public T GetParent<T>() where T : Component
		{
			return this.Parent as T;
		}

        /// <summary>
        /// 获得父对象（实体） 由于实体是继承组件的 他们的界限有点模糊 可以理解组件的父对象是实体（实体下会挂载多个组件）
        /// </summary>
		[BsonIgnore]
		public Entity Entity
		{
			get
			{
				return this.Parent as Entity;
			}
		}
		
		protected Component()
		{
		}
        
        /// <summary>
        /// 回收
        /// </summary>
		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

            // 触发Destroy事件 移除时会触发组件的Destroy方法
            Game.EventSystem.Destroy(this);

			Game.EventSystem.Remove(this.InstanceId); //从字典中一处这个组件
			
			this.InstanceId = 0; //唯一生成ID置零表示回收完成

            //如果是从池中生成的 回收时也会回收到组件Pool中
			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}
		}

		public virtual void BeginSerialize()
		{
		}

		public virtual void EndDeSerialize()
		{
		}
	}
}