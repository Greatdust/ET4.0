using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ETModel
{
	[ObjectSystem]
	public class UnityWebRequestUpdateSystem : UpdateSystem<UnityWebRequestAsync>
	{
		public override void Update(UnityWebRequestAsync self)
		{
			self.Update();
		}
	}
	
    /// <summary>
    /// unity下载WAP文件异步请求
    /// </summary>
	public class UnityWebRequestAsync : Component
	{
        /// <summary>
        /// 网络请求方式 UNITY下载文件API
        /// </summary>
		public UnityWebRequest Request;
        /// <summary>
        /// 是否取消
        /// </summary>
		public bool isCancel;
        
		public TaskCompletionSource<bool> tcs;
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.Request?.Dispose();
			this.Request = null;
			this.isCancel = false;
		}

        /// <summary>
        /// 下载进度
        /// </summary>
		public float Progress
		{
			get
			{
				if (this.Request == null)
				{
					return 0;
				}
				return this.Request.downloadProgress;
			}
		}
        /// <summary>
        /// 下载的字节数
        /// </summary>
		public ulong ByteDownloaded
		{
			get
			{
				if (this.Request == null)
				{
					return 0;
				}
				return this.Request.downloadedBytes;
			}
		}

		public void Update()
		{
            //如果取消下载  异步返回 false
            if (this.isCancel)
			{
				this.tcs.SetResult(false);
				return;
			}
			//下载没有完成  退出
			if (!this.Request.isDone)
			{
				return;
			}
            //下载出现错误
			if (!string.IsNullOrEmpty(this.Request.error))
			{
				this.tcs.SetException(new Exception($"request error: {this.Request.error}"));
				return;
			}
            //完成
			this.tcs.SetResult(true);
		}

        /// <summary>
        /// 下载资源
        /// </summary>
        /// <param name="url">资源地址</param>
        /// <returns></returns>
		public Task<bool> DownloadAsync(string url)
		{
			this.tcs = new TaskCompletionSource<bool>();
			
			url = url.Replace(" ", "%20");
			this.Request = UnityWebRequest.Get(url);
			this.Request.SendWebRequest();  //开始请求


            return this.tcs.Task;
		}
	}
}
