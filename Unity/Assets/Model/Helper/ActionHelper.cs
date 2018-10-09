using System;
using UnityEngine.UI;

namespace ETModel
{
    /// <summary>
    /// 对按钮点击事件的封装
    /// </summary>
	public static class ActionHelper
	{
		public static void Add(this Button.ButtonClickedEvent buttonClickedEvent, Action action)
		{
			buttonClickedEvent.AddListener(()=> { action(); });
		}
	}
}