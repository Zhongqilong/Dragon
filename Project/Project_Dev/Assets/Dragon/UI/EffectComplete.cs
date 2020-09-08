using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectComplete : MonoBehaviour
{
    public System.Action<Transform> callBack;
    public static EffectComplete AddCompleteCall(Transform transform,System.Action<Transform> callBack)
    {
        var completeCall = transform.GetOrAddComponent<EffectComplete>();
        completeCall.callBack = callBack;
        return completeCall;
    }

    public void OnCompleteCall()
    {
        callBack?.Invoke(transform);
    }

    private void OnDestroy()
    {
        callBack = null;
    }
}