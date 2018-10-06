using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class UiComponentAwakeSystem : AwakeSystem<UIComponent>
	{
		public override void Awake(UIComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class UiComponentLoadSystem : LoadSystem<UIComponent>
	{
		public override void Load(UIComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// 管理所有UI
	/// </summary>
	public class UIComponent: Component
	{
        /// <summary>
        /// UI根节点
        /// </summary>
		private GameObject Root;
        /// <summary>
        /// UI的类型
        /// </summary>
		private readonly Dictionary<string, IUIFactory> UiTypes = new Dictionary<string, IUIFactory>();
        /// <summary>
        /// UI实体的字典
        /// </summary>
		private readonly Dictionary<string, UI> uis = new Dictionary<string, UI>();
        

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

			foreach (string type in uis.Keys.ToArray())
			{
				UI ui;
				if (!uis.TryGetValue(type, out ui))
				{
					continue;
				}
				uis.Remove(type);
				ui.Dispose();
			}

			this.uis.Clear();
			this.UiTypes.Clear();
		}

        /// <summary>
        /// UI根节点
        /// </summary>
		public void Awake()
		{
			this.Root = GameObject.Find("Global/UI/");
			this.Load();
		}
        /// <summary>
        /// Load初始化 得到所有UI类型
        /// </summary>
		public void Load()
		{
			this.UiTypes.Clear();
            //找到所有有UIFactory特性的类
            List<Type> types = Game.EventSystem.GetTypes(typeof(UIFactoryAttribute));
            //遍历
			foreach (Type type in types)
			{
                //反射获得用户自定义属性
                object[] attrs = type.GetCustomAttributes(typeof (UIFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UIFactoryAttribute attribute = attrs[0] as UIFactoryAttribute; //进行格式转换

                //attribute.Type是自定义的UI类型  用来区分UI

                if (UiTypes.ContainsKey(attribute.Type)) //判断字典中是否已经拥有此UI类型
				{
                    Log.Debug($"已经存在同类UI Factory: {attribute.Type}");
					throw new Exception($"已经存在同类UI Factory: {attribute.Type}");
				}
				object o = Activator.CreateInstance(type); //实例化
				IUIFactory factory = o as IUIFactory;  //所有UI类型  都需继承IUIFactory 接口
                if (factory == null)
				{
					Log.Error($"{o.GetType().FullName} 没有继承 IUIFactory");
					continue;
				}
				this.UiTypes.Add(attribute.Type, factory);  //添加到字典中
			}
		}

        /// <summary>
        /// 生成UI实体
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public UI Create(string type)
		{
			try
			{
				UI ui = UiTypes[type].Create(this.GetParent<Scene>(), type, Root); //根据UI类型 生成UI实体
				uis.Add(type, ui);  //添加到实体字典中

				// 设置canvas 画布
				string cavasName = ui.GameObject.GetComponent<CanvasConfig>().CanvasName;  //画布的名字
				ui.GameObject.transform.SetParent(this.Root.Get<GameObject>(cavasName).transform, false); //设置UI的父对象
				return ui;
			}
			catch (Exception e)
			{
				throw new Exception($"{type} UI 错误: {e}");
			}
		}

        /// <summary>
        /// 添加UI实体到字典中
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ui"></param>
		public void Add(string type, UI ui)
		{
			this.uis.Add(type, ui);
		}

        /// <summary>
        /// 移除UI实体
        /// </summary>
        /// <param name="type"></param>
		public void Remove(string type)
		{
			UI ui;
			if (!uis.TryGetValue(type, out ui))
			{
				return;
			}
            uis.Remove(type);
			ui.Dispose();
		}

        /// <summary>
        /// 移除所有UI实体
        /// </summary>
		public void RemoveAll()
		{
			foreach (string type in this.uis.Keys.ToArray())
			{
				UI ui;
				if (!this.uis.TryGetValue(type, out ui))
				{
					continue;
                }
                this.uis.Remove(type);
				ui.Dispose();
			}
		}

        /// <summary>
        /// 从字典中取得UI实体
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public UI Get(string type)
		{
			UI ui;
			this.uis.TryGetValue(type, out ui);
			return ui;
		}

        /// <summary>
        /// 返回所有UI实体的名字
        /// </summary>
        /// <returns></returns>
		public List<string> GetUITypeList()
		{
			return new List<string>(this.uis.Keys);
		}
	}
}