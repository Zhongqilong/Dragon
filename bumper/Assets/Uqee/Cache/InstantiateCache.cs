using System;
using System.Collections.Generic;
using System.IO;
using PathologicalGames;
using UnityEngine;
using Uqee.Pool;

namespace Uqee.Resource {
    public struct InstantiateCacheCfg {
        public Dictionary<string, bool> prefabNameDict;
        public SpawnPool pool;
    }

    /// <summary>
    /// 对象缓存池(PathologicalGames.PoolManager)。缓存实例化对象
    /// </summary>
    public class InstantiateCache : Singleton<InstantiateCache> {
        private static Dictionary<string, string> CateGoryPath = new Dictionary<string, string> () { { RESOURCE_CATEGORY.UI, "Prefabs/UI/" },
        { RESOURCE_CATEGORY.Game, $"Prefabs/{RESOURCE_CATEGORY.Game}/" }
        };
        public Transform tran_pool {
            get;
            private set;
        }
        public Transform tran_tmp {
            get;
            private set;
        }
        public static Dictionary<string, InstantiateCacheCfg> _poolCfgDict = new Dictionary<string, InstantiateCacheCfg> ();
        public static GameObject gameObject;

        private string _FixResName (string name) {
            return name.GetSplitLast ('/'); // name.Contains("/") ? name.Substring(name.LastIndexOf('/') + 1) : name;
        }

        protected override void Init () {
            tran_pool = ResourceGameObjectCreator.I.GetOrCreate ("tran_pool").transform;
            tran_pool.gameObject.SetActive (false);
            tran_tmp = ResourceGameObjectCreator.I.GetOrCreate ("tran_tmp").transform;
            tran_tmp.gameObject.SetActive (false);
        }

        public static void AddPool (string category, string assetName, GameObject go) {
            InstantiateCacheCfg cacheData = _poolCfgDict.ContainsKey (category) ? _poolCfgDict[category] : new InstantiateCacheCfg ();
            if (cacheData.pool == null) {
                cacheData.prefabNameDict = new Dictionary<string, bool> ();
                cacheData.pool = PoolManager.Pools.Create (category, go);
                _poolCfgDict[category] = cacheData;
            }
            cacheData.prefabNameDict[assetName] = true;
        }

        public void AddPrefabPool(string category, GameObject prefabObj)
        {
            var prefabPool = new PrefabPool(prefabObj.transform);
            prefabPool.cullDelay = 15;
            prefabPool.cullAbove = 1;
            prefabPool.cullDespawned = true;
            prefabPool.cullMaxPerPass = 5;
            prefabPool.preloadAmount = 0;
            _AddAutoDespawn(prefabObj.transform, category, prefabObj.name);
            _poolCfgDict[category].pool.CreatePrefabPool(prefabPool);
        }

        public bool HasThisPrefabPool(string category, string prefabPool)
        {
            if (! hasThisPool(category))
            {
                return false;
            }
            return _poolCfgDict[category].pool.prefabPools.ContainsKey(prefabPool);
        }

        public bool CanSpawn (string category, string name) {
            if (!_poolCfgDict.ContainsKey (category) || string.IsNullOrEmpty (name)) {
                return false;
            }
            name = _FixResName (name);
            return _poolCfgDict[category].pool.prefabs.ContainsKey (name);
        }

        public bool hasThisPool(string category)
        {
            if (! _poolCfgDict.ContainsKey(category))
                return false;

            return _poolCfgDict[category].pool != null;
        }

