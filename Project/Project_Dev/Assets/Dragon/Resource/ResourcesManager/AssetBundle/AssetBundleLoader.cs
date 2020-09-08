using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Uqee.Resource
{
    public class AssetBundleLoader : IAssetLoader
    {

        public UnityEngine.Object Load(string path, string assetname)
        {
            return AssetBundleCacheManager.I.Create(path).Load(assetname);
        }

        public UnityEngine.Object Load(string path, string assetname, Type systemTypeInstance)
        {
            return AssetBundleCacheManager.I.Create(path).Load(assetname, systemTypeInstance);
        }

        public T Load<T>(string path, string assetname) where T : UnityEngine.Object
        {
            return AssetBundleCacheManager.I.Create(path).Load<T>(assetname);
        }

        public T[] LoadAll<T>(string path) where T : UnityEngine.Object
        {
            return AssetBundleCacheManager.I.Create(path).LoadAll<T>();
        }

        public UnityEngine.Object[] LoadAll(string path, Type systemTypeInstance)
        {
            return AssetBundleCacheManager.I.Create(path).LoadAll(systemTypeInstance);
        }

        public UnityEngine.Object[] LoadAll(string path)
        {
            return AssetBundleCacheManager.I.Create(path).LoadAll();
        }

        public LoadRequestBase LoadAsync<T>(string path, string assetname, Action<UnityEngine.Object> completed) where T : UnityEngine.Object
        {
            return AssetBundleCacheManager.I.Create(path).LoadAsync<T>(assetname, completed);
        }

        public LoadRequestBase LoadAsync(string path, string assetname, Action<UnityEngine.Object> completed)
        {
            return AssetBundleCacheManager.I.Create(path).LoadAsync(assetname, completed);
        }

        public LoadRequestBase LoadAsync(string path, string assetname, Type type, Action<UnityEngine.Object> completed)
        {
            return AssetBundleCacheManager.I.Create(path).LoadAsync(assetname, type, completed);
        }

        public LoadRequestBase LoadAsync(string path)
        {
            return LoadAsync(path, null, null);
        }

        public void UnloadAsset(string path)
        {
            var request = AssetBundleCacheManager.I.GetAssetBundleCache(path);
            if (request != null)
            {
                request.RemoveRefCount();
                request.Unload();
            }
        }

        public void ForceUnloadAsset(string path)
        {
            var request = AssetBundleCacheManager.I.GetAssetBundleCache(path);
            if (request != null)
            {
                request.ForceUnload();
            }
            AssetBundleProcessor.I.UnloadAsset(path);
        }

        /// <summary>
        /// 添加Instantiate物体检查，当所有Instantiate物体销毁后自动卸载对应AB
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="assetPath"></param>
        public void AddInstantiateObj(UnityEngine.Object obj, string assetPath)
        {
            if (null == obj)
            {
                return;
            }
            AssetBundleProcessor.I.AddABInstantiateObj(assetPath, obj );
        }

        public void AddRefCount(string path, int count)
        {
            AssetBundleCacheManager.I.GetAssetBundleCache(path).AddRefCount(count);
        }

        public void RemoveRefCount(string path, int count)
        {
            AssetBundleCacheManager.I.GetAssetBundleCache(path).RemoveRefCount(count);
        }

        public bool IsCached(string path)
        {
            return AssetBundleCacheManager.I.GetAssetBundleCache(path) != null;
        }
    }
}