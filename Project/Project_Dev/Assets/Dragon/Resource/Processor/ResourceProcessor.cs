using System;
using System.Collections.Generic;

namespace Uqee.Resource
{
    public class ResourceProcessor : AbstractResourceProcessor<ResourceProcessor>
    {
        /// <summary>
        /// 加载资源适配器
        /// </summary>
        public ILoadAdapter loadAdapter;
        /// <summary>
        /// 加载依赖适配器
        /// </summary>
        public IDependenciesAdapter depsAdapter;
        private volatile int _loadAssetsCount = 0;
        private List<AssetRequest> _reqLoadingList = new List<AssetRequest>();
        private List<AssetRequest> _reqAssetsList = new List<AssetRequest>();
        private List<AssetRequest> _reqAssetsSilentList = new List<AssetRequest>();

        private int MAX_LOAD_COUNT = 10;

        public override bool IsFree()
        {
            return _reqAssetsList.Count == 0;
        }
        public override bool IsWorking()
        {
            return _loadAssetsCount > 0;
        }

        public override void SetFastMode(bool val)
        {
            base.SetFastMode(val);
            if (val)
            {
                MAX_LOAD_COUNT = 20;
            }
            else
            {
                MAX_LOAD_COUNT = 10;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            loadAdapter = null;
            depsAdapter = null;
        }

        public override void ClearRequest()
        {
            _ClearReqList(_reqAssetsList);
            _ClearReqList(_reqAssetsSilentList);

            _ClearLoadingReqList();
        }
        private void _ClearLoadingReqList()
        {
            AssetRequest req;
            for (var i = _reqLoadingList.Count - 1; i >= 0; i--)
            {
                req = _reqLoadingList[i];
                if (req.silent || !req.donotClear)
                {
                    if (req.corObj == null || (req.isCompleted && !req.isDepsCompleted))
                    {
                        RequestPool.PushToPool(req);
                        _reqLoadingList.RemoveAt(i);
                        _loadAssetsCount--;
                    }
                }
            }
        }
        private void _ClearReqList(List<AssetRequest> list)
        {
            AssetRequest req;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                req = list[i];
                if (req.silent || !req.donotClear)
                {
                    RequestPool.PushToPool(req);
                    list.RemoveAt(i);
                }
            }
        }
        public void AddAssetsRequest(AssetRequest req)
        {
            var asset = CacheManager.I.GetCache<IAssetsCache>()?.GetObject(req.assetPath, req.assetName);
            if (asset != null)
            {
                req.loadedObj = asset;
                //if (req.isSystemAssets)
                //{
                //    assetsCache.SetCacheIsSystemAssets(req.category, req.assetName, true);
                //}
                req.InvokeComplete();
                return;
            }
            if (!_AddSameRequest(req))
            {
                depsAdapter?.CheckDepsAsync(req);
                AssetRequest.GetCallCount(req);
                req.callCount++;
                req.lastCallTime = AppStatus.realtimeSinceStartup;


                if (req.silent)
                {
                    _reqAssetsSilentList.Add(req);
                }
                else
                {
                    _reqAssetsList.Add(req);
                }
            }
        }


        private AssetRequest _FindReq(List<AssetRequest> list, AssetRequest req)
        {
            foreach(var tmp in list)
            {
                if(req.category==tmp.category && req.assetName==tmp.assetName)
                {
                    return tmp;
                }
            }
            return null;
        }

        /// <summary>
        /// 合并相同请求
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private bool _AddSameRequest(AssetRequest req)
        {
            AssetRequest found;

            found = _FindReq(_reqAssetsList, req);
            if (found == null)
            {
                found = _FindReq(_reqLoadingList, req);
            }
            if (found == null)
            {
                found = _FindReq(_reqAssetsSilentList, req);
                if (found != null && !req.silent)
                {
                    _reqAssetsList.Add(found);
                    _reqAssetsSilentList.Remove(found);
                }
            }
            if (found != null)
            {
                if (found.loadedObj != null)
                {
                    //已加载好的可能是要进行回收了，不放到sameRequestQueue里
                    return false;
                }
                if (req.isSystemAssets)
                {
                    found.isSystemAssets = true;
                }
                found.callCount++;
                found.lastCallTime = AppStatus.realtimeSinceStartup;
                lock (found.sameRequestQueue)
                {
                    found.sameRequestQueue.Add(req);
                }
                return true;
            }
            return false;
        }
        public override void Update()
        {
            while (_loadAssetsCount < MAX_LOAD_COUNT)
            {
                //var t = TimeUtils.GetMilliseconds();
                var tmpReq = _GetAssetsRequest(); //t < _reqTime ? null : _GetAssetsRequest();
                if (tmpReq != null)
                {
                    //_reqTime = t + 50;
                    _loadAssetsCount++;
                    _reqLoadingList.Add(tmpReq);

                    // 多线程会出现一些加载冲突问题
                    tmpReq.SetDepsCompleteCallback(_GetAssetsCor);
                    // tmpReq.SetDepsCompleteCallbackAsync(_GetABAssets);
                }
                else
                {
                    break;
                }
            }
        }
        void _GetAssetsCor(AssetRequest req)
        {
            req.corObj = CoroutineHelper.Start(__GetAssetsCor(req));
            req.SetDepsCompleteCallback(null);
        }
        /// <summary>
        /// 异步获取资源
        /// </summary>
        /// <param name="req"></param>
        private System.Collections.IEnumerator __GetAssetsCor(AssetRequest req)
        {
            yield return loadAdapter.LoadAsync(req);

            try
            {
                if (string.IsNullOrEmpty(req.error))
                {
                    req.InvokeComplete();
                }
                else
                {
                    req.InvokeError();
                }
            }
            catch (Exception ex)
            {
                Uqee.Debug.LogError(ex);
            }
            req.corObj = null;
            _reqLoadingList.Remove(req);
            _loadAssetsCount--;
        }

        private AssetRequest _GetAssetsRequest()
        {
            AssetRequest req = null;
            if (_reqAssetsList.Count > 0)
            {
                req = _reqAssetsList[0];
                _reqAssetsList.RemoveAt(0);
            }
            else if (_reqAssetsSilentList.Count > 0)
            {
                req = _reqAssetsSilentList[0];
                _reqAssetsSilentList.RemoveAt(0);
            }

            return req;
        }


        public T LoadSync<T>(AssetRequest req) where T : UnityEngine.Object
        {
            depsAdapter?.CheckDepsSync(req);
            return loadAdapter.LoadSync<T>(req);
        }
    }
}