using System;
using System.Collections.Generic;

namespace Uqee.Resource
{
    public class AssetRequest: AbstractResourceRequest<AssetRequest>
    {
        private static Dictionary<string, int> _assetCallDict = new Dictionary<string, int>();

        public static void GetCallCount(AssetRequest req)
        {
            if (req.assetPath == null)
            {
                return;
            }
            if (_assetCallDict.ContainsKey(req.assetPath))
            {
                req.callCount = _assetCallDict[req.assetPath];
            }
        }
        #region AssetBundle
        public string hashStr;
        //AssetBundle 取出来的对象
        public Dictionary<string, UnityEngine.Object> subAssets = new Dictionary<string, UnityEngine.Object>();
        public bool isAB;
        #endregion

        public UnityEngine.GameObject corObj;
        /// <summary>
        /// [加载本地资源]是否系统资源，true:保存时间更久，正常情况下不进行回收
        /// </summary>
        public bool isSystemAssets;
        /// <summary>
        /// 自动回收时，如果引用大于0时，不会进行资源释放
        /// （需要设置的地方管理，默认不进行计数）
        /// </summary>
        public volatile int refCount;
        public volatile int callCount;
        public volatile float cacheTime;
        public volatile float lastCallTime;
        public bool silent;

        //private Action<object> _onDepsCompleteAsync;
        private Action<AssetRequest> _onDepsComplete;

        /// <summary>
        /// [加载本地资源]当请求相同时，合并为一个请求，在完成时按顺序回调
        /// </summary>
        public List<AssetRequest> sameRequestQueue = new List<AssetRequest>();

        public Queue<AssetRequest> depsQueue = new Queue<AssetRequest>();

        override public void Release()
        {
            if (assetPath != null)
            {
                _assetCallDict[assetPath] = callCount;
            }
            if (corObj != null)
            {
                CoroutineHelper.Stop(corObj);
                corObj = null;
            }

            cacheTime = 0;
            silent = false;

            isSystemAssets = false;
            refCount = 0;
            callCount = 0;

            //_onDepsCompleteAsync = null;
            _onDepsComplete = null;

            depsQueue.Clear();
            lock(sameRequestQueue)
            {
                sameRequestQueue.Clear();
            }

            isAB = false;
            hashStr = null;
            subAssets.Clear();

            base.Release();
        }


        protected virtual void _CopyAsset(AssetRequest req)
        {
            req.loadedObj = loadedObj;
        }

        //public void SetDepsCompleteCallbackAsync(Action<object> callback)
        //{
        //    _onDepsCompleteAsync = callback;
        //    if (_onDepsCompleteAsync != null)
        //    {
        //        OnDepsComplete(null);
        //    }
        //}
        public void SetDepsCompleteCallback(Action<AssetRequest> callback)
        {
            _onDepsComplete = callback;
            if (_onDepsComplete != null)
            {
                OnDepsComplete(null);
            }
        }
        public bool isDepsCompleted
        {
            get
            {
                lock (depsQueue)
                {
                    while (depsQueue.Count > 0)
                    {
                        var dep = depsQueue.Peek();
                        if (!dep.isCompleted)
                        {
                            return false;
                        }
                        depsQueue.Dequeue();
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// 依赖的资源加载完成
        /// </summary>
        /// <param name="req"></param>
        public void OnDepsComplete(AssetRequest req)
        {
            if (isDepsCompleted)
            {
                //if (_onDepsCompleteAsync != null)
                //{
                //    Task.Factory.StartNew(_onDepsCompleteAsync, this);
                //}
                _onDepsComplete?.Invoke(this);
            }
        }
        public override void InvokeError()
        {
            isCompleted = true;
            onError?.Invoke(this);

            lock (sameRequestQueue)
            {
                for (int i=0; i< sameRequestQueue.Count; i++)
                {
                    var req = sameRequestQueue[i];
                    req.error = error;
                    req.InvokeError();
                }
                sameRequestQueue.Clear();
            }
            onComplete = null;
            onError = null;
            RequestPool.MarkRelease(this);
        }

        public override void InvokeComplete()
        {
            isCompleted = true;
            onComplete?.Invoke(this);

            lock (sameRequestQueue)
            {
                for (int i = 0; i < sameRequestQueue.Count; i++)
                {
                    var req = sameRequestQueue[i];
                    if (req.loadedObj == null)
                    {
                        _CopyAsset(req);
                    }
                    req.InvokeComplete();
                }
                sameRequestQueue.Clear();
            }
            onComplete = null;
            onError = null;
            if (cacheTime == 0)
            {
                RequestPool.MarkRelease(this);
            }
        }
    }
}