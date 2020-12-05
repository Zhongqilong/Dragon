using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class EventTriggerListener : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IUpdateSelectedHandler, ISelectHandler
{
    public delegate void VoidDelegate(GameObject go,params object[] pars);

    private VoidDelegate m_onClick;
    private object[] _pars;
    public VoidDelegate onClick
    {
        get { return m_onClick; }
        set
        {
            if (m_onClick != null)
            {
               // DelegateFactory.RemoveLuaDelegate(m_onClick);
                m_onClick = null;
            }
            m_onClick = value;
        }
    }
    
    private VoidDelegate m_onDown;
    public VoidDelegate onDown
    {
        get { return m_onDown; }
        set
        {
            if (m_onDown != null)
            {
               // DelegateFactory.RemoveLuaDelegate(m_onDown);
                m_onDown = null;
            }
            m_onDown = value;
        }
    }

    private VoidDelegate m_onEnter;
    public VoidDelegate onEnter
    {
        get { return m_onEnter; }
        set
        {
            if (m_onEnter != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onEnter);
                m_onEnter = null;
            }
            m_onEnter = value;
        }
    }

    private VoidDelegate m_onExit;
    public VoidDelegate onExit
    {
        get { return m_onExit; }
        set
        {
            if (m_onExit != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onExit);
                m_onExit = null;
            }
            m_onExit = value;
        }
    }

    private VoidDelegate m_onUp;
    public VoidDelegate onUp
    {
        get { return m_onUp; }
        set
        {
            if (m_onUp != null)
            {
               // DelegateFactory.RemoveLuaDelegate(m_onUp);
                m_onUp = null;
            }
            m_onUp = value;
        }    
    }

    private VoidDelegate m_onSelect;
    public VoidDelegate onSelect
    {
        get { return m_onSelect; }
        set
        {
            if (m_onSelect != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onSelect);
                m_onSelect = null;
            }
            m_onSelect = value;
        }
    }

    private VoidDelegate m_onUpdateSelect;
    public VoidDelegate onUpdateSelect
    {
        get { return m_onUpdateSelect; }
        set
        {
            if (m_onUpdateSelect != null)
            {
               // DelegateFactory.RemoveLuaDelegate(m_onUpdateSelect);
                m_onUpdateSelect = null;
            }
            m_onUpdateSelect = value;
        }
    }

    private VoidDelegate m_onLongPress;
    public VoidDelegate onLongPress
    {
        get { return m_onLongPress; }
        set
        {
            if (m_onLongPress != null)
            {
               // DelegateFactory.RemoveLuaDelegate(m_onLongPress);
                m_onLongPress = null;
            }
            m_onLongPress = value;
        }
    }

    private VoidDelegate m_onDoubleClick;
    public VoidDelegate onDoubleClick
    {
        get { return m_onDoubleClick; }
        set
        {
            if (m_onDoubleClick != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onDoubleClick);
                m_onDoubleClick = null;
            }
            m_onDoubleClick = value;
        }
    }

    public delegate void VectorDelegate(GameObject go, PointerEventData pointerData);
    private VectorDelegate m_onClickDetail;
    public VectorDelegate onClickDetail
    {
        get { return m_onClickDetail; }
        set
        {
            if (m_onClickDetail != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onClickDetail);
                m_onClickDetail = null;
            }
            m_onClickDetail = value;
        }
    }

    private VectorDelegate m_onDownDetail;
    public VectorDelegate onDownDetail
    {
        get { return m_onDownDetail; }
        set
        {
            if (m_onDownDetail != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onDownDetail);
                m_onDownDetail = null;
            }
            m_onDownDetail = value;
        }
    }

    private VectorDelegate m_onEnterDetail;
    public VectorDelegate onEnterDetail
    {
        get { return m_onEnterDetail; }
        set
        {
            if (m_onEnterDetail != null)
            {
             //   DelegateFactory.RemoveLuaDelegate(m_onEnterDetail);
                m_onEnterDetail = null;
            }
            m_onEnterDetail = value;
        }
    }

    private VectorDelegate m_onExitDetail;
    public VectorDelegate onExitDetail
    {
        get { return m_onExitDetail; }
        set
        {
            if (m_onExitDetail != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onExitDetail);
                m_onExitDetail = null;
            }
            m_onExitDetail = value;
        }
    }

    private VectorDelegate m_onUpDetail;
    public VectorDelegate onUpDetail
    {
        get { return m_onUpDetail; }
        set
        {
            if (m_onUpDetail != null)
            {
             //   DelegateFactory.RemoveLuaDelegate(m_onUpDetail);
                m_onUpDetail = null;
            }
            m_onUpDetail = value;
        }
    }

    private VectorDelegate m_onDoubleClickDetail;
    public VectorDelegate onDoubleClickDetail
    {
        get { return m_onDoubleClickDetail; }
        set
        {
            if (m_onDoubleClickDetail != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onDoubleClickDetail);
                m_onDoubleClickDetail = null;
            }
            m_onDoubleClickDetail = value;
        }
    }

    private Coroutine _UpdateHoldCoroutine;
    private VoidDelegate m_onUpdateHold;
    private IEnumerator UpdateHold()
    {
        while (true)
        {
            if (onUpdateHold != null) onUpdateHold(gameObject);
            yield return null;
        }
    }
    public VoidDelegate onUpdateHold
    {
        get { return m_onUpdateHold; }
        set
        {
            if (m_onUpdateHold != null)
            {
              //  DelegateFactory.RemoveLuaDelegate(m_onUpdateHold);
                m_onUpdateHold = null;
            }
            m_onUpdateHold = value;
        }
    }

    private Coroutine _LongClick;

    //private Vector2 pos;

    private float clickTime = float.NegativeInfinity;
    const float ClickGap = 0.5f;

    static public EventTriggerListener Get(GameObject go)
    {
        return go.transform.GetOrAddComponent<EventTriggerListener>();
    }

    static public EventTriggerListener Get(GameObject go,params object[] pars)
    {
        EventTriggerListener listener = go.transform.GetOrAddComponent<EventTriggerListener>();
        listener._pars = pars;
        return listener;
    }

    private List<IPointerClickHandler> clickRelay = new List<IPointerClickHandler>();
    public EventTriggerListener RelayClick(IPointerClickHandler clickHandler)
    {
        if (!clickRelay.Contains(clickHandler))
            clickRelay.Add(clickHandler);
        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (var p in clickRelay)
        {
            p?.OnPointerClick(eventData);
        }
    }
    private bool _doubleClickFlag = false;
    private IEnumerator ClickDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(ClickGap);
        if (!_doubleClickFlag)
        {
            if (onClick != null && !longPressTriggerd) onClick(gameObject,_pars);
            if (onClickDetail != null && !longPressTriggerd) onClickDetail(gameObject, eventData);
        }
        _doubleClickFlag = false;
    }
    private void ClickEvent(PointerEventData eventData)
    {
        if (onDoubleClick != null) //&& UIManagerEx.GetInstance().IsUIOpen("WarehouseInfoTab"))
        {
            if (clickTime + ClickGap > Time.realtimeSinceStartup)
            {
                _doubleClickFlag = true;
                if (onDoubleClick != null && !longPressTriggerd) onDoubleClick(gameObject);
                if (onDoubleClickDetail != null && !longPressTriggerd) onDoubleClickDetail(gameObject, eventData);
            }
            else
            {
                StartCoroutine(ClickDelay(eventData));
            }
        }
        else
        {
            if (onClick != null && !longPressTriggerd) onClick(gameObject, _pars);
            if (onClickDetail != null && !longPressTriggerd) onClickDetail(gameObject, eventData);
            if (clickTime + ClickGap > Time.realtimeSinceStartup)
            {
                if (onDoubleClick != null && !longPressTriggerd) onDoubleClick(gameObject);
                if (onDoubleClickDetail != null && !longPressTriggerd) onDoubleClickDetail(gameObject, eventData);
            }
        }
        clickTime = Time.realtimeSinceStartup;
    }
    private Rect rect;
    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject go = ExecuteEvents.GetEventHandler<IPointerClickHandler>(this.gameObject);
        if (go != null)
        {
            GameObject temp = go;
            //if (eventData.hovered.Count > 0)
            //    temp = eventData.hovered[0];
            RectTransform rectTrans = temp.GetComponent<RectTransform>();
            rect = UIHelper.GetChildRaycastRect(rectTrans);
            //rect = rectTrans.rect;
            Vector2 max = rect.max;
            Vector2 min = rect.min;
            max = RectTransformUtility.WorldToScreenPoint(eventData.enterEventCamera, rectTrans.TransformPoint(max));
            min = RectTransformUtility.WorldToScreenPoint(eventData.enterEventCamera, rectTrans.TransformPoint(min));
            rect = new Rect(min, max - min);
        }
        longPressTriggerd = false;
        //pos = eventData.position;
        if (onDown != null)
        {
            onDown(gameObject);
        };
        if (onLongPress != null)
        {
            if (_LongClick != null) StopCoroutine(_LongClick);
            _LongClick = StartCoroutine(LongClick(0.5f));
        }
        if (onUpdateHold != null)
        {
            if (_UpdateHoldCoroutine != null) StopCoroutine(_UpdateHoldCoroutine);
            _UpdateHoldCoroutine = StartCoroutine(UpdateHold());
        }
        if (onDownDetail != null) onDownDetail(gameObject, eventData);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
        if (onEnterDetail != null) onEnterDetail(gameObject, eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
        if (onExitDetail != null) onExitDetail(gameObject, eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        bool isIn = rect.Contains(eventData.position, true);
        bool moving = eventData.IsPointerMoving();
        if (!(eventData.dragging && moving) && isIn)
        {
            ClickEvent(eventData);
        }
        if (onUp != null) onUp(gameObject);
        if (onUpDetail != null) onUpDetail(gameObject, eventData);
        if (onLongPress != null && _LongClick != null)
        {
            StopCoroutine(_LongClick);
            _LongClick = null;
        }
        if (_UpdateHoldCoroutine != null)
        {
            StopCoroutine(_UpdateHoldCoroutine);
            _UpdateHoldCoroutine = null;
        }
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }
    private bool longPressTriggerd = false;
    private IEnumerator LongClick(float t)
    {
        yield return new WaitForSeconds(t);
        if (onLongPress != null)
        {
            longPressTriggerd = true;
            onLongPress(gameObject, _pars);
        }
    }

    protected void OnDestroy()
    {
        onClick = null;
        onDown = null;
        onEnter = null;
        onExit = null;
        onLongPress = null;
        onSelect = null;
        onUp = null;
        onUpdateSelect = null;
        onDoubleClick = null;

        onClickDetail = null;
        onDownDetail = null;
        onEnterDetail = null;
        onExitDetail = null;
        onUpDetail = null;
        onUpdateHold = null;
        onDoubleClickDetail = null;
        clickRelay.Clear();
    }

}