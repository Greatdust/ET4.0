using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 实体
    /// </summary>
	[BsonIgnoreExtraElements]
	public class Entity : ComponentWithId
	{
        /// <summary>
        /// 组件哈希表，一个实体上可以挂载多个组件
        /// </summary>
		[BsonElement("C")]
		[BsonIgnoreIfNull]
		private HashSet<Component> components;

        /// <summary>
        /// 组件字典
        /// </summary>
        [BsonIgnore]
		private Dictionary<Type, Component> componentDict;

        //初始化
		public Entity()
		{
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
		}

        //初始化 且赋值ID
		protected Entity(long id): base(id)
		{
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
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
			
			foreach (Component component in this.componentDict.Values)
			{
				try
				{
					component.Dispose(); //遍历字典 回收其下组件
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
			
			this.components.Clear();
			this.componentDict.Clear();
		}
		
        /// <summary>
        /// 挂载组件
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
		public virtual Component AddComponent(Component component)
		{
            //组件的类型
            Type type = component.GetType();

			if (this.componentDict.ContainsKey(type)) //已经有相同的就抛出异常
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {type.Name}");
			}
			
			component.Parent = this;  //将其父对象

            //如歌这个组件是序列化的实体  将其添加到哈希表中
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

        /// <summary>
        /// 挂载组件
        /// </summary>
        /// <param name="type">组件类型</param>
        /// <returns></returns>
		public virtual Component AddComponent(Type type)
		{
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {type.Name}");
			}
            //从工厂生成组件 并制定其父实体
			Component component = ComponentFactory.CreateWithParent(type, this);

            //如歌这个组件是序列化的实体  将其添加到哈希表中
            if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

        /// <summary>
        /// 泛型 挂载组件
        /// </summary>
        /// <typeparam name="K">组件类型</typeparam>
        /// <returns>返回挂载的组件</returns>
		public virtual K AddComponent<K>() where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K>(this);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

        /// <summary>
        /// 泛型 挂载组件 并传递一个参数用于这个组件Awake初始化
        /// </summary>
        /// <typeparam name="K">组件类型</typeparam>
        /// <typeparam name="P1">参数类型</typeparam>
        /// <param name="p1">参数</param>
        /// <returns></returns>
		public virtual K AddComponent<K, P1>(P1 p1) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1>(this, p1);
			
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

        /// <summary>
        /// 泛型 挂载组件 并传递两个参数用于这个组件Awake初始化
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
		public virtual K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1, P2>(this, p1, p2);
			
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

        /// <summary>
        /// 泛型 挂载组件 并传递三个参数用于这个组件Awake初始化
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <typeparam name="P3"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
		public virtual K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1, P2, P3>(this, p1, p2, p3);
			
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

        /// <summary>
        ///  泛型 移除一个组件
        /// </summary>
        /// <typeparam name="K"></typeparam>
		public virtual void RemoveComponent<K>() where K : Component
		{
			if (this.IsDisposed) //已经回收了
			{
				return;
			}
			Type type = typeof (K);
			Component component;
			if (!this.componentDict.TryGetValue(type, out component)) //获取不到
			{
				return;
			}

			this.components.Remove(component);  //移除
			this.componentDict.Remove(type); //移除

            component.Dispose();
		}

        /// <summary>
        /// 移除一个组件
        /// </summary>
        /// <param name="type"></param>
		public virtual void RemoveComponent(Type type)
		{
			if (this.IsDisposed) //已经回收了
            {
				return;
			}
			Component component;
			if (!this.componentDict.TryGetValue(type, out component)) //获取不到
            {
				return;
			}

			this.components?.Remove(component);
			this.componentDict.Remove(type);

			component.Dispose();
		}

        /// <summary>
        /// 泛型  获取组件
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <returns></returns>
		public K GetComponent<K>() where K : Component
		{
			Component component;
			if (!this.componentDict.TryGetValue(typeof(K), out component))
			{
				return default(K);
			}
			return (K)component;
		}
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type">组件类型</param>
        /// <returns></returns>
		public Component GetComponent(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return null;
			}
			return component;
		}

        /// <summary>
        /// 获取字典中所有的组件
        /// </summary>
        /// <returns></returns>
		public Component[] GetComponents()
		{
			return this.componentDict.Values.ToArray();
		}

        /// <summary>
		/// 初始化完成
		/// </summary>
		public override void EndInit()
		{
          
            try
			{
				base.EndInit();
				
				this.componentDict.Clear();

				if (this.components != null)
				{
					foreach (Component component in this.components)  //如果哈希表不为空 给字典赋值
                    {
						component.Parent = this;
						this.componentDict.Add(component.GetType(), component);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

        /// <summary>
        /// 开始序列化
        /// </summary>
		public override void BeginSerialize()
		{
           
            base.BeginSerialize();

			foreach (Component component in this.components)
			{
				component.BeginSerialize();
			}
		}

        /// <summary>
        /// 完成序列化
        /// </summary>
		public override void EndDeSerialize()
		{
         
            base.EndDeSerialize();
			
			foreach (Component component in this.components)
			{
				component.EndDeSerialize();
			}
		}
	}
}