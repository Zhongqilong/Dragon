using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Uqee.Resource
{
    public class AssetBundleCache : LoadRequestBase
    {
        /// <summary>
        /// 资源短路径
        /// </summary>
        public string assetBundlePath;

        /// <summary>
        /// 同请求合并的完成回调
        /// </summary>
        private List<System.Action> completedActions = new List<System.Action>();

        public bool isError = false;

        /// <summary>
        /// 获取AssetBundle
        /// </summary>
        public AssetBundle assetBundle
        {
            get
            {
                if(isError)
                {
                    return null;
                }
                if (allReferences != null && allReferences.Length > 0)
                {
                    AssetBundle temp = null;
                    for (int i = 0; i < allReferences.Length; i++)
                    {
                        var dep = AssetBundleCacheManager.I.GetAssetBundleCache(allReferences[i]);
                        if (dep != null && dep.isDone == false)
                        {
                            temp = dep.assetBundle;
                        }
                    }
                    temp = null;
                }
                if (request == null)
                {
                    StartLoad();
                }
                return request.assetBundle;
            }
        }
        /// <summary>
        /// 资源所有引用
        /// </summary>
        public string[] allReferences;
        /// <summary>
        /// 资源真实路径
        /// </summary>
        public string realPath;
        /// <summary>
        /// 引用计数
        /// </summary>
        public int refCount = 0;
        /// <summary>
        /// AssetBundle加载Request
        /// </summary>
        public AssetBundleCreateRequest request;

        public AssetBundleCache(string bundlePath, string realPath)
        {
            assetBundlePath = bundlePath;
            this.realPath = realPath;
        }
        public override void StartLoad()
        {
            base.StartLoad();
            if (request == null && !isError)
            {
                try
                {
                    request = AssetBundle.LoadFromFileAsync(this.realPath);
                }catch(System.Exception)
                {

                }
                if(request == null)
                {
                    isError = true;
                }
            }
        }

        public override bool isDone
        {
            get
            {
                if (allReferences != null && allReferences.Length > 0)
                {
                    for (int i = 0; i < allReferences.Length; i++)
                    {
                        var dep = AssetBundleCacheManager.I.GetAssetBundleCache(allReferences[i]);
                        if (dep != null && dep.isDone == false)
                        {
                            return false;
                        }
                    }
                }
                bool isSuccess = isError || (request != null && request.isDone && request.assetBundle != null );
                   
                if (isSuccess)
                {
                    lock (completedActions)
                    {
                        if (completedActions.Count > 0)
                        {
                            for (int i = 0; i < completedActions.Count; i++)
                            {
                                completedActions[i]();
                            }
                        }
                        completedActions.Clear();
                    }
                    return true;
                }
                return false;
            }

        }
        public override bool isSelfDone
        {
            get
            {
                if (request != null)
                {
                    return request.isDone;
                }
                return isError;
            }
        }

        public override float progress
        {
            get
            {
                int maxProgressCount = 1;
                float currentProgress = 0;
                if (allReferences != null && allReferences.Length > 0)
                {
                    for (int i = 0; i < allReferences.Length; i++)
                    {
                        var dep = AssetBundleCacheManager.I.GetAssetBundleCache(allReferences[i]);
                        if (dep != null)
                        {
                            maxProgressCount++;
                            currentProgress += dep.request.progress;
                        }
                    }
                }
                if (request != null)
                {
                    currentProgress += request.progress;
                }
                else if(isError)
                {
                    currentProgress++;
                }
                    
                return currentProgress / maxProgressCount;
            }

        }

        public override int priority
        {
            get
            {
                return request.priority;
            }
            set
            {
                if (allReferences != null && allReferences.Length > 0)
                {
                    for (int i = 0; i < allReferences.Length; i++)
                    {
                        var dep = AssetBundleCacheManager.I.GetAssetBundleCache(allReferences[i]);
                        if (dep != null)
                            dep.priority = value;
                    }
                }
                request.priority = value;
            }
        }

        public override bool allowSceneActivation
        {
            get
            {
                return request.allowSceneActivation;
            }
            set
            {
                request.allowSceneActivation = value;
            }
        }
        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddRefCount(int count = 1)
        {
            refCount+= count;

        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void RemoveRefCount(int count = 1)
        {
            refCount = Mathf.Max(0, refCount - count);
        }



        /// <summary>
        /// 尝试卸载Bundle
        /// </summary>
        public bool Unload()
        {
            bool canRelease = CanRelease();
            for (int i = 0; i < allReferences.Length; i++)
            {
                var tmp = AssetBundleCacheManager.I.GetAssetBundleCache(allReferences[i]);
                if (tmp != null)
                {
                    tmp.RemoveRefCount();
                    tmp.Unload();

                }
            }
            if (canRelease)
            {
                AssetBundleCacheManager.I.RemmoveAssetBundleCache(assetBundlePath);
                request?.assetBundle?.Unload(true);
                request = null;
                //UnityEngine.Debug.LogError(assetBundlePath + " release");      
                return true;
            }
            //else
            //{
            //    Debug.LogError(assetBundlePath + " ref count : "+refCount);
            //}
            return false;
        }
        /// <summary>
        /// 强制卸载Bundle
        /// </summary>
        public void ForceUnload()
        {
            refCount = 0;
            for (int i = 0; i < allReferences.Length; i++)
            {
                var tmp = AssetBundleCacheManager.I.GetAssetBundleCache(allReferences[i]);
                if (tmp != null)
                {
                    tmp.ForceUnload();
                }
            }
            AssetBundleCacheManager.I.RemmoveAssetBundleCache(assetBundlePath);
            request?.assetBundle?.Unload(true);
            //Debug.LogError(assetBundlePath + " release");
        }
        /// <summary>
        /// 是否可释放
        /// </summary>
        /// <returns></returns>
        public bool CanRelease()
        {
            return refCount == 0;
        }

        public UnityEngine.Object Load(string assetname)
        {
            AddRefCount();
            if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
            {
                return null;
            }
            return GetABSubAsset(assetname);
        }

        public UnityEngine.Object Load(string assetname, System.Type systemTypeInstance)
        {
            AddRefCount();
            if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
            {
                return null;
            }
            return GetABSubAsset(assetname, systemTypeInstance);
        }

        public T Load<T>(string assetname) where T : UnityEngine.Object
        {
            AddRefCount();
            if(assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
            {
                return null;                    
            }
            return GetABSubAsset(assetname,typeof(T)) as T;
        }

        public T[] LoadAll<T>() where T : UnityEngine.Object
        {
            AddRefCount();
            if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
            {
                return null;
            }
            return assetBundle.LoadAllAssets<T>();
        }

        public UnityEngine.Object[] LoadAll(System.Type systemTypeInstance)
        {
            AddRefCount();
            if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
            {
                return null;
            }
            return assetBundle.LoadAllAssets(systemTypeInstance);
        }

        public UnityEngine.Object[] LoadAll()
        {
            AddRefCount();

            return assetBundle.LoadAllAssets();
        }

        public LoadRequestBase LoadAsync<T>(string assetname, System.Action<UnityEngine.Object> completed) where T : UnityEngine.Object
        {
            AddRefCount();
            if (completed != null)
                completedActions.Add(() => {
                    if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
                    {
                        completed(null);
                    }else
                    {
                        completed(GetABSubAsset(assetname,typeof(T)));
                    }
                    });
            return this;
        }

        public LoadRequestBase LoadAsync(string assetname, System.Action<UnityEngine.Object> completed)
        {
            AddRefCount();
            if (completed != null)
                completedActions.Add(() => {
                    if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
                    {
                        completed(null);
                    }
                    else
                    {
                        completed(GetABSubAsset(assetname));
                    }
                    });
            return this;
        }

        public LoadRequestBase LoadAsync(string assetname, System.Type type, System.Action<UnityEngine.Object> completed)
        {
            AddRefCount();
            if (completed != null)
                completedActions.Add(() => {
                    if (assetBundle == null || assetBundle.isStreamedSceneAssetBundle)
                    {
                        completed(null);
                    }
                    else
                    {
                        completed(GetABSubAsset(assetname, type));
                    }
                });
            return this;
        }
        public const char AB_FILE_SPLIT = '/';
        public UnityEngine.Object GetABSubAsset(string assetName,System.Type type=null)
        {
            string name = null;
            if (string.IsNullOrEmpty(assetName))
            {
            }
            else
            {
                name = assetName.GetSplitLast(AB_FILE_SPLIT);
            }
            AssetBundle ab = assetBundle;
            if(ab == null)
            {
                return null;
            }
            var arr = ab.GetAllAssetNames();
            if (arr.Length == 0)
            {
                return null;
            }
            if (string.IsNullOrEmpty(assetName))
            {
                assetName = arr[0].GetSplitFirst('.');
                name = assetName.GetSplitLast(AB_FILE_SPLIT);
            }
            try
            {
                UnityEngine.Object obj = null;
                if (type != null)
                {
                    obj = ab.LoadAsset(name, type);
                }else
                {
                    obj = ab.LoadAsset(name);
                }
                if(ab == null)
                {
                    UnityEngine.Debug.LogError(assetBundlePath + " ->load null-> " + name);
                }

                return obj;
            }
            catch (Exception ex)
            {
                Uqee.Debug.LogError(ex);
            }
            return null;
        }
    }
}