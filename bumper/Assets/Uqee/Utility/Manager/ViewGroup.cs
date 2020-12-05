using System;
using UnityEngine;
//主要解决加载Atlas时可能出现白图一闪而过的情况
public class ViewGroup : MonoBehaviour
{
    public bool isReady => _inited;
    public CanvasGroup canvasGroup;
    public Action onShownHandler;
    private bool _inited;
    private float _alpha = 1;
    private bool _visible = true;
    private uint _timeoutId;
    private uint _blockTimeout;
    public float alpha
    {
        get
        {
            return _alpha;
        }
        set
        {
            _alpha = value;
            if (_inited && canvasGroup != null)
            {
                canvasGroup.alpha = value;
            }
        }
    }
    public bool visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
            alpha = value ? 1 : 0;
            if (_inited && canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = value;
                canvasGroup.interactable = value;
            }
        }
    }
    void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = alpha == 1 ? 0 : alpha;
    }

    void Start()
    {
        if (alpha == 1 && canvasGroup.alpha == 0)
        {
            _timeoutId = JobScheduler.I.NextFrame(_ResetAlpha);
        }
        else
        {
            _OnInit();
        }
        _blockTimeout = JobScheduler.I.SetTimeOut(_ResetBlock, 0.1f);
        canvasGroup.blocksRaycasts = _visible;
        canvasGroup.interactable = _visible;
    }

    void _ResetBlock()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.blocksRaycasts = _visible;
        _blockTimeout = 0;
    }

    void _ResetAlpha()
    {
        canvasGroup.alpha = alpha;
        _OnInit();
    }
    void _OnInit()
    {
        _inited = true;
        OnShow();
    }
    public void OnShow()
    {
        if (_inited)
        {
            onShownHandler?.Invoke();
            onShownHandler = null;
        }
    }
}
