using System.Runtime.InteropServices;
using UnityEngine;
using Uqee.Pool;

public class EffectScale : MonoBehaviour {
    public float scale = 1f;
    private bool includeRenderer;
    public static EffectScale AddScale(Transform transform, float scale,bool includeRenderer = false)
    {
        var fxScale = transform.GetOrAddComponent<EffectScale>();
        if(scale != 0&& fxScale.scale != scale)
        {
            ScaleEffect(transform, scale / fxScale.scale, includeRenderer);
            fxScale.scale = scale;
            fxScale.includeRenderer = includeRenderer;
        }
        return fxScale;
    }

    private static void ScaleEffect(Transform tfm,float scale, bool includeRenderer = false)
    {
        var pList = DataListFactory< ParticleSystem>.Get();
        var gObjList = DataListFactory<GameObject>.Get();
        tfm.GetComponentsInChildren<ParticleSystem>(true,pList);
        
        foreach(var p in pList)
        {
            //非常奇怪的代码 ,行为像local return;
            var startModule = p.sizeOverLifetime;
            startModule.enabled = true;
            gObjList.Add(p.gameObject);
            startModule.sizeMultiplier = startModule.sizeMultiplier * scale;
        }

        if (includeRenderer)
        {
            var rList = DataListFactory<Renderer>.Get();
            tfm.GetComponentsInChildren<Renderer>(rList);
            foreach (var r in rList)
            {
                if (!gObjList.Contains(r.gameObject))
                {
                    r.transform.localScale *= scale;
                }
            }

            DataListFactory<Renderer>.Release(rList);
        }
        
        DataListFactory<ParticleSystem>.Release(pList);
        DataListFactory<GameObject>.Release(gObjList);
    }
}