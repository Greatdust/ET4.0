using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ETModel
{
    /// <summary>
    /// AB包信息
    /// </summary>
	public class ABInfo : Component
	{
		private int refCount;
        /// <summary>
        /// AB包名字
        /// </summary>
		public string Name { get; }
        /// <summary>
        /// 被引用次数
        /// </summary>
		public int RefCount
		{
			get
			{
				return this.refCount;
			}
			set
			{
				//Log.Debug($"{this.Name} refcount: {value}");
				this.refCount = value;
			}
		}
        /// <summary>
        /// AB包
        /// </summary>
		public AssetBundle AssetBundle { get; }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ab"></param>
		public ABInfo(string name, AssetBundle ab)
		{
			this.InstanceId = IdGenerater.GenerateId();
			
			this.Name = name;
			this.AssetBundle = ab;
			this.RefCount = 1;
			//Log.Debug($"load assetbundle: {this.Name}");
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			//Log.Debug($"desdroy assetbundle: {this.Name}");

			this.AssetBundle?.Unload(true);
		}
	}
	
	// 用于字符串转换，减少GC
	public static class AssetBundleHelper
	{
        //INT转换string
		public static readonly Dictionary<int, string> IntToStringDict = new Dictionary<int, string>();
		//名字加后缀
		public static readonly Dictionary<string, string> StringToABDict = new Dictionary<string, string>();
        //大小写转换
		public static readonly Dictionary<string, string> BundleNameToLowerDict = new Dictionary<string, string>() 
		{
			{ "StreamingAssets", "StreamingAssets" }
		};
		
		// 缓存包依赖，不用每次计算
		public static Dictionary<string, string[]> DependenciesCache = new Dictionary<string, string[]>();

        /// <summary>
        /// 把INT型转换为String 做缓存 免得每一次都需要强转
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public static string IntToString(this int value)
		{
			string result;
			if (IntToStringDict.TryGetValue(value, out result))
			{
				return result;
			}
            //字典里有就返回 没有就新建
			result = value.ToString();
			IntToStringDict[value] = result;
			return result;
		}
        /// <summary>
        /// 把AB包的名字后面加上.unity3d后缀 放入字典只是为了缓存节约新性能
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StringToAB(this string value)
		{
			string result;
			if (StringToABDict.TryGetValue(value, out result))
			{
				return result;
			}

			result = value + ".unity3d";
			StringToABDict[value] = result;
			return result;
		}

        /// <summary>
        /// 把INT转换成String并且加上后缀.unity3d
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public static string IntToAB(this int value)
		{
			return value.IntToString().StringToAB();
		}
		
        /// <summary>
        /// 大小写转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public static string BundleNameToLower(this string value)
		{
			string result;
			if (BundleNameToLowerDict.TryGetValue(value, out result))
			{
				return result;
			}

			result = value.ToLower();  //转换为小写字母
			BundleNameToLowerDict[value] = result;
			return result;
		}
		
   
        /// <summary>
        /// 获得AB包的依赖（）并从小到大排序（依赖数量）
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
		public static string[] GetSortedDependencies(string assetBundleName)
		{
			Dictionary<string, int> info = new Dictionary<string, int>();
			List<string> parents = new List<string>();
			CollectDependencies(parents, assetBundleName, info);
            //按照他所有依赖的依赖数量从小到大排序，并把 KEY（也就是AB包的名字）放入数组中
			string[] ss = info.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
			return ss;
		}

        /// <summary>
        /// 递归遍历搜集一个AB包的所有依赖（包括子包的依赖）
        /// </summary>
        /// <param name="parents"></param>
        /// <param name="assetBundleName"></param>
        /// <param name="info"></param>
		public static void CollectDependencies(List<string> parents, string assetBundleName, Dictionary<string, int> info)
		{
			parents.Add(assetBundleName);
			string[] deps = GetDependencies(assetBundleName);
			foreach (string parent in parents)
			{
				if (!info.ContainsKey(parent)) //如果没有
				{
					info[parent] = 0;
				}
				info[parent] += deps.Length;
			}


			foreach (string dep in deps)
			{
				if (parents.Contains(dep))
				{
					throw new Exception($"包有循环依赖，请重新标记: {assetBundleName} {dep}");
				}
				CollectDependencies(parents, dep, info); //递归搜索
			}
			parents.RemoveAt(parents.Count - 1);
		}

        /// <summary>
        /// 获得依赖资源
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public static string[] GetDependencies(string assetBundleName)
        {
            string[] dependencies = new string[0];
            if (DependenciesCache.TryGetValue(assetBundleName, out dependencies))
            {
                //先查找缓存字典
                return dependencies;
            }
            if (!Define.IsAsync)
            {
#if UNITY_EDITOR  //在unity 编辑器下 获得资源的全部依赖 这里是用AssetDatabase直接获取
                dependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
#endif
            }
            else
            {
                //打包后每个AB包会有AssetBundleManifest记录依赖项 更省性能
                dependencies = ResourcesComponent.AssetBundleManifestObject.GetAllDependencies(assetBundleName);
            }
            DependenciesCache.Add(assetBundleName, dependencies); //缓存
            return dependencies;   //返回依赖
        }

    }


    public class ResourcesComponent : Component
	{
        /// <summary>
        /// unity记录依赖的文件
        /// </summary>
		public static AssetBundleManifest AssetBundleManifestObject { get; set; }
        /// <summary>
        /// 资源缓存
        /// </summary>
		private readonly Dictionary<string, Dictionary<string, UnityEngine.Object>> resourceCache = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();
        /// <summary>
        /// 已经加载的AB包
        /// </summary>
		private readonly Dictionary<string, ABInfo> bundles = new Dictionary<string, ABInfo>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
            //卸载AB包
			foreach (var abInfo in this.bundles)
			{
				abInfo.Value?.AssetBundle?.Unload(true);
			}

			this.bundles.Clear();
			this.resourceCache.Clear();
		}
        /// <summary>
        /// 取得资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
		public UnityEngine.Object GetAsset(string bundleName, string prefab)
		{
			Dictionary<string, UnityEngine.Object> dict;
            //发现缓存字典里面没有加载AB包 就报错
			if (!this.resourceCache.TryGetValue(bundleName.BundleNameToLower(), out dict))
			{
				throw new Exception($"not found asset: {bundleName} {prefab}");
			}
            //发现AB包里面没有需要的prefab 也报错
            UnityEngine.Object resource = null;
			if (!dict.TryGetValue(prefab, out resource))
			{
				throw new Exception($"not found asset: {bundleName} {prefab}");
			}

			return resource;  
		}
        /// <summary>
        /// 卸载AB包
        /// </summary>
        /// <param name="assetBundleName"></param>
		public void UnloadBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
            //先得到这个AB包的所有依赖
			string[] dependencies = AssetBundleHelper.GetSortedDependencies(assetBundleName);
            //递归遍历 卸载子包
			//Log.Debug($"-----------dep unload {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				this.UnloadOneBundle(dependency);
			}
		}
        /// <summary>
        /// 卸载一个AB包
        /// </summary>
        /// <param name="assetBundleName"></param>
		private void UnloadOneBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();

			ABInfo abInfo;
            //如果这个AB包并没有被加载  报错
			if (!this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				throw new Exception($"not found assetBundle: {assetBundleName}");
			}
			
			//Log.Debug($"---------- unload one bundle {assetBundleName} refcount: {abInfo.RefCount - 1}");
            //将这个包的被引用次数-1
			--abInfo.RefCount;
            
			if (abInfo.RefCount > 0)
			{
				return;
			}

            //如果被引用次数小于0了 就卸载
			this.bundles.Remove(assetBundleName);
			abInfo.Dispose();
			//Log.Debug($"cache count: {this.cacheDictionary.Count}");
		}

		/// <summary>
		/// 同步加载assetbundle
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <returns></returns>
		public void LoadBundle(string assetBundleName)
		{
			assetBundleName = assetBundleName.ToLower();
			string[] dependencies = AssetBundleHelper.GetSortedDependencies(assetBundleName);
			//Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
			foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				this.LoadOneBundle(dependency);
			}
        }

        /// <summary>
        /// 把一个Object 添加到资源缓存中
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="resource"></param>
		public void AddResource(string bundleName, string assetName, UnityEngine.Object resource)
		{
			Dictionary<string, UnityEngine.Object> dict;
			if (!this.resourceCache.TryGetValue(bundleName.BundleNameToLower(), out dict))
			{
				dict = new Dictionary<string, UnityEngine.Object>();
				this.resourceCache[bundleName] = dict;
			}

			dict[assetName] = resource;
		}
        /// <summary>
        /// 加载一个AB包
        /// </summary>
        /// <param name="assetBundleName"></param>
		public void LoadOneBundle(string assetBundleName)
		{
			//Log.Debug($"---------------load one bundle {assetBundleName}");
			ABInfo abInfo; //如果已经加载过了 期被引用次数+1
			if (this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				++abInfo.RefCount;
				return;
			}

			if (!Define.IsAsync)
			{
				string[] realPath = null;
#if UNITY_EDITOR  //在编辑器模式下 
				realPath = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
				foreach (string s in realPath)
				{
					string assetName = Path.GetFileNameWithoutExtension(s);
					UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
					AddResource(assetBundleName, assetName, resource);
				}

				this.bundles[assetBundleName] = new ABInfo(assetBundleName, null);
#endif
				return;
			}
            //在打包出来后
			string p = Path.Combine(PathHelper.AppHotfixResPath, assetBundleName);
			AssetBundle assetBundle = null;
			if (File.Exists(p))
			{
                //从本地加载 LoadFromFile(无需用协程)
                assetBundle = AssetBundle.LoadFromFile(p);
			}
			else
			{
				p = Path.Combine(PathHelper.AppResPath, assetBundleName);
				assetBundle = AssetBundle.LoadFromFile(p);
			}

			if (assetBundle == null)
			{
				throw new Exception($"assets bundle not found: {assetBundleName}");
			}

			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				// 异步load资源到内存cache住
				UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
				foreach (UnityEngine.Object asset in assets)
				{
					AddResource(assetBundleName, asset.name, asset); //加入资源缓存中
				}
			}
            //保存AB包
			this.bundles[assetBundleName] = new ABInfo(assetBundleName, assetBundle);
		}

		/// <summary>
		/// 异步加载assetbundle
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <returns></returns>
		public async Task LoadBundleAsync(string assetBundleName)
		{
            assetBundleName = assetBundleName.ToLower();
			string[] dependencies = AssetBundleHelper.GetSortedDependencies(assetBundleName);
            // Log.Debug($"-----------dep load {assetBundleName} dep: {dependencies.ToList().ListToString()}");
            foreach (string dependency in dependencies)
			{
				if (string.IsNullOrEmpty(dependency))
				{
					continue;
				}
				await this.LoadOneBundleAsync(dependency);
			}
        }

		public async Task LoadOneBundleAsync(string assetBundleName)
		{
			ABInfo abInfo;
			if (this.bundles.TryGetValue(assetBundleName, out abInfo))
			{
				++abInfo.RefCount;
				return;
			}

            //Log.Debug($"---------------load one bundle {assetBundleName}");
            if (!Define.IsAsync)
			{
				string[] realPath = null;
#if UNITY_EDITOR
				realPath = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
				foreach (string s in realPath)
				{
					string assetName = Path.GetFileNameWithoutExtension(s);
					UnityEngine.Object resource = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
					AddResource(assetBundleName, assetName, resource);
				}

				this.bundles[assetBundleName] = new ABInfo(assetBundleName, null);
#endif
				return;
			}

			string p = Path.Combine(PathHelper.AppHotfixResPath, assetBundleName);
			AssetBundle assetBundle = null;
			if (!File.Exists(p))
			{
				p = Path.Combine(PathHelper.AppResPath, assetBundleName);
			}
			
			using (AssetsBundleLoaderAsync assetsBundleLoaderAsync = ComponentFactory.Create<AssetsBundleLoaderAsync>())
			{
				assetBundle = await assetsBundleLoaderAsync.LoadAsync(p);
			}

			if (assetBundle == null)
			{
				throw new Exception($"assets bundle not found: {assetBundleName}");
			}

			if (!assetBundle.isStreamedSceneAssetBundle)
			{
				// 异步load资源到内存cache住
				UnityEngine.Object[] assets;
				using (AssetsLoaderAsync assetsLoaderAsync = ComponentFactory.Create<AssetsLoaderAsync, AssetBundle>(assetBundle))
				{
					assets = await assetsLoaderAsync.LoadAllAssetsAsync();
				}
				foreach (UnityEngine.Object asset in assets)
				{
					AddResource(assetBundleName, asset.name, asset);
				}
			}

			this.bundles[assetBundleName] = new ABInfo(assetBundleName, assetBundle);
		}

		public string DebugString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (ABInfo abInfo in this.bundles.Values)
			{
				sb.Append($"{abInfo.Name}:{abInfo.RefCount}\n");
			}
			return sb.ToString();
		}
	}
}