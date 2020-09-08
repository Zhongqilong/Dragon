using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace Uqee.Resource
{

    public class AssetBundleCacheManager
    {

        public static AssetBundleCacheManager I
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssetBundleCacheManager();
                }
                return instance;
            }
        }

        private static AssetBundleCacheManager instance;

        /// <summary>
        /// 路径拼接，减少GC
        /// </summary>
        public StringBuilder pathBuilder = new StringBuilder(256);
        /// <summary>
        /// 路径拼接
        /// </summary>
        public string pathSplit = "/";

        /// <summary>
        /// 每帧最大加载的资源数量
        /// </summary>
        public static int MAX_THREAD_COUNT = 10;
        /// <summary>
        /// 所有已加载的AssetBundle
        /// </summary>
        public Dictionary<string, AssetBundleCache> assetBundles = new Dictionary<string, AssetBundleCache>();

        public List<UnityEngine.U2D.SpriteAtlas> allAtlas = new List< UnityEngine.U2D.SpriteAtlas>();


        public AssetBundleCacheManager()
        {
            GameObject go = new GameObject("AssetBundleProcessor");
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<AssetBundleProcessor>();

        }
        /// <summary>
        /// 移除资源缓存
        /// </summary>
        /// <param name="bundlePath"></param>
        public void RemmoveAssetBundleCache(string bundlePath)
        {
            assetBundles.Remove(bundlePath);
        }
        /// <summary>
        /// 获取资源缓存
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public AssetBundleCache GetAssetBundleCache(string bundlePath)
        {
            AssetBundleCache cache = null;
            assetBundles.TryGetValue(bundlePath, out cache);
            return cache;
        }
        /// <summary>
        /// 创建资源缓存
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public AssetBundleCache Create(string bundlePath)
        {

            AssetBundleCache ret = GetAssetBundleCache(bundlePath);
            if (ret == null)
            {
                ret = new AssetBundleCache(bundlePath, GetAssetBundleRealPath(bundlePath));
                if(ResourcesManager.I.isAssetNull != null && ResourcesManager.I.isAssetNull(ret.assetBundlePath))
                {
                    ret.isError = true;
                    //UnityEngine.Debug.LogError("Cannot find file : " + ret.realPath+"\n"+bundlePath);
                }
                assetBundles.Add(bundlePath, ret);
            }
            //if (bundlePath.Equals("fx_xsj/ui/fx_ui_f39_11.dat"))
            //{
            //    UnityEngine.Debug.LogError("leak asset start load : " + bundlePath);
            //}
            CheckReference(ret);
            AssetBundleProcessor.I.AddRequest(ret);
            return ret;
        }
        /// <summary>
        /// 获取所有加载的AssetBundlePath
        /// </summary>
        /// <returns></returns>
        public string[] GetAllAssetBundlePath()
        {
            string[] ret = new string[assetBundles.Count];
            var keys = assetBundles.Keys;
            int i = 0;
            foreach (var item in keys)
            {
                ret[i] = item;
                i++;
            }
            return ret;
        }
        /// <summary>
        /// 获取当前平台名称
        /// </summary>
        /// <returns></returns>
        public string GetPlatformName()
        {
		    return GetPlatformForAssetBundles(Application.platform);
        }
        /// <summary>
        /// 获取当前平台名称
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }

        /// <summary>
        /// 路径拼接
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string PathCombine(params string[] args)
        {
            if (pathBuilder.Length > 0)
                pathBuilder.Remove(0, pathBuilder.Length);
            for (int i = 0; i < args.Length; i++)
            {
                pathBuilder.Append(args[i]);
                if (i < args.Length - 1)
                {
                    pathBuilder.Append(pathSplit);
                }
            }
            return pathBuilder.ToString();
        }
        /// <summary>
        /// 资源短路径转真实路径
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public string GetAssetBundleRealPath(string bundlePath)
        {
            if(ResourcesManager.I.getRealPath != null)
                return ResourcesManager.I.getRealPath(bundlePath);
            else
            {
                string persistentDataPath = PathCombine(Application.persistentDataPath, GetPlatformName(), "AssetBundles", bundlePath);
                if (File.Exists(persistentDataPath))
                {
                    return persistentDataPath;
                }
                else
                {
                    return PathCombine(Application.streamingAssetsPath, GetPlatformName(), "AssetBundles", bundlePath);
                }
            }


        }
        /// <summary>
        /// 检查所有引用
        /// </summary>
        /// <param name="assetBundleCache"></param>
        public void CheckReference(AssetBundleCache assetBundleCache)
        {

            if (assetBundleCache.allReferences == null)
            {
                if (ResourcesManager.I.getDependencies != null)
                {
                    assetBundleCache.allReferences = ResourcesManager.I.getDependencies(assetBundleCache.assetBundlePath);
                }

            }
            if(assetBundleCache.allReferences == null)
            {
                assetBundleCache.allReferences = new string[0];
            }
            for (int i = 0; i < assetBundleCache.allReferences.Length; i++)
            {
                var dep = Create(assetBundleCache.allReferences[i]);
                dep.AddRefCount();
            }
        }


        


    }
}