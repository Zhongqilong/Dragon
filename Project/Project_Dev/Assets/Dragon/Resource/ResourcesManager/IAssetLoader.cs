using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Uqee.Resource
{
    using Object = UnityEngine.Object;
    public interface IAssetLoader
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <returns></returns>
        Object Load(string path, string assetname);
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="systemTypeInstance"></param>
        /// <returns></returns>
        Object Load(string path, string assetname, Type systemTypeInstance);
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <returns></returns>
        T Load<T>(string path, string assetname) where T : Object;
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        T[] LoadAll<T>(string path) where T : Object;
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="systemTypeInstance"></param>
        /// <returns></returns>
        Object[] LoadAll(string path, Type systemTypeInstance);
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Object[] LoadAll(string path);
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        LoadRequestBase LoadAsync<T>(string path, string assetname, System.Action<Object> completed) where T : UnityEngine.Object;
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        LoadRequestBase LoadAsync(string path, string assetname, System.Action<Object> completed);
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetname"></param>
        /// <param name="type"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        LoadRequestBase LoadAsync(string path, string assetname, Type type, System.Action<Object> completed);
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        LoadRequestBase LoadAsync(string path);
        /// <summary>
        /// 卸载物体
        /// </summary>
        /// <param name="path"></param>
        void UnloadAsset(string path);
        /// <summary>
        /// 强制卸载物体
        /// </summary>
        /// <param name="path"></param>
        void ForceUnloadAsset(string path);

        /// <summary>
        /// 添加Instantiate物体检查，当所有Instantiate物体销毁后自动卸载对应AB
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="assetPath"></param>
        void AddInstantiateObj(UnityEngine.Object obj, string assetPath);


        /// <summary>
        /// 增加对应资源引用计数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        void AddRefCount(string path,int count);
        /// <summary>
        /// 减少对应资源引用计数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        void RemoveRefCount(string path,int count);
        /// <summary>
        /// 资源是否已加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsCached(string path);

    }
}