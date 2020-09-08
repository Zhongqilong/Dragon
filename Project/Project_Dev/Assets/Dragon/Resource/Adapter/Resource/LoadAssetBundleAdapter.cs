using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Uqee.Pool;

namespace Uqee.Resource
{
    public class LoadAssetBundleAdapter : ILoadAdapter
    {
        private char[] _assetReplaceCharArr = new char[] { ' ', '/' };
        private string _GetAssetBundlePath(string assetName, string assetsDir = null)
        {
            //将字符串中间的空格和目录分隔符替换成 下划线
            string filename = assetName.ReplaceChar(_assetReplaceCharArr, '_');// + "." + hash;
            string folderName = ((byte)(filename.GetHashCode() % 256)).ToString("X2");
            return Path.Combine(assetsDir ?? DirectorySetting.cacheABDir, folderName, filename);
        }
        private AssetBundle _LoadAB(string path)
        {
            try
            {
                //Uqee.Debug.Log(string.Format("[Load AssetBundle Sync] path={0}", path), Color.green);
                return AssetBundle.LoadFromFile(path);
            }
            catch (Exception )
            {
                // Uqee.Debug.LogError(ex);
            }
            return null;
        }
        private AssetRequest _GetABRequest(string category, string assetName, string assetPath, bool isSystemAssets)
        {
            string abPath = _GetABRealPath(assetPath);

            var info = CacheManager.I.GetCache<IManifestCache>()?.GetCache(assetPath);
            string hashStr = null;
            _abHashDict.TryGetValue(assetPath, out hashStr);
            if (info != null && hashStr != info.hash)
            {
                //hash变更了，需要重新加载
                CacheManager.I.GetCache<IAssetsCache>()?.RemoveCache(assetPath);
                hashStr = info.hash;
            }

            var abReq = CacheManager.I.GetCache<IAssetsCache>()?.GetCache(assetPath);
            if (abReq != null && abReq.loadedObj == null)
            {
                Uqee.Debug.LogError("AssetBundle丢失？" + assetName);
            }
            if (abReq == null && info!=null)
            {
                var assetBundle = _LoadAB(abPath);
                if (assetBundle == null)
                {
                    abReq = null;
                }
                else
                {
                    abReq = _SetABToCache(category, assetName, assetPath, hashStr, assetBundle, isSystemAssets);
                }
            }

            return abReq;
        }
        private AssetRequest _GetABRequest(AssetRequest req)
        {
            return _GetABRequest(req.category, req.assetName, req.assetPath, req.isSystemAssets);
        }
        private Dictionary<string, string> _abHashDict = new Dictionary<string, string>();
        private AssetRequest _SetABToCache(string category, string assetName, string assetPath, string hash, AssetBundle assetBundle, bool isSystemAssets)
        {
            var abReq = DataFactory<AssetRequest>.Get();
            abReq.category = category;
            abReq.assetName = assetName;
            abReq.assetPath = assetPath;
            abReq.isSystemAssets = isSystemAssets;
            abReq.loadedObj = assetBundle;
            abReq.isAB = true;
            abReq.hashStr = hash;
            if (!string.IsNullOrEmpty(hash))
            {
                _abHashDict[assetPath] = hash;
            }
            // RequestPool.GetCallCount(abReq);

            CacheManager.I.GetCache<IAssetsCache>()?.AddCache(abReq);
            return abReq;
        }
        private string _GetABRealPath(string assetPath)
        {
            string _tmpPath = null;
            var isExists = false;
            if (CDNSetting.useCDN)
            {
                _tmpPath = _GetAssetBundlePath(assetPath);
                isExists = File.Exists(_tmpPath);
            }
            if (!isExists)
            {
                //默认目录
                _tmpPath = _GetAssetBundlePath(assetPath, DirectorySetting.cacheInternalDir);
            }
            return _tmpPath;
        }

