using UnityEngine;
using Uqee.Resource;

public static class GameObjectExtensions
{
    public static T GetOrAddComponent<T>(this MonoBehaviour mono) where T : Component
    {
        return GetOrAddComponent<T>(mono.gameObject);
    }

    public static T GetOrAddComponent<T>(this Transform transform) where T : Component
    {
        var comp = transform.GetComponent<T>();
        if (comp == null)
        {
            comp = transform.gameObject.AddComponent<T>();
        }
        return comp;
    }
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }

    public static void DestroyChildren(this Transform t)
    {
        for (int i= t.childCount -1; i>=0; i--)
        {
            UnityEngine.Object.DestroyImmediate(t.GetChild(i).gameObject);
        }
    }
}
