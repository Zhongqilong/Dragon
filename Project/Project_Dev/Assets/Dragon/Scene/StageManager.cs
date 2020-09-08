using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Uqee.Pool;
using Uqee.Resource;
using Uqee.Utility;

class StageRequset
{
    public string sceneName;
    public bool addmode;
    public Action callback;
    public bool hasLod = true;

    public static StageRequset Create(string sceneName, bool addmode = false, Action callback = null, bool hasLod = true)
    {
        var req = DataFactory<StageRequset>.Get();
        req.sceneName = sceneName;
        req.addmode = addmode;
        req.callback = callback;
        req.hasLod = hasLod;
        return req;
    }

    public static void Release(StageRequset req)
    {
        if (req == null) return;
        req.callback = null;
        DataFactory<StageRequset>.Release(req);
    }

}

public class StageManager : Singleton<StageManager>
{

    #region 设置空场景

    private string _emptySceneName;
    public void SetEmptyScene(string name)
    {
        _emptySceneName = name;
        if(currSceneName==null)
        {
            currSceneName = name;
        }
    }

    #endregion 设置空场景

    // 场景监听，跟随场景的UI卸载根据场景卸载的监听来处理
    public UnityEngine.Events.UnityAction<Scene> onSceneUnload;

    public IStageAdapter adapter;
    public Func<Scene, string, bool, Camera> onSceneLoaded;
    public float progressLoading;
    public bool isError { get; private set; }
    public bool isMultiScene
    {
        get
        {
            return _sceneNameList.Count > 0;
        }
    }
    public string lastSceneName { get; private set; }
    public string currSceneName { get; private set; }

    private StageRequset _currReq;
    private bool _waitForLoad;
    private bool _isLoading;

    private List<StageRequset> _stageReqList = new List<StageRequset>();
    private List<string> _sceneNameList = new List<string>();
    private Dictionary<string, Camera> _camDict = new Dictionary<string, Camera>();
    private Dictionary<string, StageManagerHistory> _sceneDict = new Dictionary<string, StageManagerHistory>();
    private Dictionary<int, string> _sceneNameDict = new Dictionary<int, string>();

    protected override void Init()
    {
        base.Init();

        Uqee.Debug.Log("StageManager Create");
        SceneManager.sceneUnloaded += _OnSceneUnLoad;
    }

    #region 事件派发

    private void _OnSceneUnLoad(Scene scene)
    {
        onSceneUnload?.Invoke(scene);
    }

    #endregion

    #region 加载场景

    /// <summary> 加载场景 </summary>
    /// <param name="name">场景名称</param>
    /// <param name="addmode">是否附加到已有场景</param>
    /// <returns></returns>
    public bool LoadStage(string name, bool addmode = false, Action callback = null, bool hasLod = true)
    {
        if (string.IsNullOrEmpty(name))
        {
            Uqee.Debug.LogError("StageManager.LoadStage(). name is empty");
            callback?.Invoke();
            return false;
        }
        return _LoadStage(StageRequset.Create(name, addmode, callback, hasLod));
    }

    private bool _LoadStage(StageRequset req)
    {
        Uqee.Debug.Log($"LoadStage:{req.sceneName}");
        if (this._isLoading)
        {
            if (this.currSceneName == req.sceneName)
            {
                StageRequset.Release(_currReq);
                _currReq = req;
            }
            else
            {
                if (!req.addmode)
                {
                    _ClearStageReq();
                }
                else
                {
                    for (var i = 0; i < _stageReqList.Count; i++)
                    {
                        if (_stageReqList[i].sceneName == req.sceneName)
                        {
                            StageRequset.Release(_stageReqList[i]);
                            _stageReqList.RemoveAt(i);
                            break;
                        }
                    }
                }
                _stageReqList.Add(req);
            }
            // isError = true;
            // Uqee.Debug.LogError(string.Format("can't load stage {0}, {1}, is loading.", id == null ? "null" : id, currentStageName == null ? "null" : currentStageName));
            return true;
        }
        isError = false;
        _currReq = req;
        if (this.currSceneName == req.sceneName)
        {
            _InvokeOnStageLoaded();
            return false;
        }
        if (!req.addmode)
        {
            _ClearStage();
            _ClearStageReq();
        }
        if (_NeedAutoUnload(currSceneName))
        {
            UnloadStage(currSceneName);
        }
        lastSceneName = currSceneName;
        currSceneName = req.sceneName;

        Scene scene = SceneManager.GetSceneByName(currSceneName);
        //如果场景已经被加载好了，只是没有激活，则激活它
        if (scene.IsValid() && scene.isLoaded)
        {
            _OnLoaded(scene, currSceneName, req.hasLod);
            return false;
        }
        _isLoading = true;
        progressLoading = 0;

        CoroutineHelper.Start(this._LoadStageCor(req));
        return true;
    }

