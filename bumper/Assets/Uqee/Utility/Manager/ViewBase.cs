using UnityEngine;
using UnityEngine.UI;

public class ViewBase : MonoBehaviour
{
    protected ViewGroup _group;
    private static Vector3 _outSidePos = new Vector3(5000, 5000, 0);
    private bool _isVisible = true;
    private float _alpha = 1;
    public bool moveOutOnInVisible { get; set; } = true;
    public UI_Layer uiLayer { get; set; } = UI_Layer.Middle;
    /// <summary>
    /// 忽略标题栏在刘海屏上的适配
    /// </summary>
    public bool ignorFixiPhoneX { get; set; } = false;
    public float alpha
    {
        get
        {
            return _alpha;
        }
        set
        {
            _alpha = value;
        }
    }
    public bool isVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            if (value)
            {
                transform.localPosition = Vector3.zero;
                _alpha = 1;
            }
            else
            {
                if (moveOutOnInVisible)
                {
                    transform.localPosition = _outSidePos;
                }
                _alpha = 0;
            }
            _isVisible = value;
        }
    }
    protected bool _isLoadFinish = false;
    virtual public bool IsLoadFinish()
    {
        //此方法尽量不要覆盖，不然可能引导界面无法正常显示问题，
        //可以覆盖IsAnimFinish方法
        return _isLoadFinish;
    }
    private void Awake()
    {
    }

    public virtual void Init()
    {
    }

    public virtual void OnLoad()
    {
        _group = gameObject.GetOrAddComponent<ViewGroup>();
        _group.canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        _group.visible = _isVisible;
        _group.alpha = _alpha;

        _group.canvasGroup.alpha = 1;

        transform.localPosition = Vector3.zero;
        this.Init();
    }

    public virtual void OnShow(object param = null)
    {
    }

/// <summary>
/// 自行添加显示界面前需要的东西
/// </summary>
    public virtual void BeforeShow()
    {
    }

    public virtual void AfterShow()
    {
        _group.OnShow();
    }

    public virtual void OnHide()
    {
        StopAllCoroutines();
        MonoBehaviour.Destroy(this.gameObject);
    }

    protected void _OnLoadFinish()
    {
        _FixFullScreen(transform);
        _isLoadFinish = true;
    }

    protected void _FixFullScreen(Transform t)
    {
        if (!ScreenManager.I.hasNorth)
        {
            return;
        }
        var fix = t.GetComponent<FixIPhoneXFullScreen>();
        if (fix != null)
        {
            fix.ignore = ignorFixiPhoneX;
            return;
        }
        for (int i = 0; i < t.childCount; i++)
        {
            var rect = t.GetChild(i) as RectTransform;
            if (rect != null && rect.anchorMin.x == 0 && rect.anchorMax.x == 1)
            {
                //全屏显示
                if (rect.GetComponent<Image>() != null || rect.GetComponent<RawImage>() || rect.GetComponent<UIHit>())
                {
                    if (rect.GetComponent<FixIPhoneXFullScreen>() == null)
                    {
                        rect.gameObject.AddComponent<FixIPhoneXFullScreen>();
                    }
                    continue;
                }
                _FixFullScreen(rect);
            }
        }
    }
}