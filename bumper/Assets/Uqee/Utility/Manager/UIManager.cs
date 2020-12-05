using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Uqee.Resource;
using Uqee.Utility;

public class UIManager : Singleton<UIManager>
{
    //UI 挂靠点
    public Transform trans_UI;
    public Transform trans_Game;
    public Transform tran_Inactive { get; private set; }
    public AudioListener audioListener { get; private set; }
    public Camera cam_UICam { get; private set; }
    public Canvas canvas { get; private set; }
    public CanvasScaler canvasScale { get; private set; }
    /// <summary>
    /// 加载中的UI列表
    /// </summary>
    private List<string> _viewLoadingList = new List<string>();
    private List<string> _prefabLoadingList = new List<string>();
    private List<string> _hiddenList = new List<string>();

    private string _loadingView;
    //保存所有的view
    private Dictionary<string, ViewBase> _viewDict = new Dictionary<string, ViewBase>();
    /// <summary>
    /// 调用单例I.ShowView的初始化
    /// </summary>
    protected override void Init()
    {
        base.Init();

        _CreateGameObject();
    }

/// <summary>
/// 设置相机的参数以及View的相关层级
/// </summary>
    private void _CreateGameObject()
    {
        var root = UIGameObjectCreator.I.gameObject;
        this.canvas = root.GetComponent<Canvas>();
        this.canvasScale = root.GetComponent<CanvasScaler>();

    #region 相机
        Camera ca = null;
        GameObject caObj = null;
        //背景相机，防止一些场景切换时花屏
        caObj = UIGameObjectCreator.I.GetOrCreate("BgCamera");
        ca = caObj.GetComponent<Camera>();
        if (ca == null)
        {
            ca = caObj.AddComponent<Camera>();
            ca.clearFlags = CameraClearFlags.SolidColor;
            ca.backgroundColor = Color.black;
            ca.cullingMask = 0;
            ca.orthographic = true;
            ca.orthographicSize = 0.01f;
            ca.nearClipPlane = -0.01f;
            ca.farClipPlane = 1;
            ca.depth = -100;
            ca.useOcclusionCulling = true;
            ca.allowDynamicResolution = false;
            ca.allowHDR = false;
            ca.allowMSAA = false;
        }

        //UI相机
        caObj = UIGameObjectCreator.I.GetOrCreate("UICamera");
        ca = caObj.GetComponent<Camera>();
        if (ca == null)
        {
            ca = caObj.AddComponent<Camera>();
            ca.clearFlags = CameraClearFlags.Depth;
            ca.cullingMask = LAYER.UI;
            ca.orthographic = true;
            ca.orthographicSize = 36;
            ca.nearClipPlane = -100;
            ca.farClipPlane = 100;
            ca.depth = 2;
            ca.useOcclusionCulling = true;
            ca.allowDynamicResolution = false;
            ca.allowHDR = false;
            ca.allowMSAA = false;
        }
        audioListener = caObj.GetOrAddComponent<AudioListener>();
        canvas.worldCamera = ca;
        cam_UICam = ca;
    #endregion

    #region 创建UI的相关层级
        root = ViewGameObjectCreator.I.gameObject;
        trans_Game = UIGameObjectCreator.I.GetOrCreate("Game", 1, true).transform;
        trans_UI = UIGameObjectCreator.I.GetOrCreate("UI", 2, true).transform;
        trans_UI.gameObject.layer = 5;
        var go = UIGameObjectCreator.I.GetOrCreate("Inactive");
        go.SetActive(false);
        var rect2 = go.GetOrAddComponent<RectTransform>();
        rect2.anchoredPosition = Vector2.zero;
        tran_Inactive = go.transform;
    #endregion

    ScreenManager.I.Fix(canvasScale);
    }

    public void ShowView<T>(object data = null, Transform parent = null) where T : MonoBehaviour
    {
        ShowViewTo(typeof(T).Name, parent, data);
    }


    public void ShowViewTo(string viewName, Transform parent = null, object param = null, Action onShowListener = null, int showtype = LAYER.UI)
    {
        if (string.IsNullOrEmpty(viewName))
        {
            return;
        }

        if (IsLoading(viewName))
        {
            //UnityEngine.Debug.LogError(viewName + "加载出错-----------------");
            return;
        }
        _hiddenList.Remove(viewName);
        CoroutineHelper.Start(_LoadViewAsync(viewName, parent, param, showtype));
    }

    /// <summary>
    /// 判断是否有view正在加载
    /// </summary>
    /// <param name="name">ViewName</param>
    /// <returns></returns>
    public bool IsLoading(string name)
    {
        return _loadingView == name || _viewLoadingList.Count > 0;
    }