    private IEnumerator _LoadStageCor(StageRequset req)
    {
        //需要等资源管理器正在加载的处理完，
        //切换场景时会 UnloadUnusedAssets。 正在加载中的资源会出问题。可能导致卡死闪退
        while (ResourceProcessorManager.I.isLoading)
        {
            Uqee.Debug.LogWarning("wait resource loading");
            yield return null;
        }
        Uqee.Debug.Log($"Load Stage Async:{req.sceneName}");
        adapter?.BeginLoad(req.sceneName);
        AsyncOperation ao = SceneManager.LoadSceneAsync(req.sceneName, req.addmode ? LoadSceneMode.Additive : LoadSceneMode.Single);
        try
        {
            var test = ao.isDone;
        }
        catch (Exception e)
        {
            Uqee.Debug.LogError(e);
            isError = true;

            _InvokeOnStageLoaded();
            yield break;
        }
        while (!ao.isDone)
        {
            //Uqee.Debug.LogWarning("load scene progress:" + ao.progress);
            progressLoading = ao.progress;
            yield return null;
        }
        adapter?.EndLoad(req.sceneName);

        yield return null;
        var scene = SceneManager.GetSceneByName(req.sceneName);
        _sceneNameDict[scene.GetHashCode()] = req.sceneName;

        if (!scene.IsValid() || !scene.isLoaded)
        {
            Uqee.Debug.LogError(string.Format("场景加载失败：{0}, isvalid={1}, loaded={2}", req.sceneName, scene.IsValid(), scene.isLoaded));
            isError = true;

            _InvokeOnStageLoaded();
            yield break;
        }
#if ASSET_BUNDLE && UNITY_EDITOR
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            AssetBundleUtils.FixObj(root);
        }
#endif
        //yield return null;
        if (req.sceneName != _emptySceneName)
        {
            _OnLoaded(scene, req.sceneName, req.hasLod);
        }
        else
        {
            //可能切换场景时被回收了资源，导致进度卡在0.9，主动调用 UnloadUnusedAssets 看还会不会出现这个BUG
            ResourcesManager.I.UnloadAsset(ResourcesManager.I.GetFixAssetPath(RESOURCE_CATEGORY.STAGE_SCENE, lastSceneName));
            yield return Resources.UnloadUnusedAssets();
            adapter?.OnEmptySceneLoaded();
        }

