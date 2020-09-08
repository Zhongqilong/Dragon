using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPositionBinder : MonoBehaviour
{
    private List<Transform> subChildren = new List<Transform>();
    public static void Bind(Transform tfm, Transform fx)
    {
        if (tfm == null)
            return;
        var binder = tfm.GetOrAddComponent<EffectPositionBinder>();
        binder.subChildren.Add(fx);
    }

    private void LateUpdate()
    {
        var _pos = transform.position;
        foreach (var tfm in subChildren)
        {
            tfm.position = _pos;
        }
    }
}