    /// <summary>
    /// 使用迭代器加载资源
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="parent"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    private IEnumerator _LoadViewAsync(string viewName, Transform parent, object param, int showtype)
    {
        if(parent == null)
        {
            parent = showtype == LAYER.GAME ? trans_Game : trans_UI;
        }

        _viewLoadingList.Add(viewName);
        //yield return null:下一帧再执行后续代码
        while (_loadingView != null)
        {
            yield return null;
        }
        _loadingView = viewName;
        ViewBase view = null;
        bool isExsists = GetViewCache(viewName, out view);
        if (isExsists)
        {
            view.gameObject.SetActive(true);
            yield return _OnViewShow(view, param);
        }else
        {
            if (view == null)
            {
                yield return _LoadAsync(viewName, parent, (go) =>
                {
                    if (go != null)
                    {
                        view = go.GetComponent<ViewBase>();
                    }
                }, param);
            }
            //当view处于隐藏的列表之中则不显示
            if (_hiddenList.Contains(viewName))
            {
                view = null;
            }else //加载完成
            {
                yield return _OnViewLoad(view, viewName, parent);
            }
            yield return _OnViewShow(view, param);
        }
    }

    public bool GetViewCache(string prefabName, out ViewBase view)
    {
        view = null;
        _viewDict.TryGetValue(prefabName, out view);
        if (view == null)
        {
            var inactiveObj = tran_Inactive.Find(prefabName);
            if (inactiveObj != null)
            {
                view = inactiveObj.GetComponent<ViewBase>();
                return true;
            }
            return false;
        }
        return true;
    }

    private IEnumerator _OnViewShow(ViewBase view, object param = null)
    {
        if (view != null)
        {
            string prefabName = view.name;
            try
            {
                view.OnShow(param);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
            try
            {
                view.AfterShow();
            }
            catch (Exception ex)
            {
                
                UnityEngine.Debug.LogError(ex);
            }
            yield return null;
        }
    }

    public void HideView(string viewName)
    {
        if (IsLoading(viewName))
        {
            if (! _hiddenList.Contains(viewName))
            {
                _hiddenList.Add(viewName);
            }
        }
        else if (_viewDict.ContainsKey(viewName))
        {
            var view = _viewDict[viewName];
            _viewDict.Remove(viewName);
            try
            {
                view.OnHide();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e);
                throw;
            }
            InstantiateCache.I.DespawnChildren(view.transform);
        }
    }

    private IEnumerator _OnViewLoad(ViewBase view, string prefabName, Transform parent)
    {
        view.name = prefabName;
        _viewDict[prefabName] = view;

        try
        {
            view.OnLoad();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(ex);
            throw;
        }
        EventUtils.Dispatch(UIEvent.UI_BEFORE_SHOW, prefabName, (int)view.uiLayer);
        yield return null;

        while (view != null && !view.IsLoadFinish())
        {
            yield return null;
        }
        if (view == null)
        {
            yield break;
        }

        if (parent == null)
        {
            parent = trans_UI;
        }
        try
        {
             view.BeforeShow();
        }
        catch (System.Exception)
        {
            
            throw;
        }

        /// <summary>
        /// 预留处理接口
        /// </summary>
        /// <value></value>
        if (view.transform.parent != parent){}
    }

    private IEnumerator _LoadAsync(string prefabName, Transform parent, Action<GameObject> onLoad = null, object param = null)
    {
        if (parent == null)
        {
            yield break;
        }

        if (InstantiateCache.I.CanSpawn(RESOURCE_CATEGORY.UI, prefabName))
        {
            try
            {
                GameObject go  = null;
                var inst = InstantiateCache.I.Spawn(RESOURCE_CATEGORY.UI, prefabName, parent);
                if (inst != null)
                {
                    go = inst.gameObject;
                }
                onLoad?.Invoke(go);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            yield break;
        }
        _prefabLoadingList.Add(prefabName);
        bool isCompleted = false;
        AssetRequest req = InstantiateCache.LoadAssets(RESOURCE_CATEGORY.UI, prefabName, false, (r) => {isCompleted = true;});
        if(! string.IsNullOrEmpty(req.assetPath))
        {
            var go = Resources.Load(req.assetPath) as GameObject;
            var trans = parent ?? tran_Inactive;
            go = MonoBehaviour.Instantiate(go, trans);
            var view = go.GetComponent<ViewBase>();
            go.name = prefabName;
            view.OnLoad();
            trans.gameObject.SetActive(true);
            _OnViewLoadFinish(prefabName, view);
            yield return _OnViewShow(view, param);
        }
        while(! isCompleted)
            yield return null;

        if(parent != null)
        {
            if(req.loadedObj != null && req.loadedObj is GameObject)
            {
                var prefabObj = req.loadedObj as GameObject;

                var tran = InstantiateCache.I.Spawn(RESOURCE_CATEGORY.UI, prefabName, parent);
                if (tran != null)
                {
                    prefabObj = tran.gameObject;
                }
                else
                {
                    prefabObj = UnityEngine.Object.Instantiate(prefabObj, parent);
                }
                if (prefabObj == null)
                {
                    //UnityEngine.Debug.LogError("PrefabObj is error!");
                }
                else
                {
                    prefabObj.name = prefabName;

                    try
                    {
                        onLoad?.Invoke(prefabObj);
                    }catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }
            }
        }
        _prefabLoadingList.Remove(prefabName);
    }

    private void _OnViewLoadFinish(string viewName, ViewBase view)
    {
        _viewDict[viewName] = view;
        _viewLoadingList.Remove(viewName);
        _loadingView = null;
    }
}