        progressLoading = 1;
        yield return null;
        _InvokeOnStageLoaded();
    }

    private void _InvokeOnStageLoaded()
    {
        _isLoading = false;
        adapter?.OnSceneLoad(currSceneName);

        var callback = _currReq?.callback;
        StageRequset.Release(_currReq);
        _currReq = null;

        callback?.Invoke();

        _CheckStageReq();
    }

    /// <summary>
    /// 检查请求队列，是否还有其他场景需要加载
    /// </summary>
    private void _CheckStageReq()
    {
        if (_waitForLoad)
        {
            return;
        }
        Uqee.Debug.Log($"_CheckStageReq.{_stageReqList.Count}");

        _isLoading = false;

        if (_stageReqList.Count > 0)
        {
            var param = _stageReqList[0];

            _stageReqList.RemoveAt(0);
            if (param.sceneName == currSceneName)
            {
                param.callback?.Invoke();
                StageRequset.Release(param);
                _CheckStageReq();
                return;
            }
            _waitForLoad = true;
            // 等待一帧后开始加载，避免加载冲突
            JobScheduler.I.NextFrame(() =>
            {
                _waitForLoad = false;
                _LoadStage(param);
            });
        }
        else
        {
            adapter?.OnAllSceneLoaded();
        }
    }

    /// <summary>
    /// 清理请求队列
    /// </summary>
    private void _ClearStageReq()
    {
        for (int i = _stageReqList.Count - 1; i >= 0; i--)
        {
            if (_stageReqList[i].sceneName != _emptySceneName)
            {
                StageRequset.Release(_stageReqList[i]);
                _stageReqList.RemoveAt(i);
            }
        }
    }

    private void _OnLoaded(Scene scene, string sceneName, bool hasLod = false)
    {
        if (!_sceneNameList.Contains(sceneName) || !_camDict.ContainsKey(sceneName))
        {
            _RegistCamera(scene, sceneName);
            _CheckLod(sceneName, hasLod);
        }
        Camera cam = GetSceneCamera(sceneName);
        if (cam == null)
            Uqee.Debug.LogError("场景相机未找到");

        EventUtils.Dispatch("OnSceneLoaded");
        SceneManager.SetActiveScene(scene);
        adapter?.InitScene(scene, sceneName, cam);
        OnlyShowScenes(new List<string> { sceneName });
        _AddSceneHistory(scene, sceneName);
        _InvokeOnStageLoaded();
    }

    #endregion

    #region 卸载场景

    private void _ClearStage()
    {
        _camDict.Clear();
        _sceneNameList.Clear();
        _sceneNameDict.Clear();
        _sceneDict.Clear();
    }

    public void UnloadAllStage(Action onUnloaded = null)
    {
        // Uqee.Debug.LogError("------ unload all stage ----------");
        if (currSceneName == null || currSceneName == _emptySceneName)
        {
            adapter?.OnEmptySceneLoaded();
            onUnloaded?.Invoke();
        }
        else
        {
            adapter?.OnSceneUnload(_sceneNameList);
            LoadStage(_emptySceneName, false, onUnloaded);
        }
    }

    public void UnloadStage(string sceneName)
    {
        for (var i = 0; i < _stageReqList.Count; i++)
        {
            if (_stageReqList[i].sceneName == sceneName)
            {
                StageRequset.Release(_stageReqList[i]);
                _stageReqList.RemoveAt(i);
                return;
            }
        }
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene != null && scene.IsValid())
        {
            _sceneDict.Remove(sceneName);
            _camDict.Remove(sceneName);
            SceneManager.UnloadSceneAsync(scene);
            if (_sceneNameDict.ContainsKey(scene.GetHashCode()))
            {
                _sceneNameDict.Remove(scene.GetHashCode());
            }
        }
    }

    #endregion

    #region 预加载场景

    // 预加载一个场景，加载完了隐藏
    public void PreloadScene(string name, Action action, bool hasLod = true)
    {
        if (string.IsNullOrEmpty(name))
        {
            Uqee.Debug.LogError("StageManager.LoadStage(). name is empty");
            action?.Invoke();
            return;
        }

        if (currSceneName == name)
        {
            action?.Invoke();
            return;
        }

        Scene scene = SceneManager.GetSceneByName(name);
        //如果场景已经被加载好了，则不需要做任何处理
        if (scene.IsValid() && scene.isLoaded)
        {
            action?.Invoke();
            return;
        }

        _PreLoadStage(StageRequset.Create(name, true, action, hasLod));
    }

    private void _PreLoadStage(StageRequset req)
    {
        Uqee.Debug.Log($"LoadStage:{req.sceneName}");
        _currReq = req;
        _isLoading = true;
        progressLoading = 0;
        CoroutineHelper.Start(this._PreloadStageCor(req));
    }

    private IEnumerator _PreloadStageCor(StageRequset req)
    {
        //需要等资源管理器正在加载的处理完，
        //切换场景时会 UnloadUnusedAssets。 正在加载中的资源会出问题。可能导致卡死闪退
        while (ResourceProcessorManager.I.isLoading)
        {
            Uqee.Debug.LogWarning("wait resource loading");
            yield return null;
        }
        Uqee.Debug.Log($"Load Stage Async:{req.sceneName}");
        adapter?.BeginLoad(req.sceneName);
        AsyncOperation ao = SceneManager.LoadSceneAsync(req.sceneName, req.addmode ? LoadSceneMode.Additive : LoadSceneMode.Single);
        try
        {
            var test = ao.isDone;
        }
        catch (Exception e)
        {
            Uqee.Debug.LogError(e);
            isError = true;

            _InvokeOnStageLoaded();
            yield break;
        }
        while (!ao.isDone)
        {
            //Uqee.Debug.LogWarning("load scene progress:" + ao.progress);
            progressLoading = ao.progress;
            yield return null;
        }
        adapter?.EndLoad(req.sceneName);

        yield return null;
        var scene = SceneManager.GetSceneByName(req.sceneName);
        _sceneNameDict[scene.GetHashCode()] = req.sceneName;

        if (!scene.IsValid() || !scene.isLoaded)
        {
            Uqee.Debug.LogError(string.Format("场景加载失败：{0}, isvalid={1}, loaded={2}", req.sceneName, scene.IsValid(), scene.isLoaded));
            isError = true;

            _InvokeOnStageLoaded();
            yield break;
        }
#if ASSET_BUNDLE && UNITY_EDITOR
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            AssetBundleUtils.FixObj(root);
        }
