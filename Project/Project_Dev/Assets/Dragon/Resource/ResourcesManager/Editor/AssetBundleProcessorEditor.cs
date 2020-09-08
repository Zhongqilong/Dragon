using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Uqee.Resource;
using System.Text;
[CustomEditor(typeof(AssetBundleProcessor))]
public class AssetBundleProcessorEditor : Editor
{
    AssetBundleProcessor processor;
    private void OnEnable()
    {
        processor = (AssetBundleProcessor)target;
    }
    string bundlePath = "";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        bundlePath = GUILayout.TextField(bundlePath);
        if(GUILayout.Button("显示持有者"))
        {
            var list = processor.GetABInstantiateObjs(bundlePath);
            if(list != null)
            {
                foreach (var item in list)
                {
                    UnityEngine.Debug.LogError(item, item);
                }

            }
        }
        if (GUILayout.Button("显示持有者数量"))
        {
            foreach (var item in processor.allABInstantiateObjs)
            {
                UnityEngine.Debug.LogError(string.Format("{0} : {1}",item.Key,item.Value.Count));
            }
        }
        if(GUILayout.Button("加载指定ab"))
        {
            Object obj = ResourcesManager.I.Load(bundlePath,null);
            UnityEngine.Debug.LogError(obj);
            if(obj != null)
            {
                Resources.UnloadAsset(obj);
            }
        }
        if (GUILayout.Button("卸载指定ab"))
        {
            ResourcesManager.I.UnloadAsset(bundlePath);
        }
    }

}
