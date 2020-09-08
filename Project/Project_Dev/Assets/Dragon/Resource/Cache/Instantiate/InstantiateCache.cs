
using PathologicalGames;
using System.Collections.Generic;
using UnityEngine;
using Uqee.Pool;

namespace Uqee.Resource
{
    /// <summary>
    /// 对象缓存池(PathologicalGames.PoolManager)。缓存实例化对象
    /// </summary>
    public class InstantiateCache : Singleton<InstantiateCache>, IInstantiateCache
    {
        public Transform tran_pool
        {
            get; private set;
        }
        public Transform tran_tmp
        {
            get; private set;
        }
        protected override void Init()
        {
            tran_pool = ResourceGameObjectCreator.I.GetOrCreate("tran_pool").transform;
            tran_pool.gameObject.SetActive(false);
            tran_tmp = ResourceGameObjectCreator.I.GetOrCreate("tran_tmp").transform;
            tran_tmp.gameObject.SetActive(false);
        }
        public override void Dispose()
        {
            tran_pool = null;
            tran_tmp = null;
            base.Dispose();
        }
        public void RemoveAll()
        {
            tran_pool?.RemoveAllChildren();
            tran_tmp?.RemoveAllChildren();
        }

        private Dictionary<string, InstantiateCacheCfg> _poolCfgDict = new Dictionary<string, InstantiateCacheCfg>();

        private string _FixResName(string name)
        {
            return name.GetSplitLast('/');// name.Contains("/") ? name.Substring(name.LastIndexOf('/') + 1) : name;
        }
        public bool IsInPool(string category, string name)
        {
            if (!_poolCfgDict.ContainsKey(category) || string.IsNullOrEmpty(name))
            {
                return false;
            }
            name = _FixResName(name);
            return _poolCfgDict[category].prefabNameDict.ContainsKey(name);
        }
        public bool CanSpawn(string category, string name)
        {
            if (!_poolCfgDict.ContainsKey(category) || string.IsNullOrEmpty(name))
            {
                return false;
            }
            name = _FixResName(name);
            return _poolCfgDict[category].pool.prefabs.ContainsKey(name);
        }

        public void AddInstantiatePool(string category, GameObject prefabObj)
        {
            if (prefabObj == null)
            {
                Uqee.Debug.LogWarning("[AddPrefabPool]prefabObj is null.");
                return;
            }
            var prefabName = prefabObj.name;
            if (CanSpawn(category, prefabName))
            {
                return;
            }
            prefabObj = UnityEngine.Object.Instantiate(prefabObj, tran_tmp, false);
            prefabObj.name = prefabName;
            //prefabObj.transform.SetParent(tran_tmp, false);
            prefabObj.SetActive(true);
            AddPool(category, prefabName);
            _AddPrefabPool(category, prefabObj);
        }

        private void _AddPrefabPool(string category, GameObject prefabObj)
        {
            var prefabPool = new PrefabPool(prefabObj.transform);
            //prefabPool.limitInstances = true;
            //prefabPool.limitAmount = 20;
            //prefabPool.limitFIFO = true;
            prefabPool.cullDelay = 15;
            prefabPool.cullAbove = 1;
            prefabPool.cullDespawned = true;
            prefabPool.cullMaxPerPass = 5;
            prefabPool.preloadAmount = 0;
            _AddAutoDespawn(prefabObj.transform, category, prefabObj.name);
            _poolCfgDict[category].pool.CreatePrefabPool(prefabPool);
        }
        public void ReleaseCache(bool all)
        {
            if (!all) return;
            for (int i = tran_tmp.childCount - 1; i >= 0; i--)
            {
                var go = tran_tmp.GetChild(i).gameObject;
                if (!_autoDespawnDict.ContainsKey(go.GetInstanceID()))
                {
                    //var gpuSkin = go.GetComponent<GPUSkinningPlayerMono>();
                    //if (gpuSkin != null)
                    //{
                    //    gpuSkin.Cleanup();
                    //}
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
            foreach (var cfg in _poolCfgDict)
            {
                cfg.Value.pool.ClearPrefabs(_DestroyPoolObj);
            }
        }
        public UnityEngine.Object GetFromCache(string category, string name)
        {
            return null;
        }
        public Transform Spawn(string category, string name, Transform parent)
        {
            name = _FixResName(name);
            if (IsInPool(category, name))
            {
                var pool = _poolCfgDict[category].pool;
                if (!pool.prefabs.ContainsKey(name))
                {
                    return null;
                }
                if (parent == null)
                {
                    Uqee.Debug.LogWarning("[ResManager]Spawn(). parent==null??");
                    parent = tran_tmp;
                }
                var inst = pool.Spawn(name, parent);
                inst.name = name;
                var poolable = _AddAutoDespawn(inst, category, name);


                var poolMonoList = DataListFactory<PoolableMono>.Get();
                inst.GetComponents(poolMonoList);
                for (var i = 0; i < poolMonoList.Count; i++)
                {
                    poolMonoList[i].OnSpawn();
                }
                DataListFactory<PoolableMono>.Release(poolMonoList);

                AssetBundleUtils.FixObj(inst);
                return inst;
            }
            else
            {
                return null;
            }
        }
        private Dictionary<int, AutoDespawnMono> _autoDespawnDict = new Dictionary<int, AutoDespawnMono>();
        private AutoDespawnMono _AddAutoDespawn(Transform inst, string category, string name)
        {
            var poolable = inst.GetComponent<AutoDespawnMono>();
            if (poolable == null)
            {
                poolable = inst.gameObject.AddComponent<AutoDespawnMono>();
                poolable.prefabName = name;
                poolable.category = category;
            }
            _autoDespawnDict[inst.gameObject.GetInstanceID()] = poolable;
            return poolable;
        }

        public void _DestroyPoolObj(GameObject go)
        {
            _autoDespawnDict.Remove(go.GetInstanceID());
            UnityEngine.Object.DestroyImmediate(go);
        }
        /// <summary>
        /// 将子节点中可回收的节点放到缓存池里，不可回收的不处理。
        /// 需要删除不可回收的子节点，使用 parent.RemoveAllChildren()
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        public void DespawnChildren(Transform parent, bool worldPositionStays = false)
        {
            if (parent == null) return;
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);

                DespawnChildren(child, worldPositionStays);
                _Despawn(child.gameObject, worldPositionStays);
            }
        }

