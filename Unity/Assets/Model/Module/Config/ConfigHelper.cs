using System;
using UnityEngine;

namespace ETModel
{
	public static class ConfigHelper
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public static string GetText(string key)
		{
			try
			{
				GameObject config = (GameObject)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("config.unity3d", "Config");
				string configStr = config.Get<TextAsset>(key).text;
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, key: {key}", e);
			}
		}
		
        /// <summary>
        /// 获得AB包地址和服务器地址
        /// </summary>
        /// <returns></returns>
		public static string GetGlobal()
		{
			try
			{
				GameObject config = (GameObject)ResourcesHelper.Load("KV");  //加载Resources文件夹下KV 预制体
                string configStr = config.Get<TextAsset>("GlobalProto").text; //得到上面的GlobalProto文本
                return configStr;  
			}
			catch (Exception e)
			{
				throw new Exception($"load global config file fail", e);
			}
		}

		public static T ToObject<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}
	}
}