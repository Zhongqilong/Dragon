using UnityEngine;
using System;
using System.Collections.Generic;

namespace Uqee.Resource
{
    /// <summary>
    /// 原始资源缓存,保存Request产生的数据
    /// </summary>
    public class AssetsCache : Singleton<AssetsCache>, IAssetsCache
    {
        private Dictionary<string, AssetRequest> _assetsCacheDict = new Dictionary<string, AssetRequest>();
        public IReleaseAdapter releaseAdapter;

        public override void Dispose()
        {
            foreach (var item in _assetsCacheDict)
            {
                try
                {
                    _ReleaseAssets(item.Value);
                }
                catch (Exception e)
                {
                    Uqee.Debug.LogError(e);
                }
                RequestPool.PushToPool(item.Value);
            }
            _assetsCacheDict.Clear();
            base.Dispose();
        }

        public UnityEngine.Object GetObject(string assetPath, string assetName)
        {
            AssetRequest req = null;

            req = GetCache(assetPath);
            if (req != null)
            {
#if ASSET_BUNDLE
                if (string.IsNullOrEmpty(assetName))
                {
                    return null;
                }
                return AssetBundleUtils.GetABSubAsset(assetName, req);
#else
                return req.loadedObj;
#endif
            }
            return null;
        }

        //public void SetCacheIsSystemAssets(string assetPath, bool isSystem)
        //{
        //    if (_assetsCacheDict.ContainsKey(assetPath))
        //    {
        //        _assetsCacheDict[assetPath].isSystemAssets = isSystem;
        //    }
        //}
        public void RemoveCache(string assetPath, bool unloadAggressively = false)
        {
            if (assetPath == null || !_assetsCacheDict.ContainsKey(assetPath))
            {
                return;
            }
            var req = _assetsCacheDict[assetPath];
            try
            {
                _ReleaseAssets(req, unloadAggressively);
            }
            catch (Exception e)
            {
                Uqee.Debug.LogError(e);
            }
            RequestPool.PushToPool(req);

            _assetsCacheDict.Remove(assetPath);
        }
        public AssetRequest GetCache(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            AssetRequest req = null;

            if (_assetsCacheDict.ContainsKey(assetPath))
            {
                req = _assetsCacheDict[assetPath];
            }
            if (req != null && !AppStatus.isApplicationQuit)
            {
                //Log.ResInfo("[Get Asset In Cache] {0}", assetPath);
                req.callCount++;
                req.lastCallTime = AppStatus.realtimeSinceStartup;
                return req;
            }
            return null;
        }

        public void AddCache(AssetRequest req)
        {
            if (string.IsNullOrEmpty(req.assetPath))
            {
                return;
            }
            var instCache = CacheManager.I.GetCache<IInstantiateCache>();
            if (instCache != null && req.loadedObj is GameObject)
            {
                if (instCache.IsInPool(req.category, req.prefabName))
                {
                    if (req.loadedObj == null)
                    {
                        Uqee.Debug.LogError($"[AssetCache]资源不存在:{req.assetName}");
                    }
                    else
                    {
                        if (!instCache.CanSpawn(req.category, req.prefabName))
                        {
                            var prefabObj = req.loadedObj as GameObject;
                            prefabObj.name = req.prefabName;
                            instCache.AddInstantiatePool(req.category, prefabObj);
                        }
                    }
                }
            }
            req.cacheTime = AppStatus.realtimeSinceStartup;

            _assetsCacheDict[req.assetPath] = req;
        }

        private void _ReleaseAssets(AssetRequest req, bool unloadAggressively = false)
        {
            //Uqee.Debug.Log(string.Format("release:{0}, call times:{1}, last call: {2}", req.assetKey, req.callCount, req.lastCallTime));
            var asset = req.loadedObj;

            if (asset is AssetBundle)
            {
                foreach (var pairs in req.subAssets)
                {
                    if (pairs.Value == null) continue;
                    _UnloadAsset(req.category, pairs.Value, unloadAggressively);
                }
            }

            _UnloadAsset(req.category, asset, unloadAggressively);
        }
        private void _UnloadAsset(string category, UnityEngine.Object asset, bool unloadAggressively = false)
        {
            releaseAdapter?.UnloadAssets(category, asset, unloadAggressively);
        }
        const int RELEASE_PRE_FRAME = 10;


        private Queue<string> _tmpKeyQueue = new Queue<string>();
        /// <summary>
        /// 释放加载的资源
        /// </summary>
        /// <param name="all">true.释放所有资源, false.按规则释放部分资源</param>
        /// <returns></returns>
        public void ReleaseCache(bool all)
        {
            RequestPool.Release();
            _tmpKeyQueue.Clear();

            if (all)
            {
                Uqee.Debug.Log("Release all caches.");

                foreach (var item in _assetsCacheDict)
                {
                    if (releaseAdapter == null || releaseAdapter.CanRelease(item.Value))
                    {
                        _tmpKeyQueue.Enqueue(item.Key);
                    }
                }
            }
            else
            {
                // Uqee.Debug.Log("ReleaseCaches");            
                var count = 0;
                foreach (var item in _assetsCacheDict)
                {
                    var req = item.Value;
                    if (releaseAdapter == null || releaseAdapter.CanAutoRelease(req))
                    {
                        count++;
                        // if (count > RELEASE_PRE_FRAME)
                        // {
                        //     break;
                        // }

                        _tmpKeyQueue.Enqueue(item.Key);
                    }
                }
            }
            while (_tmpKeyQueue.Count > 0)
            {
                RemoveCache(_tmpKeyQueue.Dequeue());
            }
        }
    }
}