using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class UiBundleDownloaderComponentAwakeSystem : AwakeSystem<BundleDownloaderComponent>
	{
		public override void Awake(BundleDownloaderComponent self)
		{
			self.bundles = new Queue<string>();
			self.downloadedBundles = new HashSet<string>();
			self.downloadingBundle = "";
		}
	}

	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent : Component
	{
        /// <summary>
        /// 远程端版本配置文件  
        /// </summary>
		private VersionConfig remoteVersionConfig;
		/// <summary>
        /// 需要下载的AB包的名字
        /// </summary>
		public Queue<string> bundles;
        /// <summary>
        /// 总大小
        /// </summary>
		public long TotalSize;
        /// <summary>
        /// 已经下载的AB包哈希表
        /// </summary>
		public HashSet<string> downloadedBundles;
        /// <summary>
        /// 当前下载的AB包的名字
        /// </summary>
		public string downloadingBundle;

		public UnityWebRequestAsync webRequest;


        /// <summary>
        /// 开始下载远程Version文件进行本地对比 确认需要下载的AB包文件
        /// </summary>
        /// <returns></returns>
		public async Task StartAsync()
		{
			// 获取远程的Version.txt
			string versionUrl = "";
			try
			{
                
				using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
				{
                    //从Resources文件夹读取“VK”预制体上的TXT文档取得AB资源服务区地址 得到最新的Version.txt
                    versionUrl = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + "Version.txt";
					//Log.Debug(versionUrl);
					await webRequestAsync.DownloadAsync(versionUrl); //下载资源
					remoteVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequestAsync.Request.downloadHandler.text);
					//Log.Debug(JsonHelper.ToJson(this.VersionConfig));
				}

			}
			catch (Exception e)
			{
				throw new Exception($"url: {versionUrl}", e);
			}

			// 获取streaming目录的Version.txt
			VersionConfig streamingVersionConfig;
            //得到本地Version文件地址
            string versionPath = Path.Combine(PathHelper.AppResPath4Web, "Version.txt");
			using (UnityWebRequestAsync request = ComponentFactory.Create<UnityWebRequestAsync>())
			{
				await request.DownloadAsync(versionPath);
				streamingVersionConfig = JsonHelper.FromJson<VersionConfig>(request.Request.downloadHandler.text);
			}
			
			// 删掉远程不存在的文件 获取本地热更新文件夹
			DirectoryInfo directoryInfo = new DirectoryInfo(PathHelper.AppHotfixResPath);
			if (directoryInfo.Exists) //如果有这个文件夹
			{
                //得到文件夹内所有文件
				FileInfo[] fileInfos = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in fileInfos)
				{
                    //新的版本配置表中没有这个文件 就删除此文件
					if (remoteVersionConfig.FileInfoDict.ContainsKey(fileInfo.Name))
					{
						continue;
					}

					if (fileInfo.Name == "Version.txt")
					{
						continue;
					}
					
					fileInfo.Delete();
				}
			}
			else
			{
                //否则就创建新的热更新资源文件夹
				directoryInfo.Create();
			}

			// 对比MD5
			foreach (FileVersionInfo fileVersionInfo in remoteVersionConfig.FileInfoDict.Values)
			{
				// 本地
				string localFileMD5 = BundleHelper.GetBundleMD5(streamingVersionConfig, fileVersionInfo.File);
				if (fileVersionInfo.MD5 == localFileMD5)
				{
					continue;
				}
				this.bundles.Enqueue(fileVersionInfo.File); //需要重新下载的AB包的名字
				this.TotalSize += fileVersionInfo.Size;     //需要下载的资源的总大小
			}
		}

        /// <summary>
        /// 下载进度 
        /// </summary>
		public int Progress
		{
			get
			{
				if (this.TotalSize == 0)
				{
					return 0;
				}
                //已经下载的大小
				long alreadyDownloadBytes = 0;
                //得到已经下载AB包的大小
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					long size = this.remoteVersionConfig.FileInfoDict[downloadedBundle].Size;
					alreadyDownloadBytes += size;
				}
                //当前正在下载
				if (this.webRequest != null)
				{
					alreadyDownloadBytes += (long)this.webRequest.Request.downloadedBytes;//总下载大小= 已经下载的AB包大小+ 正在下载的文件中已经下载的大小
				}
				return (int)(alreadyDownloadBytes * 100f / this.TotalSize);  //百分比
			}
		}
        /// <summary>
        /// 开始下载AB包
        /// </summary>
        /// <returns></returns>
		public async Task DownloadAsync()
		{
			if (this.bundles.Count == 0 && this.downloadingBundle == "")
			{
				return;
			}

			try
			{
				while (true)
				{
					if (this.bundles.Count == 0)
					{
						break;
					}

					this.downloadingBundle = this.bundles.Dequeue();

					while (true)
					{
						try
						{
							using (this.webRequest = ComponentFactory.Create<UnityWebRequestAsync>())
							{
                                //异步下载
								await this.webRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + this.downloadingBundle);
								byte[] data = this.webRequest.Request.downloadHandler.data; //下载完成得到数据
                                //写入数据到热更新文件夹
								string path = Path.Combine(PathHelper.AppHotfixResPath, this.downloadingBundle);
								using (FileStream fs = new FileStream(path, FileMode.Create))
								{
									fs.Write(data, 0, data.Length);
								}
							}
						}
						catch (Exception e)
						{
							Log.Error($"download bundle error: {this.downloadingBundle}\n{e}");
							continue;
						}

						break;
					}
                    //加入到已经下载的AB包中
					this.downloadedBundles.Add(this.downloadingBundle);
					this.downloadingBundle = "";
					this.webRequest = null;
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
