using System;
using UnityEngine;
using UnityEngine.UI;

public class BaseButtonItemBehaviour : MonoBehaviour
{
    private Action<int> _callback;
    private Action<object> _callback2;
    public int index;
    protected object _data;
    // Use this for initialization
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(_OnClick);
        _OnStart();
    }
    virtual protected void _OnStart()
    {

    }
    virtual protected void _OnClick()
    {
        _callback?.Invoke(index);
        _callback2?.Invoke(_data);
    }
    private void OnDestroy()
    {
        Clear();
    }
    public void Clear()
    {
        _callback = null;
        _callback2 = null;
        _data = null;
    }
    /// <summary> 数据初始化</summary>
    public void InitData(int idx, Action<int> callback)
    {
        gameObject.SetActive(true);
        index = idx;
        _callback = callback;
        _Init();
    }
    /// <summary> 数据初始化</summary>
    public void InitData(object o, Action<object> callback, int idx=-1)
    {
        gameObject.SetActive(true);
        _data= o;
        index = idx;
        _callback2 = callback;
        _Init();
    }
    virtual protected void _Init()
    {

    }
}
