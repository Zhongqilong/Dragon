using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Uqee.Resource;

public static class Spine2DAction
{
    public static string IDLE = "idle";
    public static string TALK = "talk";
}
public class SpinePool
{
    private static Dictionary<string, SpinePool> _poolDict = new Dictionary<string, SpinePool>();
    public static SpinePool GetPool(string poolName)
    {
        if (!_poolDict.ContainsKey(poolName))
        {
            _poolDict[poolName] = new SpinePool($"{poolName}SpinePool");
        }
        return _poolDict[poolName];
    }
    public static void ReleaseCache()
    {
        foreach (var pool in _poolDict.Values)
        {
            pool.ReleaseCache(true);
        }
    }
    const float AUTO_RELEASE_DELAY = 10;
    const int MAX_CACHE = 5;
    private Transform tran_pool;
    private List<GameObject> _spineList = new List<GameObject>();
    private List<float> _timeList = new List<float>();
    private List<string> _assetPathList = new List<string>();

    public SpinePool(string poolName)
    {
        var root = new GameObject(poolName);
        UnityEngine.Object.DontDestroyOnLoad(root);
        tran_pool = root.transform;
        root.SetActive(false);

        UpdateManager.I.AddCallback(_Update, poolName);
    }
    private void _Update()
    {
        ReleaseCache(false);
    }
    public SkeletonGraphic Get(string name)
    {
        int len = _spineList.Count;
        GameObject spine;
        for (int i = 0; i < len; i++)
        {
            spine = _spineList[i];
            if (spine != null && spine.name == name)
            {
                _timeList.RemoveAt(i);
                _assetPathList.RemoveAt(i);
                _spineList.RemoveAt(i);
                return spine.GetComponent<SkeletonGraphic>();
            }
        }
        return null;
    }
    public void Put(SkeletonGraphic spine, string assetPath)
    {
        if (spine != null)
        {
            spine.transform.SetParent(tran_pool, false);
            _spineList.Add(spine.gameObject);
        }
        else
        {
            _spineList.Add(null);
        }
        _assetPathList.Add(assetPath);
        _timeList.Add(AppStatus.realtimeSinceStartup + AUTO_RELEASE_DELAY);
        int cnt = _timeList.Count - MAX_CACHE;
        if (cnt > 1)
        {
            //超过5个，移除
            for (int i = 0; i < cnt; i++)
            {
                _timeList[i] = AppStatus.realtimeSinceStartup;
            }
        }
    }
    public void ReleaseCache(bool all)
    {
        var endIdx = _timeList.Count - 1;
        if (!all)
        {
            for (int i = endIdx; i >= 0; i--)
            {
                if (_timeList[i] <= AppStatus.realtimeSinceStartup)
                {
                    endIdx = i;
                    break;
                }
            }
        }
        for (int i = endIdx; i >= 0; i--)
        {
            AssetsCache.I.RemoveCache(_assetPathList[i], true);
            if (_spineList[i])
            {
                UnityEngine.Object.Destroy(_spineList[i]);
            }
            _assetPathList.RemoveAt(i);
            _timeList.RemoveAt(i);
            _spineList.RemoveAt(i);
        }
    }
}
public class Spine2D : PoolableMono
{
    //public SkeletonGraphic ske_halfbody;
    private SkeletonGraphic _spineObj;
    public RawImage img_halfbody;
    public Action<GameObject> onLoaded;
    public bool freeze;
    private bool _isActive;
    protected string _skeName;
    protected string _imgName;
    protected string _nameSpell;
    protected string _folder;
    protected Vector2 _spinePos = Vector2.zero;
    private bool _isGray;
    private AssetRequest _spineReq;
    private bool _isLoadedSuccess;
    private string _tmpAction;
    private Material _spineMat;
    //private SpinePool _spinePool;

