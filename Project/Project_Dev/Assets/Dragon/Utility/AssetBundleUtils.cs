using System;
using UnityEngine;
using Uqee.Resource;

public static class AssetBundleUtils
{
    /// Editor模式下，重新设置shader，不然无法正常显示
    /// </summary>
    /// <param name="mat"></param>
    public static void FixObj(UnityEngine.Object obj)
    {
#if UNITY_EDITOR && ASSET_BUNDLE
        if (obj is Component)
        {
            obj = (obj as Component).gameObject;
        }
        if (obj is GameObject)
        {
            var go = (GameObject)obj;
            _RefindShader(go.transform);
        }
#endif
    }
    /// <summary>
    /// Editor模式下，重新设置shader，不然无法正常显示
    /// </summary>
    /// <param name="mat"></param>
    public static void FixMat(Material mat)
    {
#if UNITY_EDITOR && ASSET_BUNDLE
        _RefindShader(mat);
#endif
    }
    private static void _RefindShader(Transform tran)
    {
        Renderer rd = tran.GetComponent<Renderer>();
        if (rd != null)
        {
            _RefindShader(rd.materials);
            if(rd is ParticleSystemRenderer)
            {
                _RefindShader((rd as ParticleSystemRenderer).trailMaterial);
            }
        }

        UnityEngine.UI.Graphic graphic = tran.GetComponent<UnityEngine.UI.Graphic>();
        if (graphic != null)
        {
            _RefindShader(graphic.material);
        }

        Projector prj = tran.GetComponent<Projector>();
        if (prj != null)
        {
            _RefindShader(prj.material);
        }

        for (int i = 0; i < tran.childCount; i++)
        {
            _RefindShader(tran.GetChild(i));
        }
    }
    private static void _RefindShader(Material[] mArr)
    {
        for (var i = 0; i < mArr.Length; i++)
        {
            mArr[i].shader = Shader.Find(mArr[i].shader.name);
        }
    }
    private static void _RefindShader(Material m)
    {
        if (m != null)
        {
            m.shader = Shader.Find(m.shader.name);
        }
    }


    public const char AB_FILE_SPLIT = '/';
    public static UnityEngine.Object GetABSubAsset(string assetName, AssetRequest abReq)
    {
        if (abReq.loadedObj == null)
        {
            return null;
        }
        UnityEngine.Object obj = null;
        string name = null;
        if (string.IsNullOrEmpty(assetName))
        {
            var keys = abReq.subAssets.Keys;
            if (keys.Count > 0)
            {
                var et = keys.GetEnumerator();
                if (et.MoveNext())
                {
                    name = et.Current;
                }
            }
        }
        else
        {
            name = assetName.GetSplitLast(AB_FILE_SPLIT);
        }
        if (name != null)
        {
            abReq.subAssets.TryGetValue(name, out obj);
        }
        if (obj == null)
        {
            AssetBundle ab = abReq.loadedObj as AssetBundle;
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
                var asset = ab.LoadAsset(name);
                abReq.subAssets[name] = asset;
                _AddToPool(asset as GameObject, abReq.category, name);

                return asset;
            }
            catch (Exception ex)
            {
                Uqee.Debug.LogError(ex);
            }
        }
        return obj;
    }
    private static void _AddToPool(GameObject go, string category, string name)
    {
        var instCache = CacheManager.I.GetCache<IInstantiateCache>();
        if (instCache == null) return;
        if (go != null && instCache.IsInPool(category, name))
        {
            if (!instCache.CanSpawn(category, name))
            {
                go.name = name;
                instCache.AddInstantiatePool(category, go);
            }
        }
    }
}