﻿using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class AssetsLoaderAsyncAwakeSystem : AwakeSystem<AssetsLoaderAsync, AssetBundle>
	{
		public override void Awake(AssetsLoaderAsync self, AssetBundle a)
		{
			self.Awake(a);
		}
	}

	[ObjectSystem]
	public class AssetsLoaderAsyncUpdateSystem : UpdateSystem<AssetsLoaderAsync>
	{
		public override void Update(AssetsLoaderAsync self)
		{
			self.Update();
		}
	}

    /// <summary>
    /// 用 async 封装了 unity的异步加载所有资源API
    /// </summary>
	public class AssetsLoaderAsync : Component
	{
		private AssetBundle assetBundle;

		private AssetBundleRequest request;

		private TaskCompletionSource<bool> tcs;

		public void Awake(AssetBundle ab)
		{
			this.assetBundle = ab;
		}

		public void Update()
		{
			if (!this.request.isDone)
			{
				return;
			}

			TaskCompletionSource<bool> t = tcs;
			t.SetResult(true);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			this.assetBundle = null;
			this.request = null;
		}

		public async Task<UnityEngine.Object[]> LoadAllAssetsAsync()
		{
			await InnerLoadAllAssetsAsync();
			return this.request.allAssets;
		}

		private Task<bool> InnerLoadAllAssetsAsync()
		{
			this.tcs = new TaskCompletionSource<bool>();
			this.request = assetBundle.LoadAllAssetsAsync(); //异步加载AB包里都所有资源
			return this.tcs.Task;
		}
	}
}
