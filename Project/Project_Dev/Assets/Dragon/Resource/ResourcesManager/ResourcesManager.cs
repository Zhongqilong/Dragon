using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Uqee.Resource
{

    public class ResourcesManager
    {
        public delegate string GetFixAssetPathDelegate(string category, string assetName);
        public delegate string GetRealPathDelegate(string bundlePath);
        public delegate string[] GetDependenciesDelegate(string bundlePath);
        public delegate bool IsAssetNullDelegate(string bundlePath);

        public GetFixAssetPathDelegate getFixAssetPath;
        public GetRealPathDelegate getRealPath;
        public GetDependenciesDelegate getDependencies;
        public IsAssetNullDelegate isAssetNull;

        private static ResourcesManager instance;

        public static ResourcesManager I
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResourcesManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// 加载器，用于不同模式加载资源
        /// </summary>
        public IAssetLoader assetLoader = new ResourcesLoader();

        //
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <returns></returns>
        public UnityEngine.Object Load(string path, string assetname)
        {
            return assetLoader.Load(path, assetname);
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="systemTypeInstance"></param>
        /// <returns></returns>
        public UnityEngine.Object Load(string path, string assetname, Type systemTypeInstance)
        {
            return assetLoader.Load(path, assetname, systemTypeInstance);
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <returns></returns>
        public T Load<T>(string path, string assetname) where T : UnityEngine.Object
        {
            return assetLoader.Load<T>(path, assetname);
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T[] LoadAll<T>(string path) where T : UnityEngine.Object
        {
            return assetLoader.LoadAll<T>(path);
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="systemTypeInstance"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAll(string path, Type systemTypeInstance)
        {
            return assetLoader.LoadAll(path, systemTypeInstance);
        }
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAll(string path)
        {
            return assetLoader.LoadAll(path);
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public LoadRequestBase LoadAsync<T>(string path, string assetname, Action<UnityEngine.Object> completed) where T : UnityEngine.Object
        {
            return (assetLoader.LoadAsync<T>(path, assetname, completed));
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public LoadRequestBase LoadAsync(string path, string assetname, Action<UnityEngine.Object> completed)
        {
            return (assetLoader.LoadAsync(path, assetname, completed));
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="type"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public LoadRequestBase LoadAsync(string path, string assetname, Type type, Action<UnityEngine.Object> completed)
        {
            return (assetLoader.LoadAsync(path, assetname, type, completed));
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public LoadRequestBase LoadAsync(string path)
        {
            return (assetLoader.LoadAsync(path));
        }

        /// <summary>
        /// 尝试卸载资源
        /// </summary>
        /// <param name="path"></param>
        public void UnloadAsset(string path)
        {
            assetLoader.UnloadAsset(path);
        }
        /// <summary>
        /// 强制卸载资源
        /// </summary>
        /// <param name="path"></param>
        public void ForceUnloadAsset(string path)
        {
            assetLoader.ForceUnloadAsset(path);
        }
        /// <summary>
        /// 增加对应资源引用计数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        public void AddRefCount(string path, int count)
        {
            assetLoader.AddRefCount(path,count);

        }
        /// <summary>
        /// 减少对应资源引用计数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        public void RemoveRefCount(string path, int count)
        {
            assetLoader.RemoveRefCount(path,count);
        }
        /// <summary>
        /// 资源是否已加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsCached(string path)
        {
            return assetLoader.IsCached(path);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public IEnumerator LoadAsyncAsset(string path, string assetname, Action<UnityEngine.Object> completed)
        {
            var temp = LoadAsync(path, assetname, completed);
            while (!temp.isDone)
            {
                yield return null;
            }
        }
        /// <summary>
        /// 添加Instantiate物体检查，当所有Instantiate物体销毁后自动卸载对应资源
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="category"></param>
        /// <param name="assetName"></param>
        public void AddInstantiateObj(UnityEngine.Object obj, string category, string assetName)
        {
            AddInstantiateObj(obj, GetFixAssetPath(category, assetName));
        }
        /// <summary>
        /// 添加Instantiate物体检查，当所有Instantiate物体销毁后自动卸载对应资源
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="assetPath"></param>
        public void AddInstantiateObj(UnityEngine.Object obj, string assetPath)
        {
            assetLoader.AddInstantiateObj(obj, assetPath);
        }

        /// <summary>
        /// 拼接资源路径
        /// </summary>
        /// <param name="category"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetFixAssetPath(string category, string assetName)
        {
            if(getFixAssetPath != null)
            {
                return getFixAssetPath(category, assetName);
            }
            return assetName;
        }

    }
}
