using UnityEngine;
using Uqee.Pool;

namespace Uqee.Resource
{
    public class LoadResourcesAdapter : ILoadAdapter
    {
        public T LoadSync<T>(AssetRequest req) where T : Object
        {
            var cacheAsset = CacheManager.I.GetCache<IAssetsCache>()?.GetObject(req.assetPath, req.assetName);
            if (cacheAsset != null)
            {
                return cacheAsset as T;
            }
            Uqee.Debug.Log(string.Format("[Get Assets Sync] {0}", req.assetPath), Color.yellow);
            T asset = Resources.Load<T>(req.assetPath);

            _CreateAssetsCache(asset, req.category, req.assetName, req.assetPath, req.isSystemAssets);

            return asset;
        }
        private void _CreateAssetsCache(UnityEngine.Object asset, string category, string assetName, string assetPath, bool isSystemAssets)
        {
            var req = DataFactory<AssetRequest>.Get();
            req.category = category;
            req.assetName = assetName;
            req.assetPath = assetPath;
            req.isSystemAssets = isSystemAssets;
            req.loadedObj = asset;
            AssetRequest.GetCallCount(req);
            req.callCount++;
            req.lastCallTime = AppStatus.realtimeSinceStartup;
            CacheManager.I.GetCache<IAssetsCache>()?.AddCache(req);
        }

        public System.Collections.IEnumerator LoadAsync(AssetRequest req)
        {
            if (string.IsNullOrEmpty(req.assetPath))
            {
                Uqee.Debug.LogWarning("[Get Assets] failed. path is empty");
                req.error = "path is empty";
            }
            else
            {
                Uqee.Debug.Log($"[Get Assets] {req.assetPath}", Color.yellow);

                var _tmpAsset = CacheManager.I.GetCache<IAssetsCache>()?.GetObject(req.assetPath, req.assetName);

                if (_tmpAsset != null)
                {
                    req.loadedObj = _tmpAsset;
                }
                else
                {
                    var reqAsync = Resources.LoadAsync<UnityEngine.Object>(req.assetPath);
                    yield return reqAsync;
                    if (reqAsync.asset == null)
                    {
                        Uqee.Debug.LogWarning($"[Get Assets] failed. asset={req.assetPath} : assets not exists.");
                        req.error = "assets not exists.";
                    }
                    else
                    {
                        req.loadedObj = reqAsync.asset;
                        CacheManager.I.GetCache<IAssetsCache>()?.AddCache(req);
                    }
                }
            }
        }
    }
}