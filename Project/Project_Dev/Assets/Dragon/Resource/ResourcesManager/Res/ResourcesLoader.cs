using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Uqee.Resource
{
    public class ResourcesLoader : IAssetLoader
    {

        public UnityEngine.Object Load(string path, string assetname)
        {
            return Resources.Load(path);
        }

        public UnityEngine.Object Load(string path, string assetname, Type systemTypeInstance)
        {
            return Resources.Load(path, systemTypeInstance);
        }

        public T Load<T>(string path, string assetname) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        public T[] LoadAll<T>(string path) where T : UnityEngine.Object
        {
            return Resources.LoadAll<T>(path);
        }

        public UnityEngine.Object[] LoadAll(string path, Type systemTypeInstance)
        {
            return Resources.LoadAll(path, systemTypeInstance);
        }

        public UnityEngine.Object[] LoadAll(string path)
        {
            return Resources.LoadAll(path);
        }

        public LoadRequestBase LoadAsync<T>(string path, string assetname, System.Action<UnityEngine.Object> completed) where T : UnityEngine.Object
        {
            return new ResourcesLoaderRequest(Resources.LoadAsync<T>(path), completed);
        }

        public LoadRequestBase LoadAsync(string path, string assetname, System.Action<UnityEngine.Object> completed)
        {
            return new ResourcesLoaderRequest(Resources.LoadAsync(path), completed);
        }

        public LoadRequestBase LoadAsync(string path, string assetname, Type type, System.Action<UnityEngine.Object> completed)
        {
            return new ResourcesLoaderRequest(Resources.LoadAsync(path, type), completed);
        }

        public LoadRequestBase LoadAsync(string path)
        {
            return LoadAsync(path, null, null);
        }

        public void UnloadAsset(string path)
        {

        }

        public void ForceUnloadAsset(string path)
        {

        }

        public void AddInstantiateObj(UnityEngine.Object obj, string assetPath)
        {
            
        }

        public void AddSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            
        }

        public void AddSpriteAtlasHolder(GameObject gameObject)
        {
            
        }

        public void AddRefCount(string path, int count)
        {
        }

        public void RemoveRefCount(string path, int count)
        {
        }
        public bool IsCached(string path)
        {
            return false;
        }
    }
}
