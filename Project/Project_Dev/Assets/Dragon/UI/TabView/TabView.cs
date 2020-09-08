using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Uqee.Utility;

public class TabView : PoolableMono
{
    //0第一个节点的"层级"最大,1不处理层级，其他：最后一个节点的层级最大
    // private int _zorderType = 0;
    // private Action<int> _callback;
    // //0=unselect color,1=select color
    // public Color[] tabColorArr = null;
    private List<Transform> _tabList = new List<Transform>();
    private List<Vector3> _tabPanelPosList = new List<Vector3>();
    private List<ViewGroup> _tabPanelGroupList = new List<ViewGroup>();
    private List<IPanel> _tabPanelList = new List<IPanel>();
    private List<Toggle> _tabToggleList = new List<Toggle>();
    private List<Text> _tabLabelList = new List<Text>();
    private List<GameObject> _tabSelectedImgList = new List<GameObject>();
    private bool _inited;
    private TabViewParam _param;
    public bool moveOutInVisible = true;
    public Transform tran_tabCont;
    public int currIdx {get; private set;} = -1;
    public int lastIdx {get; private set;} = -1;
    private Vector3 _outSizePos = new Vector3(-5000,0,0);

    private bool _useInPop = false;
    public bool useInPop
    {
        set
        {
            _useInPop = value;
            var tabGroup = transform.GetComponent<TabGroup>();
            var layout = tabGroup.GetComponent<VerticalLayoutGroup>();
            layout.spacing = value ? 20 : 5;
            if (!value)
            {
                transform.GetComponent<RectTransform>().SetHeight(652);
            }
        }
        get { return _useInPop; }
    }

    public override void OnDespawn()
    {
        currIdx = -1;
        lastIdx = -1;
        if (_inited)
        {
            TabViewParam.Release(_param);
        }
        tran_tabCont = null;
        for(int i=0; i<_tabList.Count; i++)
        {
            var child = _tabList[i];
            if (child == null) continue;
            if (child.GetComponent<Toggle>() != null)
            {
                child.GetOrAddComponent<BaseToggleItemBehaviour>().Clear();
            }
            else
            {
                child.GetOrAddComponent<ClickItemBehaviour>().Clear();
            }
        }
        _tabPanelList.Clear();
        _tabPanelPosList.Clear();
        _tabPanelGroupList.Clear();
        _tabToggleList.Clear();
        _tabLabelList.Clear();
        _tabSelectedImgList.Clear();
        _tabList.Clear();
        _inited = false;
        EventUtils.RemoveListener("UNLOCK_FUNCTION", _UNLOCK_FUNCTION);
    }

