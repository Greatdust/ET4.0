using System;

namespace ETModel
{
    /// <summary>
    /// 组件工厂
    /// </summary>
	public static class ComponentFactory
	{
        /// <summary>
        /// 创建组件并指定其父组件（实体）
        /// </summary>
        /// <param name="type">需创建的组件类型</param>
        /// <param name="parent">父组件（实体）</param>
        /// <returns>返回的是Component类型</returns>
		public static Component CreateWithParent(Type type, Component parent)
		{
			Component component = Game.ObjectPool.Fetch(type); //从池中生成
			component.Parent = parent;  //制定父组件（实体）
            ComponentWithId componentWithId = component as ComponentWithId;  //如果他是一个带ID的组件
			if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId;   //将他的ID = 唯一的InstanceId
            }
			Game.EventSystem.Awake(component);  //调用初始化Awake方法
            return component;
		}

        /// <summary>
        ///  泛型 创建组件并指定其父组件（实体） 
        /// 和上面的区别是  上面返回的是Component类型  这个可以返回是指定的泛型T类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="parent">父组件（实体） </param>
        /// <returns>返回的是指定的T类型T类型</returns>
		public static T CreateWithParent<T>(Component parent) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			ComponentWithId componentWithId = component as ComponentWithId; //如果他是一个带ID的组件
            if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId;
			}
			Game.EventSystem.Awake(component);
			return component;
		}

        /// <summary>
        /// 泛型 创建组件并指定其父组件（实体） 可带一个参数 用于Awake初始化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="A">参数类型</typeparam>
        /// <param name="parent">父组件（实体） </param>
        /// <param name="a">传递参数</param>
        /// <returns>返回的是指定的T类型</returns>
		public static T CreateWithParent<T, A>(Component parent, A a) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			ComponentWithId componentWithId = component as ComponentWithId; //如果他是一个带ID的组件
            if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId; //将他的ID = 唯一的InstanceId
            }
			Game.EventSystem.Awake(component, a); //带一个参数的初始化
			return component;
		}

        /// <summary>
        /// 泛型 创建组件并指定其父组件（实体） 可带两个参数 用于Awake初始化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="A">参数类型1</typeparam>
        /// <typeparam name="B">参数类型2</typeparam>
        /// <param name="parent">父组件（实体）</param>
        /// <param name="a">传递参数a</param>
        /// <param name="b">传递参数b</param>
        /// <returns>返回的是指定的T类型</returns>
		public static T CreateWithParent<T, A, B>(Component parent, A a, B b) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			ComponentWithId componentWithId = component as ComponentWithId; //如果他是一个带ID的组件
            if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId; //将他的ID = 唯一的InstanceId
            }
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

        /// <summary>
        /// 泛型 创建组件并指定其父组件（实体） 可带三个个参数 用于Awake初始化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="parent"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
		public static T CreateWithParent<T, A, B, C>(Component parent, A a, B b, C c) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			ComponentWithId componentWithId = component as ComponentWithId; //如果他是一个带ID的组件
            if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId; //将他的ID = 唯一的InstanceId
            }
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}


        /// <summary>
        /// 泛型 创建组件
        /// </summary>
        /// <param name="type">需创建的组件类型</param>
        /// <returns>返回的是指定的T类型</returns>
        public static T Create<T>() where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			ComponentWithId componentWithId = component as ComponentWithId;   //如果他是一个带ID的组件
            if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId;  //将他的ID = 唯一的InstanceId
            } 
			Game.EventSystem.Awake(component);
			return component;
		}


        /// <summary>
        /// 泛型 创建组件 带一个参数用于 Awake初始化
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <typeparam name="A">参数类型</typeparam>
        /// <param name="a">传参</param>
        /// <returns>返回的是指定的T类型</returns>
		public static T Create<T, A>(A a) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			ComponentWithId componentWithId = component as ComponentWithId;
			if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId;
			}
			Game.EventSystem.Awake(component, a);
			return component;
		}

        /// <summary>
        /// 泛型 创建组件 带两个参数用于 Awake初始化
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <typeparam name="A">参数类型a</typeparam>
        /// <typeparam name="B">参数类型b</typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
		public static T Create<T, A, B>(A a, B b) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			ComponentWithId componentWithId = component as ComponentWithId;
			if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId;
			}
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

        /// <summary>
        /// 泛型 创建组件 带三个个参数用于 Awake初始化
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <typeparam name="A">参数类型a</typeparam>
        /// <typeparam name="B">参数类型b</typeparam>
        /// <typeparam name="C">参数类型c</typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
		public static T Create<T, A, B, C>(A a, B b, C c) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			ComponentWithId componentWithId = component as ComponentWithId;
			if (componentWithId != null)
			{
				componentWithId.Id = component.InstanceId;
			}
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}

        /// <summary>
        ///  泛型 创建组件并赋值其ID 
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="id">ID值</param>
        /// <returns></returns>
		public static T CreateWithId<T>(long id) where T : ComponentWithId
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Id = id;   //这里的组件 id 和 InstanceId便是不同了
            Game.EventSystem.Awake(component);
			return component;
		}

        /// <summary>
        ///  泛型 创建组件并赋值其ID  带一个参数用于 Awake初始化
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <typeparam name="A">参数类型</typeparam>
        /// <param name="id">ID值</param>
        /// <param name="a">参数</param>
        /// <returns></returns>
		public static T CreateWithId<T, A>(long id, A a) where T : ComponentWithId
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Id = id;
			Game.EventSystem.Awake(component, a);
			return component;
		}

        /// <summary>
        ///  泛型 创建组件并赋值其ID  带两个参数用于 Awake初始化
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <typeparam name="A">参数类型1</typeparam>
        /// <typeparam name="B">参数类型2</typeparam>
        /// <param name="id">ID值</param>
        /// <param name="a">参数</param>
        /// <param name="b">参数</param>
        /// <returns></returns>
		public static T CreateWithId<T, A, B>(long id, A a, B b) where T : ComponentWithId
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Id = id;
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

        /// <summary>
        ///  泛型 创建组件并赋值其ID  带三个参数用于 Awake初始化
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <typeparam name="A">参数类型1</typeparam>
        /// <typeparam name="B">参数类型2</typeparam>
        /// <typeparam name="C">参数类型3</typeparam>
        /// <param name="id">ID值</param>
        /// <param name="a">参数</param>
        /// <param name="b">参数</param>
        /// <param name="c">参数</param>
        /// <returns></returns>
		public static T CreateWithId<T, A, B, C>(long id, A a, B b, C c) where T : ComponentWithId
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Id = id;
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}
	}
}
