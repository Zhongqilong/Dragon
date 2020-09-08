using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Uqee.Resource
{
    public class AssetBundleProcessor : MonoBehaviour
    {
        private static AssetBundleProcessor instance;
        public static AssetBundleProcessor I
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AssetBundleProcessor>();
                }
                return instance;
            }
        }
        /// <summary>
        /// 当前等待加载的资源列表
        /// </summary>
        public Queue<LoadRequestBase> willLoadRequest = new Queue<LoadRequestBase>(128);
        /// <summary>
        /// 正在加载的资源列表
        /// </summary>
        public List<LoadRequestBase> currentRequests = new List<LoadRequestBase>(64);
        /// <summary>
        /// 一帧内可加载的资源数量
        /// </summary>
        [SerializeField]
        private int willLoadRequestCount;
        /// <summary>
        /// Instantiate物体检查，当所有Instantiate物体销毁后自动卸载对应AB
        /// </summary>
        public Dictionary<string, List<UnityEngine.Object>> allABInstantiateObjs = new Dictionary<string, List<UnityEngine.Object>>();

        private List<string> removeABInstantiateKeys = new List<string>(32);

        public int maxFrame = 150;
        public int currentFrame = 0;
        // Update is called once per frame
        void Update()
        {
            willLoadRequestCount = AssetBundleCacheManager.MAX_THREAD_COUNT;
            for (int i = currentRequests.Count - 1; i >= 0; i--)
            {
                if (currentRequests[i].isSelfDone)
                {
                    if (currentRequests[i].isDone)
                    {
                        currentRequests.RemoveAt(i);
                        willLoadRequestCount--;
                    }
                }
            }

            while (willLoadRequestCount > 0 && willLoadRequest.Count > 0)
            {
                var request = willLoadRequest.Dequeue();
                request.StartLoad();
                if (!request.isDone)
                {
                    currentRequests.Add(request);
                    willLoadRequestCount--;
                }

            }

            currentFrame++;
            if(currentFrame >= maxFrame)
            {
                currentFrame = 0;
                lock(allABInstantiateObjs)
                {
                    foreach (var item in allABInstantiateObjs)
                    {
                        var list = item.Value;
                        int count = list.Count;
                        string assetPath = item.Key;
                        for (int i = count - 1; i >= 0; i--)
                        {
                            if (list[i] == null)
                            {
                                ResourcesManager.I.UnloadAsset(assetPath);
                                list.RemoveAt(i);
                            }
                        }
                        if(list.Count == 0)
                        {
                            //var tmpc= AssetBundleCacheManager.I.GetAssetBundleCache(item.Key);
                            //UnityEngine.Debug.LogError( string.Format("{0} removed , refcount {1}",item.Key, tmpc != null ? tmpc.refCount : -1) );
                            removeABInstantiateKeys.Add(item.Key);
                        }

                    }
                    while (removeABInstantiateKeys.Count > 0)
                    {
                        allABInstantiateObjs.Remove(removeABInstantiateKeys[0]);
                        removeABInstantiateKeys.RemoveAt(0);
                    }
                }

            }

            
        }

        public void UnloadAsset(string path)
        {
            allABInstantiateObjs.Remove(path);
        }

        /// <summary>
        /// 添加资源请求
        /// </summary>
        /// <param name="request"></param>
        public void AddRequest(LoadRequestBase request)
        {
            if (!willLoadRequest.Contains(request))
                willLoadRequest.Enqueue(request);
        }

        public void AddABInstantiateObj(string assetPath,UnityEngine.Object go)
        {
            List<UnityEngine.Object> list = null;
            allABInstantiateObjs.TryGetValue(assetPath, out list);
            if(list == null)
            {
                list = new List<UnityEngine.Object>();
                allABInstantiateObjs.Add(assetPath, list);
            }
            list.Add(go);
        }

        public List<UnityEngine.Object> GetABInstantiateObjs(string assetPath)
        {
            List<UnityEngine.Object> list = null;
            allABInstantiateObjs.TryGetValue(assetPath, out list);
            return list;
        }

        
    }
}