    public Transform GetTab(int idx)
    {
        if(idx<0 || idx>=_tabList.Count)
        {
            return null;
        }
        return _tabList[idx];
    }
    public Transform GetCont(int idx)
    {
        if (idx < 0 || idx >= _tabPanelGroupList.Count)
        {
            return null;
        }
        // var viewName = _param.cfgArr[idx].viewName;
        // if(string.IsNullOrEmpty(viewName))
        // {
        //     return null;
        // }
        // return tran_tabCont.Find(viewName);
        return _tabPanelGroupList[idx]?.transform;
    }
    private void _SetTabOn(int idx, bool isOn)
    {
        var tab = _tabList[idx];
        var tog = _tabToggleList[idx];
        if (tog != null)
        {
            tog.isOn = isOn;
            tog.targetGraphic.raycastTarget = !isOn;
            tog.graphic.raycastTarget = !isOn;
        }
        else
        {
            _tabSelectedImgList[idx]?.SetActive(isOn);
        }
        if (_param.colorArr != null && _param.colorArr.Length>1)
        {
            var txt = _tabLabelList[idx];
            if (txt != null)
            {
                txt.color = _param.colorArr[isOn?1:0];
            }
        }
    }
    public void SetTabIdx(int idx)
    {
        _OnTabClick(idx);
    }
    private void _OnTabClick(int idx)
    {
        if(currIdx==idx)
        {
            return;
        }
        if(idx<0 || idx>=_tabList.Count)
        {
            idx = 0;
        }
        switch(_param.zorderType) {
            case 0:
                for (var i = _tabList.Count - 1; i >= 0; i--)
                {
                    _SetTabOn(i, i == idx);
                    if (i != idx)
                    {
                        _tabList[i].SetAsLastSibling();
                    }
                }
                _SetTabOn(idx, true);
                _tabList[idx].SetAsLastSibling();
                break;
            case 1:
                for (var i = 0; i < _tabList.Count; i++)
                {
                    _SetTabOn(i, i == idx);
                }
                break;
            default:
                for (var i = 0; i < _tabList.Count; i++)
                {
                    _SetTabOn(i, i == idx);
                    if (i != idx)
                    {
                        _tabList[i].SetAsFirstSibling();
                    }
                }
                _tabList[idx].SetAsFirstSibling();
                break;
        }
        var cfg = _param.cfgArr[idx];
        if(cfg.funcId!=0 && !DataUtils.IsFunctionOpen(cfg.funcId, true))
        {
            return ;
        }
        if(cfg.onOpenCheck!=null && !cfg.onOpenCheck(idx))
        {
            return;
        }
        lastIdx = currIdx;
        currIdx = idx;

        var exist = false;
        if(string.IsNullOrEmpty(cfg.viewName)){
            exist = true;
        } else {
            for(int k=0; k<_tabPanelGroupList.Count; k++)
            {
                var child = _tabPanelGroupList[k];
                if(child==null)
                {
                    continue;
                }
                var view = _tabPanelList[k];
                // var child = tran_tabCont.GetChild(k);

                // var view = child.GetComponent<IViewPanel>();
                if (child.name!=cfg.viewName )
                {
                    // child.gameObject.SetActive(false);

                    child.visible = false;
                    if(moveOutInVisible)
                    {
                        child.transform.localPosition = _outSizePos;
                    }
                    if (view!=null)
                    {
                        view.OnDisActive();
                    }
                }
                else
                {
                    child.visible = true;
                    child.transform.localPosition = _tabPanelPosList[k];
                    
                    // child.gameObject.SetActive(true);
                    if (view!=null)
                    {
                        view.OnActive();
                    }
                    exist = true;
                }
            }
        }
        if (!exist) 
        {            
            var inst = ResManager.LoadInstSync(RESOURCE_CATEGORY.UI, cfg.viewName, tran_tabCont);
            if(inst!=null)
            {
                _AddPanel(inst.transform, idx);
                
                if (_tabPanelList[idx] != null)
                {
                    _tabPanelList[idx].OnActive();
                }
            }
            // UIManager.I.AddPrefabTo(cfg.viewName, _OnPanelLoaded, tran_tabCont);
        }
        // else
        // {
        //     _param.callback?.Invoke(currIdx);
        // }
        _param.callback?.Invoke(currIdx);
    }

    public void Clear()
    {
        currIdx = -1;
        lastIdx = -1;
        for (int i=0; i< _tabPanelGroupList.Count; i++)
        {
            if(_tabPanelGroupList[i]!=null)
            {
                DestroyImmediate(_tabPanelGroupList[i].gameObject);
                _tabPanelGroupList[i] = null;
                _tabPanelList[i] = null;
            }
        }
    }

