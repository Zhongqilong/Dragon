using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using Uqee.Resource;
using UnityEngine.U2D;

public class ResManagerUtilEditor
{
    [MenuItem("ResManager/UnloadUnusedAssets")]
    static void ForceUnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }

    [MenuItem("ResManager/LogAssetBundleCache")]
    static void LogAssetBundleCache()
    {
        LogAssetBundleCache1(false);
    }
    [MenuItem("ResManager/LogAssetBundleCacheDeps")]
    static void LogAssetBundleCacheDeps()
    {
        LogAssetBundleCache1(true);
    }
    [MenuItem("ResManager/StartAssetBundleSnapshot")]
    static void StartAssetBundleSnapshot()
    {
        startSnapshot = true;
        lastAssetBundleSnapshot.Clear();
        Debug.LogError("Start Snapshot ");

    }
    [MenuItem("ResManager/ClearAssetBundleSnapshot")]
    static void ClearAssetBundleSnapshot()
    {
        Debug.LogError("Clear Snapshot ");
        startSnapshot = false;
        lastAssetBundleSnapshot.Clear();
    }
    public static List<string> lastAssetBundleSnapshot = new List<string>();
    public static bool startSnapshot = false;
    static void LogAssetBundleCache1(bool isShowDep = false)
    {
        bool endSnapshot =  lastAssetBundleSnapshot.Count != 0;
        StringBuilder stringBuilder = new StringBuilder();
        int count = 0;
        foreach (var item in AssetBundleCacheManager.I.assetBundles)
        {
            if(item.Value.assetBundle != null)
            {
                count++;
            }
        }
        var keys = AssetBundleCacheManager.I.assetBundles.Keys.ToList().OrderBy(u=>u);


        

        stringBuilder.AppendLine("Total " + count);
        int lineCount = 1;
        foreach (var item in keys)
        {
            if(startSnapshot)
            {
                if (endSnapshot)
                {
                    if (lastAssetBundleSnapshot.Contains(item))
                        lastAssetBundleSnapshot.Remove(item);
                    else
                        lastAssetBundleSnapshot.Add(item);
                }
                else
                {
                    lastAssetBundleSnapshot.Add(item);
                }
            }
            
            var tmp = AssetBundleCacheManager.I.assetBundles[item];
            stringBuilder.Append(tmp.assetBundlePath);
            if(tmp.assetBundle == null)
            {
                stringBuilder.Append(" missing  ");
                stringBuilder.Append(tmp.realPath);
            }
            if(isShowDep )
            {
                stringBuilder.Append("  -> ref count : ");
                stringBuilder.Append(tmp.refCount.ToString());
                stringBuilder.AppendLine();
                lineCount++;
                
            }else
            {
                stringBuilder.Append(" \n");
                lineCount++;
            }
            stringBuilder.AppendLine(string.Empty);
            lineCount++;
            if (lineCount > 500)
            {
                Debug.LogError(stringBuilder.ToString());
                stringBuilder.Remove(0, stringBuilder.Length);
                lineCount = 0;
            }
        }


        Debug.LogError(stringBuilder.ToString());
        stringBuilder.Clear();
        if(startSnapshot)
        {
            if (endSnapshot && lastAssetBundleSnapshot.Count > 0)
            {
                stringBuilder.AppendFormat("Snapshot : {0}\n", lastAssetBundleSnapshot.Count);
                lastAssetBundleSnapshot.ForEach((item) => { stringBuilder.Append("\""); stringBuilder.Append(item); stringBuilder.AppendLine("\","); });
                Debug.LogError(stringBuilder.ToString());
                ClearAssetBundleSnapshot();
            }
            else
            {
                Debug.LogError("Start Snapshot :" + lastAssetBundleSnapshot.Count);
            }
        }
        
    }

    [MenuItem("ResManager/LogUnityAllAsetBundles")]
    static void LogAllAsetBundles()
    {
        StringBuilder stringBuilder = new StringBuilder();
        var bundles = AssetBundle.GetAllLoadedAssetBundles();

        foreach (var item in bundles)
        {
            stringBuilder.AppendLine(item.ToString());
        }
        Debug.LogError(stringBuilder.ToString());
    }
    [MenuItem("ResManager/UnloadAllAsetBundles")]
    static void UnloadAllAsetBundles()
    {
        var keys = AssetBundleCacheManager.I.assetBundles.Keys.ToList().OrderBy(u => u).ToList();
        foreach (var item in keys)
        {
            ResourcesManager.I.ForceUnloadAsset(item);
        }
    }
    [MenuItem("ResManager/ShowRefSpriteAtlas")]
    static void ShowRefSpriteAtlas()
    {
        if (Selection.activeGameObject == null)
            return;
        var gameObject = Selection.activeGameObject;
        List<SpriteAtlas> allAtlas = new List<SpriteAtlas>(AssetBundleCacheManager.I.allAtlas);
        var images = gameObject.GetComponentsInChildren<UnityEngine.UI.Image>();
        List<UnityEngine.Sprite> allSprites = new List<Sprite>(64);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].sprite != null)
            {
                if (!allSprites.Contains(images[i].sprite))
                {
                    allSprites.Add(images[i].sprite);
                }
            }
        }
        images = null;

        while (allSprites.Count > 0)
        {
            var sprite = allSprites[0];
            allSprites.RemoveAt(0);
            for (int n = 0; n < allAtlas.Count; n++)
            {
                if (allAtlas[n].CanBindTo(sprite))
                {
                    string assetName = allAtlas[n].name;
                    string category = ResourceDefineManager.I.atlasDict[assetName];
                    Debug.LogError( ResourcesManager.I.GetFixAssetPath(category, assetName));
                    allAtlas.RemoveAt(n);
                    break;
                }
            }
        }
}

    [MenuItem("ResManager/一键选中指定Object")]
    static void SelectionObjs()
    {
        string basePath = "Assets/Uqee/";
        List<string> objsPath = new List<string>()
        {
            "Core/Resource/ResourcesManager/AssetBundle/AssetBundleCache.cs",
            "Core/Resource/ResourcesManager/AssetBundle/AssetBundleCacheManager.cs",
            "Core/Resource/ResourcesManager/AssetBundle/AssetBundleLoader.cs",
            "Core/Resource/ResourcesManager/AssetBundle/AssetBundleProcessor.cs",
            "Core/Resource/ResourcesManager/Editor/AssetBundleProcessorEditor.cs",
            "Core/Resource/ResourcesManager/Editor/ResManagerUtilEditor.cs",
            "Core/Resource/ResourcesManager/IAssetLoader.cs",
            "Core/Resource/ResourcesManager/LoaderRequestBase.cs",
            "Core/Resource/ResourcesManager/Res/ResourcesLoader.cs",
            "Core/Resource/ResourcesManager/Res/ResourcesLoaderRequest.cs",
            "Core/Resource/ResourcesManager/ResourcesManager.cs",
            "Core/Resource/Processor/InstantiateProcessor.cs",
            "Core/Scene/StageManager.cs",
            "Extensions/UI/Spine/Spine2D.cs",
            "Logic/World/WorldSceneView.cs",
            "Utility/Manager/Resource/ResManager.cs",
            "Utility/Manager/UI/UIManager.cs",
            "Utility/ResUtils.cs",

        };
        Object[] objs = new Object[objsPath.Count];
        for(int i=0;i< objsPath.Count;i++)
        {
            objs[i] = AssetDatabase.LoadAssetAtPath<Object>(basePath + objsPath[i]);
        }
        Selection.objects = objs;
    }

}