    public string halfName
    {
        get
        {
            return _nameSpell;
        }
    }
    public virtual void Init()
    {

    }
    private void Awake()
    {
        _isActive = true;
        //ske_halfbody.startingLoop = true;
        //ske_halfbody.startingAnimation = Spine2DAction.IDLE;
        //ske_halfbody.raycastTarget = false;
        //ske_halfbody.transform.localScale = Vector3.zero;
        img_halfbody.transform.localScale = Vector3.zero;
    }
    private void OnDestroy()
    {
        _isActive = false;
    }
    private void _Reset()
    {
        string assetPath = null;
        if (_spineReq != null)
        {
            assetPath = _spineReq.assetPath;
            _spineReq.refCount--;
            _spineReq = null;
        }


        if (_spineObj != null)
        {
            _spineObj.material = _spineMat;
            _spineMat = null;
            if (assetPath == null)
            {
                assetPath = ResManager.GetAssetPath(RESOURCE_CATEGORY.SPINE_ASSET, _skeName);
            }
            SpinePool.GetPool(_folder).Put(_spineObj, assetPath);
            _spineObj = null;
        }
        else
        {
            SetGray(false);
            img_halfbody.texture = null;
            img_halfbody.transform.localScale = Vector3.zero;

            SpinePool.GetPool(_folder).Put(null, assetPath);
        }
        _isLoadedSuccess = false;

    }
    public override void OnDespawn()
    {
        _Reset();
        _isActive = false;
        _tmpAction = null;
        _skeName = null;
        _imgName = null;
        _nameSpell = null;
    }
    public override void OnSpawn()
    {
        base.OnSpawn();
        _isActive = true;
    }
    private static Dictionary<string, Skeleton> _skeDict = new Dictionary<string, Skeleton>();
    protected void _OnHalfLoaded(AssetRequest req)
    {
        if (!_isActive || req == null)
        {
            return;
        }

        if (req.assetName != _skeName && req.assetName != _imgName)
        {
            return;
        }
        if (req.loadedObj == null)
        {
            if (req.assetName == _skeName)
            {
                _LoadImg();
            }
            else
            {
                onLoaded?.Invoke(gameObject);
            }
            return;
        }

#if ASSET_BUNDLE
        _spineReq = CacheManager.I.GetCache<IAssetsCache>()?.GetCache(req.assetPath);
#else
        _spineReq = req;
#endif
        if (req.loadedObj is Texture2D)
        {
            var tex = req.loadedObj as Texture2D;
            img_halfbody.texture = tex;
            tex.name = _nameSpell;
            Vector2 size;
            if (ResourceDefineManager.I.imgSizeDict.TryGetValue(_imgName, out size))
            {
                img_halfbody.rectTransform.sizeDelta = size;
            }
            else
            {
                img_halfbody.rectTransform.sizeDelta = new Vector2(tex.width, tex.height);
            }
            img_halfbody.transform.localScale = Vector3.one;

            ResourcesManager.I.AddInstantiateObj(img_halfbody.gameObject, req.assetPath);
        }
        else
        {
            var obj = req.loadedObj as GameObject;
            if (obj == null)
            {
                onLoaded?.Invoke(gameObject);
                _spineReq = null;
                return;
            }
            var go = Instantiate(obj, transform);
            ResourcesManager.I.AddInstantiateObj(go, req.assetPath);
            AssetBundleUtils.FixObj(go);
            go.name = _nameSpell;
            _SetSpine(go.GetComponent<SkeletonGraphic>());
        }
        if (_spineReq != null)
        {
            _spineReq.refCount++;
        }
        _isLoadedSuccess = true;
        SetGray(_isGray);
        onLoaded?.Invoke(gameObject);
    }
    private void _SetSpine(SkeletonGraphic obj)
    {
        if (obj == null)
        {
            return;
        }
        _spineObj = obj;
        _spineMat = obj.material;
        ((RectTransform)_spineObj.transform).anchoredPosition = _spinePos;
        _spineObj.transform.localEulerAngles = Vector3.zero;
        _spineObj.transform.localScale = Vector3.one;
        if (_tmpAction != null)
        {
            PlayAction(_tmpAction);
            _tmpAction = null;
        }
    }
    protected virtual void _LoadImg()
    {
        var req = ResManager.LoadAssets(RESOURCE_CATEGORY.SPINE_IMG, _imgName, false, _OnHalfLoaded);

    }
    public void PlayAction(string action)
    {
        try
        {
            if (_spineObj == null)
            {
                _tmpAction = action;
                return;
            }
            if (action == Spine2DAction.TALK)
            {
                _spineObj.AnimationState.SetAnimation(0, action, false);
                _spineObj.AnimationState.Complete += _OnTalkComplete;
            }
            else
            {
                _spineObj.AnimationState.Complete -= _OnTalkComplete;
                _spineObj.AnimationState.SetAnimation(0, action, true);
            }
        }
        catch (Exception) { }
    }
    private void _OnTalkComplete(TrackEntry t)
    {
        PlayAction(Spine2DAction.IDLE);
    }
    public void LoadHalf(string fileName)
    {
        if (_spineObj != null)
        {
            if (_spineObj.name == fileName)
            {
                onLoaded?.Invoke(gameObject);
                return;
            }
            _Reset();
        }
        else
        {
            if (img_halfbody.texture != null)
            {
                if (img_halfbody.texture.name == fileName)
                {
                    onLoaded?.Invoke(gameObject);
                    return;
                }
                _Reset();
            }
        }
        if (_nameSpell == fileName)
        {
            return;
        }
        if (string.IsNullOrEmpty(fileName))
        {
            _Reset();
            _isLoadedSuccess = true;
            onLoaded?.Invoke(gameObject);
            return;
        }
        //ske_halfbody.transform.localScale = Vector3.zero;
        img_halfbody.transform.localScale = Vector3.zero;
        _nameSpell = fileName;// name.Replace("_half", "");
        _imgName = $"{_folder}/{_nameSpell}/{_nameSpell}";
        //_skeName = $"{_imgName}_SkeletonData";
        _skeName = $"{_imgName}_ske";

        var spine = SpinePool.GetPool(_folder).Get(fileName);
        if (spine != null)
        {
            spine.transform.SetParent(transform, false);
            _SetSpine(spine);
            _isLoadedSuccess = true;
            SetGray(_isGray);
            onLoaded?.Invoke(gameObject);
            return;
        }
        ResManager.LoadAssets(RESOURCE_CATEGORY.SPINE_ASSET, _skeName, false, _OnHalfLoaded);
    }
    public void SetGray(bool val)
    {
        _isGray = val;
        if (!_isLoadedSuccess)
        {
            return;
        }
        if (val)
        {
            var mat = ResManager.LoadMatSync(RESOURCE_CATEGORY.MATERIAL, "UI/UIGray");
            if (_spineObj != null)
            {
                _spineObj.material = mat;
                ResourcesManager.I.AddInstantiateObj(_spineObj.gameObject, ResourcesManager.I.GetFixAssetPath(RESOURCE_CATEGORY.MATERIAL, "UI/UIGray"));
            }
            else
            {
                img_halfbody.material = mat;
                ResourcesManager.I.AddInstantiateObj(img_halfbody.gameObject, ResourcesManager.I.GetFixAssetPath(RESOURCE_CATEGORY.MATERIAL, "UI/UIGray"));
            }
        }
        else
        {
            if (_spineObj != null)
            {
                _spineObj.material = _spineMat;
            }
            else
            {
                img_halfbody.material = null;
            }
        }
    }
}
