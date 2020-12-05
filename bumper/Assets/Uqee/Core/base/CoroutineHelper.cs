using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour {
    public IEnumerator AutoRun (IEnumerator routine) {
        yield return routine;
        DestroyImmediate (gameObject);
    }
}

public static class CoroutineHelper {
    public static GameObject Start (IEnumerator routine) {
        GameObject go = new GameObject ();
        var runner = go.AddComponent<CoroutineRunner> ();
        runner.StartCoroutine (runner.AutoRun (routine));
        go.hideFlags = HideFlags.HideInHierarchy;
        UnityEngine.Object.DontDestroyOnLoad (go);
        return go;
    }

    public static void Stop (GameObject corObj) {
        if (corObj == null)
            return;

        var runner = corObj.GetComponent<CoroutineRunner> ();
        if (runner != null) {
            runner.StopAllCoroutines ();
        }
        Object.Destroy (corObj);
    }
}