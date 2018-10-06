using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ETModel
{
	public enum DLLType
	{
		Model,
		Hotfix,
		Editor,
	}

	public sealed class EventSystem
	{
        /// <summary>
        /// 所有的组件字典  ID ->组件
        /// </summary>
        private readonly Dictionary<long, Component> allComponents = new Dictionary<long, Component>();

        /// <summary>
        /// 保存了程序集的名称 这里其实就只有Assembly-CSharp.dll
        /// </summary>
        private readonly Dictionary<DLLType, Assembly> assemblies = new Dictionary<DLLType, Assembly>();

        /// <summary>
        /// 保存了所有 特性 与 类 的关联的字典
        /// </summary>
		private readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();

        /// <summary>
        /// 所有的事件字典
        /// </summary>
        private readonly Dictionary<string, List<IEvent>> allEvents = new Dictionary<string, List<IEvent>>();

        /// <summary>
        /// 类的类型 -> 类
        /// </summary>
		private readonly UnOrderMultiMap<Type, IAwakeSystem> awakeSystems = new UnOrderMultiMap<Type, IAwakeSystem>();

		private readonly UnOrderMultiMap<Type, IStartSystem> startSystems = new UnOrderMultiMap<Type, IStartSystem>();

		private readonly UnOrderMultiMap<Type, IDestroySystem> destroySystems = new UnOrderMultiMap<Type, IDestroySystem>();

		private readonly UnOrderMultiMap<Type, ILoadSystem> loadSystems = new UnOrderMultiMap<Type, ILoadSystem>();

		private readonly UnOrderMultiMap<Type, IUpdateSystem> updateSystems = new UnOrderMultiMap<Type, IUpdateSystem>();

		private readonly UnOrderMultiMap<Type, ILateUpdateSystem> lateUpdateSystems = new UnOrderMultiMap<Type, ILateUpdateSystem>();

		private readonly UnOrderMultiMap<Type, IChangeSystem> changeSystems = new UnOrderMultiMap<Type, IChangeSystem>();


		private Queue<long> updates = new Queue<long>(); //存放了需要执行的updates的ID 
        private Queue<long> updates2 = new Queue<long>();  //使用的时候 先从updates中取 用后放入updates2中  updates中取完了，这一次update结束 并把updates2中放入

        private Queue<long> loaders = new Queue<long>();   //同上
        private Queue<long> loaders2 = new Queue<long>();

		private Queue<long> lateUpdates = new Queue<long>(); //同上
        private Queue<long> lateUpdates2 = new Queue<long>();

        private readonly Queue<long> starts = new Queue<long>(); //这个只需要一个  因为starts只会执行一次  

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="dllType"></param>
        /// <param name="assembly"></param>
        public void Add(DLLType dllType, Assembly assembly)
		{
			this.assemblies[dllType] = assembly;

			this.types.Clear();  //清空

            foreach (Assembly value in this.assemblies.Values)
			{
				foreach (Type type in value.GetTypes())
				{
					object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), false);  //有BaseAttribute特性的类
                    if (objects.Length == 0)
					{
						continue;
					}

					BaseAttribute baseAttribute = (BaseAttribute) objects[0];   
					this.types.Add(baseAttribute.AttributeType, type);    //把类根据其特性的类型  加入到不同的字典中
				}
			}

			this.awakeSystems.Clear();
			this.lateUpdateSystems.Clear();
			this.updateSystems.Clear();
			this.startSystems.Clear();
			this.loadSystems.Clear();
			this.changeSystems.Clear();

			foreach (Type type in types[typeof(ObjectSystemAttribute)])  //遍历有ObjectSystem特性的类 
            {
				object[] attrs = type.GetCustomAttributes(typeof(ObjectSystemAttribute), false);  //再次检查

				if (attrs.Length == 0)
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);      //实例化

                //比如 public class TimerComponentUpdateSystem : UpdateSystem<TimerComponent>
                //     我们写的功能都在  TimerComponent 中  它里面有  start update 等方法 但是不用我们来调用
                //     TimerComponentUpdateSystem 类会自动帮我们调用 其中的 update 方法 
                //     updateSystems 字典中会保存  TimerComponent类名 和 TimerComponentUpdateSystem 这个类 
                //     当 TimerComponent 这个类生成的时候   updateSystems 字典中检索到了  有这个类名  就会调用TimerComponentUpdateSystem 
                //     而 TimerComponentUpdateSystem  的传参是 TimerComponent  他会自动调用 TimerComponent 中的方法

                IAwakeSystem objectSystem = obj as IAwakeSystem;
				if (objectSystem != null)
				{
					this.awakeSystems.Add(objectSystem.Type(), objectSystem);  //！！！这里的objectSystem.Type()其实就是需要执行的类的类型 所以他能收集对应的类的方法 
                }

				IUpdateSystem updateSystem = obj as IUpdateSystem;
				if (updateSystem != null)
				{
					this.updateSystems.Add(updateSystem.Type(), updateSystem);
				}

				ILateUpdateSystem lateUpdateSystem = obj as ILateUpdateSystem;
				if (lateUpdateSystem != null)
				{
					this.lateUpdateSystems.Add(lateUpdateSystem.Type(), lateUpdateSystem);
				}

				IStartSystem startSystem = obj as IStartSystem;
				if (startSystem != null)
				{
					this.startSystems.Add(startSystem.Type(), startSystem);
				}

				IDestroySystem destroySystem = obj as IDestroySystem;
				if (destroySystem != null)
				{
					this.destroySystems.Add(destroySystem.Type(), destroySystem);
				}

				ILoadSystem loadSystem = obj as ILoadSystem;
				if (loadSystem != null)
				{
					this.loadSystems.Add(loadSystem.Type(), loadSystem);
				}

				IChangeSystem changeSystem = obj as IChangeSystem;
				if (changeSystem != null)
				{
					this.changeSystems.Add(changeSystem.Type(), changeSystem);
				}
			}
            /////////////////////////////////////////////////////////////////////////////////////////

            this.allEvents.Clear(); //清除事件
            //遍历所有的有事件特性的类
			foreach (Type type in types[typeof(EventAttribute)])
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);  //生成特性

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;   
					object obj = Activator.CreateInstance(type);  //反射出类
					IEvent iEvent = obj as IEvent;
					if (iEvent == null)                          //判断类是否继承IEvent接口
                    {
						Log.Error($"{obj.GetType().Name} 没有继承IEvent");
					}
					this.RegisterEvent(aEventAttribute.Type, iEvent);   //加入字典中
				}
			}

			this.Load();
		}
        
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="e"></param>
		public void RegisterEvent(string eventId, IEvent e)
		{
			if (!this.allEvents.ContainsKey(eventId))
			{
				this.allEvents.Add(eventId, new List<IEvent>());
			}
			this.allEvents[eventId].Add(e);
		}

        /// <summary>
        /// 返回程序集
        /// </summary>
        /// <param name="dllType"></param>
        /// <returns></returns>
		public Assembly Get(DLLType dllType)
		{
			return this.assemblies[dllType];
		}
		
        /// <summary>
        /// 返回拥有此特性标记的所有类的链表
        /// </summary>
        /// <param name="systemAttributeType">特性名字</param>
        /// <returns></returns>
		public List<Type> GetTypes(Type systemAttributeType)
		{
			if (!this.types.ContainsKey(systemAttributeType))
			{
				return new List<Type>();
			}
			return this.types[systemAttributeType];
		}

        /// <summary>
        /// 添加组件  所有的组件在创建的时候就会自动添加进来
        /// </summary>
        /// <param name="component"></param>
		public void Add(Component component)
		{
            //加入到总组件字典中
			this.allComponents.Add(component.InstanceId, component);
            //获取类型
			Type type = component.GetType();

            //根据各自的类型 添加到long 栈中 
            //如果LOAD中有这个类  把这个类的ID加入到字典中 以便调用  这里其实就是用来判断这个类改在什么时候执行
            if (this.loadSystems.ContainsKey(type))
			{
				this.loaders.Enqueue(component.InstanceId);
			}
            //如果updateSystems字典中有这个类型  把这个类的ID加入到字典中 以便调用
            if (this.updateSystems.ContainsKey(type))
			{
				this.updates.Enqueue(component.InstanceId);
			}

			if (this.startSystems.ContainsKey(type))
			{
				this.starts.Enqueue(component.InstanceId);
			}

			if (this.lateUpdateSystems.ContainsKey(type))
			{
				this.lateUpdates.Enqueue(component.InstanceId);
			}
		}

        /// <summary>
        /// 移除组件  从总字典中
        /// </summary>
        /// <param name="instanceId"></param>
		public void Remove(long instanceId)
		{
			this.allComponents.Remove(instanceId);
		}

        /// <summary>
        /// 获得组件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public Component Get(long id)
		{
			Component component = null;
			this.allComponents.TryGetValue(id, out component);
			return component;
		}

        /// <summary>
        /// 执行组件里面的Awake方法 无参  在组件被工厂创建的时候 会自动执行这个方法
        /// </summary>
        /// <param name="component"></param>
		public void Awake(Component component)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake iAwake = aAwakeSystem as IAwake;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

        /// <summary>
        /// 执行组件里面的Awake方法 1参  在组件被工厂创建的时候 会自动执行这个方法
        /// </summary>
        /// <typeparam name="P1"></typeparam>
        /// <param name="component"></param>
        /// <param name="p1"></param>
		public void Awake<P1>(Component component, P1 p1)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake<P1> iAwake = aAwakeSystem as IAwake<P1>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component, p1);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

        /// <summary>
        /// 执行组件里面的Awake方法 2参  在组件被工厂创建的时候 会自动执行这个方法
        /// </summary>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <param name="component"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
		public void Awake<P1, P2>(Component component, P1 p1, P2 p2)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake<P1, P2> iAwake = aAwakeSystem as IAwake<P1, P2>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component, p1, p2);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

        /// <summary>
        /// 执行组件里面的Awake方法 3参  在组件被工厂创建的时候 会自动执行这个方法
        /// </summary>
        /// <typeparam name="P1"></typeparam>
        /// <typeparam name="P2"></typeparam>
        /// <typeparam name="P3"></typeparam>
        /// <param name="component"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
		public void Awake<P1, P2, P3>(Component component, P1 p1, P2 p2, P3 p3)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeSystems[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}

				IAwake<P1, P2, P3> iAwake = aAwakeSystem as IAwake<P1, P2, P3>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component, p1, p2, p3);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

        /// <summary>
        /// 变化  没用到
        /// </summary>
        /// <param name="component"></param>
		public void Change(Component component)
		{
			List<IChangeSystem> iChangeSystems = this.changeSystems[component.GetType()];
			if (iChangeSystems == null)
			{
				return;
			}

			foreach (IChangeSystem iChangeSystem in iChangeSystems)
			{
				if (iChangeSystem == null)
				{
					continue;
				}

				try
				{
					iChangeSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
        

        /// <summary>
        /// LOAD 方法加载
        /// </summary>
		public void Load()
		{
			while (this.loaders.Count > 0)
			{
				long instanceId = this.loaders.Dequeue(); //从字典中取出一个
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))  //总字典中取得类
				{
					continue;
				}
				if (component.IsDisposed)
				{
					continue;
				}
				
				List<ILoadSystem> iLoadSystems = this.loadSystems[component.GetType()];
				if (iLoadSystems == null)
				{
					continue;
				}

				this.loaders2.Enqueue(instanceId);

				foreach (ILoadSystem iLoadSystem in iLoadSystems)
				{
					try
					{
						iLoadSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
            //更换
			ObjectHelper.Swap(ref this.loaders, ref this.loaders2);
		}

        /// <summary>
        /// Start
        /// </summary>
		private void Start()
		{
			while (this.starts.Count > 0)
			{
				long instanceId = this.starts.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}


                List<IStartSystem> iStartSystems = this.startSystems[component.GetType()];
				if (iStartSystems == null)
				{
					continue;
				}
				
				foreach (IStartSystem iStartSystem in iStartSystems)
				{
					try
					{
						iStartSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}


		public void Destroy(Component component)
		{
			List<IDestroySystem> iDestroySystems = this.destroySystems[component.GetType()];
			if (iDestroySystems == null)
			{
				return;
			}

			foreach (IDestroySystem iDestroySystem in iDestroySystems)
			{
				if (iDestroySystem == null)
				{
					continue;
				}

				try
				{
					iDestroySystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Update()
		{
			this.Start();
			
			while (this.updates.Count > 0)
			{
				long instanceId = this.updates.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}
				if (component.IsDisposed)
				{
					continue;
				}
				
				List<IUpdateSystem> iUpdateSystems = this.updateSystems[component.GetType()];
				if (iUpdateSystems == null)
				{
					continue;
				}

				this.updates2.Enqueue(instanceId);

				foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
				{
					try
					{
						iUpdateSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.updates, ref this.updates2);
		}

		public void LateUpdate()
		{
			while (this.lateUpdates.Count > 0)
			{
				long instanceId = this.lateUpdates.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}
				if (component.IsDisposed)
				{
					continue;
				}

				List<ILateUpdateSystem> iLateUpdateSystems = this.lateUpdateSystems[component.GetType()];
				if (iLateUpdateSystems == null)
				{
					continue;
				}

				this.lateUpdates2.Enqueue(instanceId);

				foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
				{
					try
					{
						iLateUpdateSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
		}

        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="type"></param>
		public void Run(string type)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A>(string type, A a)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle(a);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A, B>(string type, A a, B b)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle(a, b);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A, B, C>(string type, A a, B b, C c)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle(a, b, c);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}