using System;
using System.Collections.Generic;
using UnityEngine;
using Uqee.Resource;

public class EffectContainer : MonoBehaviour
{
    internal class RemoveItem
    {
        public GameObject target;
        public float time;
        public RemoveItem(GameObject target, float time)
        {
            this.target = target;
            this.time = time;
        }
    }
    public bool isXsj = true;
    public string url;
    public float duration;
    public bool isLoop;
    //保证只有一个.
    public bool single = true;

    public string category
    {
        get
        {
            return isXsj ? RESOURCE_CATEGORY.FX_XSJ : RESOURCE_CATEGORY.FX;
        }
    }

    //private List<GameObject> toHideList = new List<GameObject>();
    private Queue<RemoveItem> hideDict = new Queue<RemoveItem>();
    private Action<Transform> onLoadCallBack;
    private bool IsPreLoad { get; set; }
    private List<string> loadingList = new List<string>(4);
    public void preLoad()
    {
        if (transform.childCount>0)
        {
            return;
        }
        if (IsPreLoad)
            return;
        IsPreLoad = true;
        if (isActiveAndEnabled) {
            loadingList.Add(url);
            ResManager.LoadAndInst(category, url, transform, false, _OnPreLoadEnd);
        }
    }

    private void OnEnable()
    {
        IsPreLoad = false;
    }

    private void OnDisable()
    {
        IsPreLoad = false;
    }

    public void Show(Action<Transform> onLoadCallBack = null)
    {
        if (transform.childCount>0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var subTfm = transform.GetChild(i).gameObject;
                if (!subTfm.activeSelf)
                {
                    subTfm.SetActive(true);
                    onLoadCallBack?.Invoke(subTfm.transform);
                    if (!isLoop)
                    {
                        AddToHide(subTfm, duration);
                    }
                    return;
                }
            }

            if (!single)
            {
                var fisttChild = transform.GetChild(0);
                var subObj = GameObject.Instantiate(fisttChild.gameObject,transform);
                subObj.transform.localPosition = fisttChild.localPosition;
                subObj.transform.localScale = fisttChild.localScale;
                subObj.gameObject.SetActive(false);
                var animator = subObj.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.Rebind();
                }
                subObj.gameObject.SetActive(true);
                onLoadCallBack?.Invoke(subObj.transform);
                if (!isLoop)
                {
                    AddToHide(subObj, duration);
                }
                return;
            }
            else
            {
                var subObj = transform.GetChild(0).gameObject;

                var animator = subObj.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.Rebind();
                }
                subObj.gameObject.SetActive(true);
                onLoadCallBack?.Invoke(subObj.transform);
                if (!isLoop)
                {
                    ResetOnHide(subObj, Time.realtimeSinceStartup + duration);
                }
            }
        }
        this.onLoadCallBack = onLoadCallBack;
        if (loadingList.Contains(url))
            return;

        if (isActiveAndEnabled)
        {
            loadingList.Add(url);
            ResManager.LoadAndInst(category, url, transform, false, _OnLoadEnd);
        }
        else if(UIManager.IsValid())
        {
            loadingList.Add(url);
            ResManager.LoadAndInst(category, url, transform, false, _OnLoadEnd);
        }
    }

    public void Hide()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var subTfm = transform.GetChild(i);
            if (subTfm.gameObject.activeSelf)
            {
                subTfm.gameObject.SetActive(false);
            }
        }
    }

    private void _OnPreLoadEnd(InstantiateRequest req)
    {
        IsPreLoad = false;
        if (req.loadedObj != null)
        {
            var gObj = req.loadedObj as GameObject;
            gObj.SetActive(false);
            AddToOnDestroy(gObj);
        }
    }

    private void _OnPreLoadErr(InstantiateRequest req)
    {
        IsPreLoad = false;
        Uqee.Debug.Log( "_OnPreLoadErr:" + req?.assetName);
    }

    private void _OnLoadEnd(InstantiateRequest req)
    {
        var gObj = req.loadedObj as GameObject;
        onLoadCallBack?.Invoke(gObj?.transform);
        AddToOnDestroy(gObj);
        if(!isLoop)
            AddToHide(gObj,duration);
    }

    private void _OnLoadErr(InstantiateRequest req)
    {
        Uqee.Debug.Log( "_OnPreLoadErr:" + req?.assetName);
    }

    private uint clearId = 0;
    private void AddToHide(GameObject gObj,float duration)
    {
        hideDict.Enqueue(new RemoveItem(gObj, Time.realtimeSinceStartup + duration));
        if (clearId == 0)
        {
            clearId = JobScheduler.I.SetInterval(OnPopHide, 0.1f);
        }
    }

    /// <summary>
    /// 虽然不准确..但是没办法;;
    /// </summary>
    /// <param name="gObj"></param>
    /// <param name="duration"></param>
    private void ResetOnHide(GameObject gObj,float duration)
    {
        var d = default(RemoveItem);
        foreach(var itm in hideDict)
        {
            if (itm.target == gObj)
            {
                d = itm;
                d.time = duration;
            }
        }
    }

    private void OnPopHide()
    {
        if (hideDict.Count > 0)
        {
            var pair = hideDict.Peek();
            if (Time.realtimeSinceStartup > pair.time)
            {
                pair = hideDict.Dequeue();
                if (pair.target != null)
                {
                    pair.target.SetActive(false);
                }
            }
        }
        else
        {
            JobScheduler.I.ClearTimer(clearId);
            clearId = 0;
        }
    }

    #region OnDestroyAsset
    private List<UnityEngine.Object> _onDestroyList = new List<UnityEngine.Object>();
    public void AddToOnDestroy(UnityEngine.Object obj)
    {
        _onDestroyList.Add(obj);
    }

    private void ApplyOnDestroy()
    {
        foreach (var item in _onDestroyList)
        {
            if (item != null)
            {
                GameObject.Destroy(item);
            }
        }
        _onDestroyList.Clear();
    }
    #endregion

    private void OnDestroy()
    {
        if(clearId!=0 && JobScheduler.IsValid())
            JobScheduler.I.ClearTimer(clearId);
        StopAllCoroutines();
        hideDict.Clear();
        ApplyOnDestroy();
    }
}