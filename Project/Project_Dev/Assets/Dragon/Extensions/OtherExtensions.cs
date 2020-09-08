using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class OtherExtensions
{
    /*public static Vector2 WorldToCanvasPos(this Vector2 world)
    {
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.I.canvas.transform as RectTransform, world, UIManager.I.cam_UICam, out position);
        Vector2 referenceResolution = UIManager.I.canvasScale.referenceResolution;
        position.x += referenceResolution.x * 0.5f;
        position.y += referenceResolution.y * 0.5f;
        return position;
    }*/

    public static void Set(this Transform transform, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    public static void Set(this Transform transform, Vector3 pos, Vector3 rot)
    {
        transform.position = pos;
        transform.eulerAngles = rot;
    }

    public static void CopyFrom(this Transform transform, Transform from_transform)
    {
        transform.position = from_transform.position;
        transform.rotation = from_transform.rotation;
    }

    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        foreach (var a in array)
        {
            action(a);
        }
    }
}