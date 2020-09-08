using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UDragCardScrollView : EventTrigger
{
    private CardScrollView enhanceScrollView;
    public void SetScrollView(CardScrollView view)
    {
        enhanceScrollView = view;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewBegin(eventData.delta);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewMove(eventData.delta);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (enhanceScrollView != null)
            enhanceScrollView.OnDragEnhanceViewEnd();
    }
}
