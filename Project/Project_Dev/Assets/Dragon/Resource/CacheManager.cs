using System.Collections.Generic;

namespace Uqee.Resource
{
    public class CacheManager:Singleton<CacheManager>
    {
        ////实例化对象缓存
        //public static IInstantiateCache instCache;
        ////资源缓存
        //public static IAssetsCache assetsCache;
        ////Manifest数据缓存，包含hash和依赖。(AssetBundle中使用)
        //public static IManifestCache manifestCache;

        private Dictionary<string, IResourceCache> _cacheDict = new Dictionary<string, IResourceCache>();
        private Dictionary<string, bool> _cacheManualDict = new Dictionary<string, bool>();
        /// <summary>
        /// 添加一个缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="manualRelease">是否手动控制ReleaseCache.非手动控制，当调用CacheManager.ReleaseCache。会一起调用</param>
        public void SetCache<T>(T cache, bool manualRelease=false) where T: class, IResourceCache
        {
            var name = typeof(T).Name;
            _cacheDict[name] = cache;
            _cacheManualDict[name] = manualRelease;
        }

        public T GetCache<T>() where T : class, IResourceCache
        {
            IResourceCache cache = null;
            _cacheDict.TryGetValue(typeof(T).Name, out cache);
            return cache as T;
        }

        public void ReleaseCache(bool all)
        {
            foreach(var pairs in _cacheDict)
            {
                if(!_cacheManualDict[pairs.Key])
                {
                    pairs.Value.ReleaseCache(all);
                }
            }
            //assetsCache?.ReleaseCache(all);
            //instCache?.ReleaseCache(all);
        }
        public override void Dispose()
        {
            foreach (var pairs in _cacheDict)
            {
                pairs.Value.Dispose();
            }
            _cacheDict.Clear();
            _cacheManualDict.Clear();
        }
    }
}