        private static string _FixAssetName (string assetName) {
            if (!string.IsNullOrEmpty (assetName)) {
                var ext = Path.GetExtension (assetName);
                if (!string.IsNullOrEmpty (ext) && ext != ".lua" && ext != ".proto" && ext != ".dat") {
                    assetName = assetName.Replace (ext, string.Empty);
                }
            }
            return assetName;
        }
        public Transform Spawn (string category, string name, Transform parent) {
            //name = _FixResName (name);
            // if (CanSpawn (category, name)) {
            //     var pool = _poolCfgDict[category].pool;
            //     if (parent == null) {
            //         parent = tran_tmp;
            //     }
            //     var inst = pool.Spawn (name, parent);
            //     inst.name = name;
            //     return inst;
            // }
            // return null;
            if (CanSpawn(category, name))
            {
                var pool = _poolCfgDict[category].pool;
                if (!pool.prefabs.ContainsKey(name))
                {
                    return null;
                }
                if (parent == null)
                {
                    parent = tran_tmp;
                }
                var inst = pool.Spawn(name, parent);
                // var prefabPool = pool.prefabPools[name];
                // var inst = PoolManager.Pools[category].Spawn(prefabPool.prefab);
                inst.name = name;
                var poolable = _AddAutoDespawn(inst, category, name);
                var poolMonoList = DataListFactory<PoolableMono>.Get();
                inst.GetComponents(poolMonoList);
                for (var i = 0; i < poolMonoList.Count; i++)
                {
                    poolMonoList[i].OnSpawn();
                }
                DataListFactory<PoolableMono>.Release(poolMonoList);
                return inst;
            }
            else
            {
                return null;
            }
        }

        //加载资源目录可以自行添加判断
        public static AssetRequest LoadAssets (string category, string assetName, bool isSystemAssets = false, Action<AssetRequest> onComplete = null, bool silent = false) {
            var req = _CreatRequest (category, assetName, isSystemAssets, onComplete, silent);

            if (!_IsRequestValid (req.category, req.assetName)) { return null; }
            switch (category) {
                case RESOURCE_CATEGORY.UI:
                    req.donotClear = true;
                    break;
                default:
                    break;
            }

            return req;
        }

        private static AssetRequest _CreatRequest (string category, string assetName, bool isSystemAssets = false, Action<AssetRequest> onComplete = null, bool silent = false) {
            assetName = _FixAssetName (assetName);
            var req = DataFactory<AssetRequest>.Get ();
            req.category = category;
            req.assetName = assetName;
            req.isSystemAssets = isSystemAssets;
            req.onComplete = onComplete;
            req.onError = onComplete;
            req.silent = silent;
            req.assetPath = GetAssetPath (req.category, req.assetName);

            return req;
        }

        #region 获取资源路径
        public static string GetAssetPath (string category, string assetName, string childPath = "") {
            var assetPath = assetName;
            var config = CateGoryPath[category];
            if (config == null) {
                return "";
            }
            return config + childPath + assetPath;
        }

        private static bool _IsRequestValid (string category, string assetName) {
            return !(category != null && string.IsNullOrEmpty (assetName));
        }

        public override void Dispose () {
            tran_pool = null;
            tran_tmp = null;
            base.Dispose ();
        }

        public void RemoveAll () {
            tran_pool?.DestroyChildren ();
            tran_tmp?.DestroyChildren ();
        }

        private Dictionary<int, AutoDespawnMono> _autoDespawnDict = new Dictionary<int, AutoDespawnMono> ();
        private AutoDespawnMono _AddAutoDespawn (Transform inst, string category, string name) {
            var poolable = inst.GetComponent<AutoDespawnMono> ();
            if (poolable == null) {
                poolable = inst.gameObject.AddComponent<AutoDespawnMono> ();
                poolable.prefabName = name;
                poolable.category = category;
            }
            _autoDespawnDict[inst.gameObject.GetInstanceID ()] = poolable;
            return poolable;
        }

        /// <summary>
        /// 将子节点中可回收的节点放到缓存池里，不可回收的不处理。
        /// 需要删除不可回收的子节点，使用 parent.RemoveAllChildren()
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        public void DespawnChildren (Transform parent, bool worldPositionStays = false) {
            if (parent == null) return;
            for (var i = parent.childCount - 1; i >= 0; i--) {
                var child = parent.GetChild (i);

                DespawnChildren (child, worldPositionStays);
                _Despawn (child.gameObject, worldPositionStays);
            }
        }

