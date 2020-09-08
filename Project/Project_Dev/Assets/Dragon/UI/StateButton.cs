using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StateButton : Button,IDragHandler
{
    public ButtonStageChangedEvent onStageChanged { get; set; }
    public ButtonMoveEvent onMoveEvent { get; set; }
    private List<int> downPoints = new List<int>(1);
    private Vector2 drag_vec2 = Vector2.zero;
    [SerializeField]
    private bool exitAsUp = false;
    protected override void Awake()
    {
        onStageChanged = new ButtonStageChangedEvent();
        onMoveEvent = new ButtonMoveEvent();
        base.Awake();
    }

    public bool IsSelected => downPoints.Count > 0;

    internal void CancelTouch()
    {
        if (downPoints.Count > 0)
        {
            onStageChanged.Invoke(this, false);
        }
        downPoints.Clear();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (downPoints.Count > 0)
        {
            drag_vec2 += eventData.delta;
            onMoveEvent.Invoke(this, drag_vec2);
        }
    }

    override public void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (downPoints.Count == 1)
            return;
        downPoints.Add(eventData.pointerId);
        drag_vec2 = Vector2.zero;
        onStageChanged.Invoke(this, downPoints.Count>0);
    }

    override public void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (downPoints.Count>0&& downPoints.Contains(eventData.pointerId))
        {
            downPoints.Clear();
            onStageChanged.Invoke(this, false);
        }
    }

    override public void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }
   
    override public void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (exitAsUp && downPoints.Count > 0 && downPoints.Contains(eventData.pointerId))
        {
            downPoints.Clear();
            onStageChanged.Invoke(this, false);
        }
    }

    public class ButtonStageChangedEvent : UnityEvent<Button,bool>
    {
        public ButtonStageChangedEvent()
        {
        }
    }

    public class ButtonMoveEvent : UnityEvent<Button, Vector2>
    {
        public ButtonMoveEvent()
        {
        }
    }

}