        public bool Despawn(GameObject go, bool worldPositionStays = false)
        {
            if (go == null)
                return false;

            DespawnChildren(go.transform, worldPositionStays);
            if (!_Despawn(go, worldPositionStays))
            {
                go.transform.SetParent(tran_tmp);
                UnityEngine.Object.Destroy(go);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 回收实例
        /// </summary>
        /// <param name="category"></param>
        /// <param name="go"></param>
        /// <param name="prefabName">空值取go.name</param>
        /// <returns></returns>
        private bool _Despawn(GameObject go, bool worldPositionStays = false)
        {
            if (go == null)
                return false;

            AutoDespawnMono poolable = null;
            int instId = go.GetInstanceID();
            _autoDespawnDict.TryGetValue(instId, out poolable);

            if (poolable == null)
            {
                return false;
            }

            var prefabName = poolable.prefabName;
            go.name = prefabName;
            if (IsInPool(poolable.category, prefabName))
            {
                if (go.transform.parent == tran_pool)
                {
                    //Uqee.Debug.LogWarning("重复回收了？？" + prefabName);
                    return true;
                }
                go.transform.SetParent(tran_pool, worldPositionStays);
                _poolCfgDict[poolable.category].pool.Despawn(prefabName, go.transform);
                if (_autoDespawnDict.ContainsKey(instId))
                {
                    var poolMonoList = DataListFactory<PoolableMono>.Get();
                    go.GetComponentsInChildren(poolMonoList);
                    for (var i = 0; i < poolMonoList.Count; i++)
                    {
                        poolMonoList[i].OnDespawn();
                    }
                    DataListFactory<PoolableMono>.Release(poolMonoList);
                }
                return true;
            }
            else
            {
                _autoDespawnDict.Remove(instId);
            }
            return false;
        }

        public void AddPool(string category, string assetName)
        {
            if (tran_pool == null)
            {
                return;
            }
            InstantiateCacheCfg poolCfg = _poolCfgDict.ContainsKey(category) ? _poolCfgDict[category] : new InstantiateCacheCfg();
            if (poolCfg.pool == null)
            {
                poolCfg.prefabNameDict = new Dictionary<string, bool>();
                poolCfg.pool = PoolManager.Pools.Create(category.ToString(), ResourceGameObjectCreator.I.gameObject);
                poolCfg.pool.destroyDelegates = this._DestroyPoolObj;
                _poolCfgDict[category] = poolCfg;
            }
            poolCfg.prefabNameDict[assetName] = true;
        }
    }
}
namespace PathologicalGames
{
    public partial class SpawnPool
    {
        public void ClearPrefabs(System.Action<GameObject> destroyAction)
        {
            for (int i = _prefabPools.Count - 1; i >= 0; i--)
            {
                if (_prefabPools[i].Clear(destroyAction))
                {
                    prefabs._Remove(_prefabPools[i].name);
                    _prefabPools.RemoveAt(i);
                }
            }
        }
    }

    public partial class PrefabPool
    {

        public bool Clear(System.Action<GameObject> destroyAction)
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (_spawned[i] == null)
                {
                    _spawned.RemoveAt(i);
                }
            }

            var destroy = true;

            if (Uqee.Resource.CacheSetting.IsDonotRelease(name))
            {
                destroy = false;
            }
            if (spawned.Count == 0 && destroy)
            {
                if (prefabGO != null)
                {
                    destroyAction(prefabGO);
                }
                SelfDestruct();
                return true;
            }
            else
            {
                var cullingLeft = 5;
                if (destroy)
                {
                    Uqee.Debug.LogWarning("SpawnPool can't clear.Spawned has reference:" + name);
                    cullingLeft = 0;
                }
                for (int i = this._despawned.Count - 1; i >= cullingLeft; i--)
                {
                    if (this._despawned[i] != null && this._despawned[i].gameObject != null)
                    {
                        this.spawnPool.DestroyInstance(this._despawned[i].gameObject);
                    }
                    this._despawned.RemoveAt(i);
                };
                return false;
            }
        }
    }
}