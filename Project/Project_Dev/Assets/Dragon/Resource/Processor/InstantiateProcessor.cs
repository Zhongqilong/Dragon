using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;

namespace Uqee.Resource
{
    public class InstantiateProcessor : AbstractResourceProcessor<InstantiateProcessor>
    {
        private List<InstantiateRequest> _reqInstList = new List<InstantiateRequest>();

        private int MAX_INST_COUNT = 1;

        public override void Init()
        {
            base.Init();

        }
        public override bool IsFree()
        {
            return _reqInstList.Count == 0;
        }
        public override void SetFastMode(bool val)
        {
            base.SetFastMode(val);
            if (val)
            {
                MAX_INST_COUNT = 6;
            }
            else
            {
                MAX_INST_COUNT = 3;
            }
        }
        public override void Dispose()
        {
        }
        public override void ClearRequest()
        {
            InstantiateRequest req;
            for (var i = _reqInstList.Count - 1; i >= 0; i--)
            {
                req = _reqInstList[i];
                if (!req.donotClear)
                {
                    RequestPool.PushToPool(req);
                    _reqInstList.RemoveAt(i);
                }
            }
        }
        public override void Update()
        {
            var len = Math.Min(_reqInstList.Count, MAX_INST_COUNT);
            for (int i = 0; i < len; i++)
            {
                var _tmpInstReq = _reqInstList[0];
                _reqInstList.RemoveAt(0);
                _DoInst(_tmpInstReq);
            }
        }

        public void AddInstRequest(InstantiateRequest req)
        {
            _reqInstList.Add(req);
        }
        private void _DoInst(InstantiateRequest req)
        {
            if (req.parent == null)
            {
                // Uqee.Debug.LogWarning("Instantiate fail. asset ({0}) loaded success, but the parent has been destroyed.", req.assetName);
                return;
            }
            var instCache = CacheManager.I.GetCache<IInstantiateCache>();
            var inst = instCache?.Spawn(req.category, req.assetName, req.parent);
            if (inst == null)
            {
                UnityEngine.Object obj = null;
#if NEW_ASSET_BUNDLE
                if (req.loadedObj != null)
                {
                    obj = req.loadedObj;
                }else
                {
                    obj = CacheManager.I.GetCache<IAssetsCache>()?.GetObject(req.assetPath, req.assetName);
                }
#else
                obj = CacheManager.I.GetCache<IAssetsCache>()?.GetObject(req.assetPath, req.assetName);
#endif
                if (obj == null)
                {
                    Uqee.Debug.LogWarning($"[Instantiate] failed. cache not found. category={req.category.ToString()}, assetName={req.assetName}");
                    req.InvokeError();
                    return;
                }
                req.loadedObj = UnityEngine.Object.Instantiate(obj, req.parent);
                AssetBundleUtils.FixObj(req.loadedObj);
            }
            else
            {
                req.loadedObj = inst.gameObject;
            }
            ResourcesManager.I.AddInstantiateObj(req.loadedObj, req.assetPath);
            req.loadedObj.name = req.prefabName;
            req.InvokeComplete();
        }
    }
}