        public T LoadSync<T>(AssetRequest req) where T : UnityEngine.Object
        {
            var manifestCache = CacheManager.I.GetCache<IManifestCache>();
            if (manifestCache?.GetCache(req.assetPath) != null)
            {
                var abReq = _GetABRequest(req.category, req.assetName, req.assetPath, req.isSystemAssets);

                if (abReq != null)
                {
                    return AssetBundleUtils.GetABSubAsset(req.assetName, abReq) as T;
                }
            }

            Uqee.Debug.LogWarning(string.Format("[Get AssetBundle Sync]asset not found:{0} --> path={1}", req.assetName, req.assetPath));
            return null;
        }
        //System.Collections.IEnumerator AssetBundleUtils.GetABSubAssetCor(AssetRequest req, AssetRequest abReq)
        //{
        //    if (abReq.loadedObj == null)
        //    {
        //        yield break;
        //    }
        //    string assetName = req.assetName;

        //    UnityEngine.Object obj = null;
        //    string name = null;
        //    if (string.IsNullOrEmpty(assetName))
        //    {
        //        var keys = abReq.subAssets.Keys;
        //        if (keys.Count > 0)
        //        {
        //            var et = keys.GetEnumerator();
        //            if (et.MoveNext())
        //            {
        //                name = et.Current;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        name = assetName.GetSplitLast(FILE_SPLIT);
        //    }
        //    if (name != null)
        //    {
        //        abReq.subAssets.TryGetValue(name, out obj);
        //    }

        //    if (obj == null)
        //    {
        //        AssetBundle ab = abReq.loadedObj as AssetBundle;
        //        var arr = ab.GetAllAssetNames();
        //        if (arr.Length == 0)
        //        {
        //            yield break;
        //        }
        //        if (string.IsNullOrEmpty(assetName))
        //        {
        //            assetName = arr[0].GetSplitFirst('.');
        //            name = assetName.GetSplitLast(FILE_SPLIT);
        //        }

        //        var reqAsync = ab.LoadAssetAsync(name);
        //        yield return reqAsync;

        //        if (reqAsync.asset != null)
        //        {
        //            req.loadedObj = reqAsync.asset;
        //            _AddToPool(req.loadedObj as GameObject, req.category, req.prefabName);
        //        }
        //        abReq.subAssets[name] = reqAsync.asset;

        //    }

        //    yield return null;
        //}
        public System.Collections.IEnumerator LoadAsync(AssetRequest req)
        {
            Uqee.Debug.Log($"[Get AssetBundle]category={req.category.ToString()}. assetName={req.assetName}. path={req.assetPath}", Color.yellow);
            var abReq = _GetABRequest(req);

            if (abReq == null)
            {
                Uqee.Debug.LogWarning($"[Get AssetBundle] failed. path={req.assetPath}. realPath={_GetABRealPath(req.assetPath)}");
                req.error = "assetbundle load failed";
            }
            else
            {
                //Uqee.Debug.LogWarning($"[Get AssetBundle Assets] name={req.assetName}.");
                req.loadedObj = AssetBundleUtils.GetABSubAsset(req.assetName, abReq);
                //yield return AssetBundleUtils.GetABSubAssetCor(req, abReq);

                yield return null;
                //_SetAssetsToCache(req);
                lock (req.sameRequestQueue)
                {
                    var cnt = req.sameRequestQueue.Count;
                    for(int i=0; i<cnt; i++)
                    {
                        req.sameRequestQueue[i].loadedObj = AssetBundleUtils.GetABSubAsset(req.sameRequestQueue[i].assetName, abReq);
                        //yield return null;
                    }
                    //foreach (var sameReq in req.sameRequestQueue)
                    //{
                    //    sameReq.loadedObj = AssetBundleUtils.GetABSubAsset(sameReq.assetName, abReq);
                    //    //yield return AssetBundleUtils.GetABSubAssetCor(sameReq, abReq);

                    //    yield return null;
                    //}
                }
                //Uqee.Debug.LogWarning($"[Get AssetBundle Assets] finish={req.assetName}.");
            }
        }
    }
}