        public void DespawnChildren(List<GameObject> trans)
        {
            foreach (var item in trans)
            {
                _Despawn (item);
            }
        }

        public bool IsInPool (string category, string name) {
            if (!_poolCfgDict.ContainsKey (category) || string.IsNullOrEmpty (name)) {
                return false;
            }
            name = _FixResName (name);
            return _poolCfgDict[category].prefabNameDict.ContainsKey (name);
        }

        /// <summary>
        /// 回收实例
        /// </summary>
        /// <param name="category"></param>
        /// <param name="go"></param>
        /// <param name="prefabName">空值取go.name</param>
        /// <returns></returns>
        private bool _Despawn (GameObject go, bool worldPositionStays = false) {
            if (go == null)
                return false;

            AutoDespawnMono poolable = null;
            int instId = go.GetInstanceID ();
            _autoDespawnDict.TryGetValue (instId, out poolable);

            if (poolable == null) {
                return false;
            }

            var prefabName = poolable.prefabName;
            go.name = prefabName;
            if (IsInPool (poolable.category, prefabName)) {
                if (go.transform.parent == tran_pool) {
                    //Uqee.Debug.LogWarning("重复回收了？？" + prefabName);
                    return true;
                }
                go.transform.SetParent (tran_pool, worldPositionStays);
                _poolCfgDict[poolable.category].pool.Despawn (prefabName, go.transform);
                if (_autoDespawnDict.ContainsKey (instId)) {
                    var poolMonoList = DataListFactory<PoolableMono>.Get ();
                    go.GetComponentsInChildren (poolMonoList);
                    for (var i = 0; i < poolMonoList.Count; i++) {
                        poolMonoList[i].OnDespawn ();
                    }
                    DataListFactory<PoolableMono>.Release (poolMonoList);
                }
                return true;
            } else {
                _autoDespawnDict.Remove (instId);
            }
            return false;
        }

        public void ReleaseCache (bool all) {
            if (!all) return;
            for (int i = tran_tmp.childCount - 1; i >= 0; i--) {
                var go = tran_tmp.GetChild (i).gameObject;
                if (!_autoDespawnDict.ContainsKey (go.GetInstanceID ())) {
                    UnityEngine.Object.DestroyImmediate (go);
                }
            }
            foreach (var cfg in _poolCfgDict) {
                cfg.Value.pool.ClearPrefabs (_DestroyPoolObj);
            }
        }

        public void _DestroyPoolObj (GameObject go) {
            _autoDespawnDict.Remove (go.GetInstanceID ());
            UnityEngine.Object.DestroyImmediate (go);
        }

    }
    #endregion
}

namespace PathologicalGames {
    public partial class SpawnPool {
        public void ClearPrefabs (System.Action<GameObject> destroyAction) {
            for (int i = _prefabPools.Count - 1; i >= 0; i--) {
                if (_prefabPools[i].Clear (destroyAction)) {
                    prefabs._Remove (_prefabPools[i].name);
                    _prefabPools.RemoveAt (i);
                }
            }
        }
    }

    public partial class PrefabPool {

        public bool Clear (System.Action<GameObject> destroyAction) {
            for (int i = spawned.Count - 1; i >= 0; i--) {
                if (_spawned[i] == null) {
                    _spawned.RemoveAt (i);
                }
            }

            var destroy = true;

            if (spawned.Count == 0 && destroy) {
                if (prefabGO != null) {
                    destroyAction (prefabGO);
                }
                SelfDestruct ();
                return true;
            } else {
                var cullingLeft = 5;
                if (destroy) {
                    cullingLeft = 0;
                }
                for (int i = this._despawned.Count - 1; i >= cullingLeft; i--) {
                    if (this._despawned[i] != null && this._despawned[i].gameObject != null) {
                        this.spawnPool.DestroyInstance (this._despawned[i].gameObject);
                    }
                    this._despawned.RemoveAt (i);
                };
                return false;
            }
        }
    }
}