#endif
        //yield return null;
        if (req.sceneName != _emptySceneName)
        {
            _RegistCamera(scene, req.sceneName);
            _CheckLod(req.sceneName, req.hasLod);
            _AddSceneHistory(scene, req.sceneName);
            // 预加载的场景，默认隐藏所有的元素
            _DisActiveAdditiveScene(req.sceneName);
        }
        else
        {
            //可能切换场景时被回收了资源，导致进度卡在0.9，主动调用 UnloadUnusedAssets 看还会不会出现这个BUG            
            yield return Resources.UnloadUnusedAssets();
            adapter?.OnEmptySceneLoaded();
        }

        progressLoading = 1;
        yield return null;
        _InvokeOnStageLoaded();
    }

    #endregion

    #region 场景查询接口
    public bool IsLoading(string sceneName = null)
    {
        if (sceneName == null)
        {
            return _waitForLoad || _isLoading || _stageReqList.Count > 0;
        }
        if (currSceneName == sceneName)
        {
            return _waitForLoad || _isLoading;
        }
        for (var i = 0; i < _stageReqList.Count; i++)
        {
            if (_stageReqList[i].sceneName == sceneName)
            {
                return true;
            }
        }

        return _waitForLoad;
    }

    public bool Contains(string sceneName)
    {
        return _sceneNameList.Contains(sceneName);
    }

    #endregion

    #region 场景相机相关
    public Camera GetSceneCamera(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = currSceneName;
        }

        if (!string.IsNullOrEmpty(sceneName) && _camDict.ContainsKey(sceneName))
        {
            return _camDict[sceneName];
        }
        return Camera.main;
    }
    private void _RegistCamera(Scene scene, string sceneName)
    {
        if (sceneName == _emptySceneName)
        {
            return;
        }
        if (sceneName == null)
        {
            return;
        }
        if (!_sceneNameList.Contains(sceneName))
        {
            _sceneNameList.Add(sceneName);
        }
        if (!_camDict.ContainsKey(sceneName))
        {
            _camDict[sceneName] = adapter?.GetCamera(scene, sceneName);
        }
    }

    #endregion

    #region 场景Lod

    private void _CheckLod(string sceneName, bool loadLod = false)
    {
        if (loadLod)
        {
            _LoadLod(sceneName);
        }
    }
    private void _LoadLod(string sceneName)
    {
        LoadingUtils.InvokeWhenFinish(() => {
            if (!_sceneNameList.Contains(sceneName))
            {
                return;
            }
            adapter?.LoadLod(sceneName);
        });
    }

    #endregion


    #region 场景信息存储
    public class StageManagerHistory
    {
        public string sceneName;
        public Dictionary<string, bool> acitveHistoryDict = new Dictionary<string, bool>();
        public void SetActive(bool isactive)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene == null || !scene.IsValid())
            {
                return;
            }
            GameObject[] rootobjs = scene.GetRootGameObjects();
            for (int i = 0; i < rootobjs.Length; i++)
            {
                bool active = true;
                if (acitveHistoryDict.TryGetValue(rootobjs[i].name, out active))
                {
                    rootobjs[i].SetActive(isactive ? active : false);
                }
            }
        }
    }

    /// <summary> 检查是否已经加载过了</summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private void _AddSceneHistory(Scene scene, string sceneName)
    {
        if (_sceneDict.ContainsKey(sceneName))
        {
            _sceneDict[sceneName].sceneName = sceneName;
            GameObject[] objs = scene.GetRootGameObjects();
            for (int i = 0; i < objs.Length; i++)
            {
                _sceneDict[sceneName].acitveHistoryDict[objs[i].name] = objs[i].activeSelf;
            }
        }
        else
        {
            var history = _sceneDict[sceneName] = new StageManagerHistory();

            history.sceneName = sceneName;
            GameObject[] objs = scene.GetRootGameObjects();
            for (int i = 0; i < objs.Length; i++)
            {
                history.acitveHistoryDict[objs[i].name] = objs[i].activeSelf;
            }
            if (!history.acitveHistoryDict.ContainsKey("LodModel"))
            {
                history.acitveHistoryDict["LodModel"] = true;
                var go = new GameObject("LodModel");
                SceneManager.MoveGameObjectToScene(go, scene);
            }
        }
    }

    private void _DisActiveAdditiveScene(string sceneName)
    {
        if (_sceneDict.ContainsKey(sceneName))
        {
            var scene = _sceneDict[sceneName];
            scene.SetActive(false);
        }
    }

    /// <summary> 把其他场景的物体隐藏起来</summary>
    public void OnlyShowScenes(List<string> sceneNameList)
    {
        foreach (var kp in _sceneDict)
        {
            if (sceneNameList.Contains(kp.Key)) ////需要显示的场景
            {
                kp.Value.SetActive(true);
                currSceneName = kp.Value.sceneName;
            }
            else
            {
                kp.Value.SetActive(false);
            }
        }
    }

    public Transform FindRoot(string name, string sceneName = null)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName ?? currSceneName);
        if (!scene.IsValid())
            return null;
        var rootArr = scene.GetRootGameObjects();
        Transform container = null;
        foreach (var root in rootArr)
        {
            if (root.name == name)
            {
                container = root.transform;
                break;
            }
        }
        return container;
    }
    public Transform GetOrCreateRoot(string name, string sceneName = null)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName ?? currSceneName);
        var rootArr = scene.GetRootGameObjects();
        Transform container = null;
        foreach (var root in rootArr)
        {
            if (root.name == name)
            {
                container = root.transform;
                break;
            }
        }

        if (container == null)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            container = go.transform;
        }
        return container;
    }

    #endregion

    private bool _NeedAutoUnload(string sceneName)
    {
        if (sceneName == SceneNameConst.WORLD_02)
            return false;

        if (sceneName.Contains("GQ"))
            return false;

        return true;
    }

    #region 处理场景audio listener

    private void _DisableAudioListener()
    {

    }

    private void _EnableAudioListener()
    {

    }

    #endregion
}