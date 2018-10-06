using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class UiAwakeSystem : AwakeSystem<UI, GameObject>
	{
		public override void Awake(UI self, GameObject gameObject)
		{
			self.Awake(gameObject);
		}
	}
    /// <summary>
    /// UI
    /// </summary>
	public sealed class UI: Entity
	{
        /// <summary>
        /// UI的名字
        /// </summary>
		public string Name
		{
			get
			{
				return this.GameObject.name;
			}
		}
        /// <summary>
        /// UI对象
        /// </summary>
		public GameObject GameObject { get; private set; }
        /// <summary>
        /// UI字典  保持子UI
        /// </summary>
		public Dictionary<string, UI> children = new Dictionary<string, UI>();
		
        /// <summary>
        /// 初始化 
        /// </summary>
        /// <param name="gameObject">UI对象</param>
		public void Awake(GameObject gameObject)
		{
			this.children.Clear();
			this.GameObject = gameObject;
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

			foreach (UI ui in this.children.Values)
			{
				ui.Dispose();
			}
			
			UnityEngine.Object.Destroy(GameObject); //删除
			children.Clear();
			this.Parent = null;
		}

        /// <summary>
        /// 将该对象移到同级对象的首位（最上面 ）
        /// </summary>
		public void SetAsFirstSibling()
		{
			this.GameObject.transform.SetAsFirstSibling();
		}

        /// <summary>
        /// 添加子UI
        /// </summary>
        /// <param name="ui"></param>
		public void Add(UI ui)
		{
			this.children.Add(ui.Name, ui);
			ui.Parent = this;
		}

        /// <summary>
        /// 移除子UI
        /// </summary>
        /// <param name="name"></param>
		public void Remove(string name)
		{
			UI ui;
			if (!this.children.TryGetValue(name, out ui))
			{
				return;
			}
			this.children.Remove(name);
			ui.Dispose();
		}

        /// <summary>
        /// 得到子UI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public UI Get(string name)
		{
			UI child;
			if (this.children.TryGetValue(name, out child)) //自己下面有 直接返回
			{
				return child;
			}
			GameObject childGameObject = this.GameObject.transform.Find(name)?.gameObject;  //在视图中查找
			if (childGameObject == null)
			{
				return null;
			}
			child = ComponentFactory.Create<UI, GameObject>(childGameObject);  //最后用组件工厂生成
			this.Add(child);
			return child;
		}
	}
}