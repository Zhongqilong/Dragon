using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
//希望他的聚合度更高:
//UIEventSender OnClick(System.Action<Transform,int> onClick,subTarget)
public class UIEventSender : MonoBehaviour,IPointerClickHandler
{
    private List<IPointerClickHandler> clickRelay = new List<IPointerClickHandler>();
    internal enum ACTION
    {
        OnClick=0,
        MAX = 1
    }

    public Transform Target { get; private set; }
    public int index;
    private List<Transform> listenerTargets = new List<Transform>();
    private SafeGetList<System.Action<Transform, int>> dict = new SafeGetList<System.Action<Transform, int>>((int)ACTION.MAX);

    static public UIEventSender Get(Transform go,Transform  target ,int index)
    {
        var listener = go.GetOrAddComponent<UIEventSender>();
        listener.Target = target;
        listener.index = index;
        return listener;
    }

    private void _Click()
    {
        dict[(int)ACTION.OnClick]?.Invoke(Target,index);
    }

    public UIEventSender OnClick(System.Action<Transform,int> onClick)
    {
        dict[(int)ACTION.OnClick] = onClick;
        var btn = this.transform.GetComponent<Button>();
        AddToListeners(transform);
        return this;
    }

    public UIEventSender RelayClick(IPointerClickHandler clickHandler)
    {
        if (!clickRelay.Contains(clickHandler))
            clickRelay.Add(clickHandler);
        return this;
    }
    
    private void AddToListeners(Transform target)
    {
        if (target == null)
            return;
        if (!listenerTargets.Contains(target))
        {
            target.GetComponent<Button>().onClick.AddListener(_Click);
            listenerTargets.Add(target);
        }
    }

    private void OnDestroy()
    {
        dict.Clear();
        listenerTargets.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach(var p in clickRelay)
        {
            p?.OnPointerClick(eventData);
        }
    }
}