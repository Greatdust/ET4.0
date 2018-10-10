using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	public static class BundleHelper
	{
        /// <summary>
        /// 下载AB包
        /// </summary>
        /// <returns></returns>
		public static async Task DownloadBundle()
		{
			if (Define.IsAsync) //如果不是在编辑器模式下
			{
				try
				{
					using (BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.AddComponent<BundleDownloaderComponent>())
					{
						await bundleDownloaderComponent.StartAsync(); //确认需要更新的AB包
						
						Game.EventSystem.Run(EventIdType.LoadingBegin); //发出开始加载事件消息

                        await bundleDownloaderComponent.DownloadAsync(); //开始下载AB包
					}
					
					Game.EventSystem.Run(EventIdType.LoadingFinish); //发出加载完成事件消息
                    //同步加载StreamingAssets ab包
                    Game.Scene.GetComponent<ResourcesComponent>().LoadOneBundle("StreamingAssets");
                    //拿到StreamingAssets ab包的依赖文件AssetBundleManifest
                    ResourcesComponent.AssetBundleManifestObject = (AssetBundleManifest)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("StreamingAssets", "AssetBundleManifest");
				}
				catch (Exception e)
				{
					Log.Error(e);
				}

			}
		}
        /// <summary>
        /// 获取本地AB包MD5值
        /// </summary>
        /// <param name="streamingVersionConfig"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
		public static string GetBundleMD5(VersionConfig streamingVersionConfig, string bundleName)
		{
            //如果本地有这个AB包 就生成MD5值返回
            string path = Path.Combine(PathHelper.AppHotfixResPath, bundleName);
			if (File.Exists(path))
			{
				return MD5Helper.FileMD5(path);
			}
            //如果没有，就看看本地Version中是否有这个文件的的MD5值
            if (streamingVersionConfig.FileInfoDict.ContainsKey(bundleName))
			{
				return streamingVersionConfig.FileInfoDict[bundleName].MD5;	
			}
            //否则就返回空
			return "";
		}
	}
}
