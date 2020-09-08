using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CanvasGroupToggle : MonoBehaviour
{
    private List<CanvasGroup> groups = new List<CanvasGroup>();
    public UnityEngine.Events.UnityEvent<float> alphaChanged = new AlphaChanged();
    private void Awake()
    {
        this.GetComponentsInParent<CanvasGroup>(false,groups);
    }

    private void OnDestroy()
    {
        alphaChanged?.RemoveAllListeners();
    }

    public static CanvasGroupToggle AddAlphaChanged(MonoBehaviour mono,UnityEngine.Events.UnityAction<float> update)
    {
        var groupToggle = mono.GetOrAddComponent<CanvasGroupToggle>();
        groupToggle.alphaChanged.AddListener(update);
        update(groupToggle.alpha);
        return groupToggle;
    }

    public float alpha {
        get {
            var alpha = 1f;
            foreach(var g in groups)
            {
                alpha *= g.alpha;
            }
            return alpha;
        }
    }

    private void OnCanvasGroupChanged()
    {
        alphaChanged.Invoke(alpha);
    }

    [XLua.LuaCallCSharp]
    public class AlphaChanged : UnityEngine.Events.UnityEvent<float>
    {

    }
}