    private void _AddPanel(Transform panel,int idx)
    {
        
        _tabPanelPosList[idx] = panel.localPosition;

        // var group = panel.GetOrAddComponent<CanvasGroup>();
        // group.blocksRaycasts = true;
        // group.interactable = true;
        var group = panel.GetOrAddComponent<ViewGroup>();
        group.canvasGroup = panel.GetOrAddComponent<CanvasGroup>();
        _tabPanelGroupList[idx] = group;

        var view = panel.GetComponent<IPanel>();
        
        if (view != null)
        {
            _tabPanelList[idx] = view;
        }
        // group.alpha = 0;
        // JobScheduler.I.SetTimeOut(() =>
        // {
        //     if (group.interactable)
        //     {
        //         group.alpha = 1;
        //     }
        // }, 0.12f * QualityLevelManager.I.phoneLevel );
    }
    // void _OnPanelLoaded(GameObject go)
    // {
    //     var view = go.GetComponent<IViewPanel>();
    //     if (view != null)
    //     {
    //         view.OnActive();
    //     }
    //     _param.callback?.Invoke(currIdx);
    // }
    /// <summary>
    /// /// 
    /// </summary>
    /// <param name="callBack">点击回调</param>
    /// <param name="dis">需要自动排列的话传入btn的间距</param>
    /// <param name="zorderType">0表示第一个孩子的"层级"最大</param>
    public void Init(TabViewParam param)
    {
        if (_inited)
        {
            _OnTabClick(param.defaultTabIdx);
            TabViewParam.Release(param);
            return;
        }
        _inited = true;
        _param = param;
        useInPop = false;
        EventUtils.AddListener("UNLOCK_FUNCTION", _UNLOCK_FUNCTION);
        var childCount = transform.childCount;
        //var maxY = param.dis * (childCount - 1) * 0.5f;
        var firstActive = -1;
        //var tabActiveIdx = 0;
        for (var i = 0; i < _param.cfgArr.Length; ++i)
        {
            var cfg = _param.cfgArr[i];
            var child = string.IsNullOrEmpty(cfg.tabName) ? transform.GetChild(i) : transform.Find(cfg.tabName);
            if (child == null)
            {
                Uqee.Debug.LogError("TabView.Init() fail. tab数量与配置不一致");
                if (_param.defaultTabIdx == i)
                {
                    _param.defaultTabIdx = -1;
                }
                return;
            }
            _tabList.Add(child);
            _tabPanelList.Add(null);
            _tabPanelGroupList.Add(null);
            _tabPanelPosList.Add(_outSizePos);
            _tabToggleList.Add(child.GetComponent<Toggle>());
            _tabLabelList.Add(child.GetComponentInChildren<Text>());
            _tabSelectedImgList.Add(child.Find("img_selected")?.gameObject);

            if (!string.IsNullOrEmpty(cfg.viewName))
            {
                var panel = tran_tabCont.Find(cfg.viewName);
                if(panel!=null)
                {
                    _AddPanel(panel.transform, i);
                }
            }
            bool hide = false;
             
            if ((cfg.funcId != 0 && !DataUtils.IsFunctionOpen(cfg.funcId)) || (cfg.onOpenCheck!=null&& !cfg.onOpenCheck(i)))
            {
                if (_param.defaultTabIdx == i)
                {
                    _param.defaultTabIdx = -1;
                }
                //TODO:可以修改cfg配置，扩展为不隐藏，添加提示什么时候开放
                if (cfg.hideWhenClose)
                {
                    hide = true;
                }
            }
            else
            {
                if (firstActive == -1)
                {
                    firstActive = i;
                }
            }

            //if (param.dis != 0)
            //{
            //    child.GetComponent<RectTransform>().localPosition = new Vector3(0, maxY - param.dis * tabActiveIdx, 0);
            //}
            //if(!hide)
            //{
            //    tabActiveIdx++;
            //}
            if (child.GetComponent<Toggle>() != null)
            {
                child.GetOrAddComponent<BaseToggleItemBehaviour>().InitData(i, _OnTabClick);
            }
            else
            {
                child.GetOrAddComponent<ClickItemBehaviour>().InitData(i, _OnTabClick);
            }
            if (hide)
            {
                child.gameObject.SetActive(false);
            }
        }
        if (firstActive == -1)
        {
            Uqee.Debug.LogError("TabView.Init() fail. tab可显示数量为0.");

            return;
        }
        if (_param.defaultTabIdx == -1)
        {
            _param.defaultTabIdx = firstActive;
        }
        JobScheduler.I.NextFrame(_FirstClick);
    }
    void _FirstClick()
    { 
        if(!_inited)
        {
            return;
        }
        _OnTabClick(_param.defaultTabIdx);
    }
    public void RefreshTabs()
    {
        _UNLOCK_FUNCTION();
    }
    void _UNLOCK_FUNCTION()
    {
        if(!_inited)
        {
            return;
        }
        for (var i = 0; i < _param.cfgArr.Length; ++i)
        {
            var cfg = _param.cfgArr[i];
            bool isOpen = (cfg.funcId == 0 ||DataUtils.IsFunctionOpen(cfg.funcId) 
                &&( cfg.onOpenCheck == null || cfg.onOpenCheck(i)));
            
            var child = string.IsNullOrEmpty(cfg.tabName) ? transform.GetChild(i) : transform.Find(cfg.tabName);
            //var isOpen = DataUtils.IsFunctionOpen(cfg.funcId);
            if (!isOpen && child.gameObject.activeSelf) {
                child.gameObject.SetActive(false);
            }
            if (isOpen && !child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
            }
           
        }
    }
    private void OnDestroy() {
        OnDespawn();
    }
}