using PathologicalGames;
using System.Collections.Generic;
using UnityEngine;

public struct CacheData
{
    public Dictionary<string, bool> prefabNameDict;
    public SpawnPool pool;
}

public class CacheInit
{
    public static Dictionary<string, CacheData> _poolCfgDict = new Dictionary<string, CacheData>();
    public static GameObject gameObject;
    public static void AddPool(string category, string assetName)
    {
        CacheData cacheData = _poolCfgDict.ContainsKey(category) ? _poolCfgDict[category] : new CacheData();
        if (cacheData.pool == null)
        {
            cacheData.prefabNameDict = new Dictionary<string, bool>();
            cacheData.pool = PoolManager.Pools.Create(category, gameObject);
            _poolCfgDict[category] = cacheData;
        }
        cacheData.prefabNameDict[category] = true;
    }
}