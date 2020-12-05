using System;
using System.Collections.Generic;

/// <summary>
//TODO:DIMON 尽量不要用循环来遍历所有管理器的循环，在各个manager上做各个系统的迭代器，分管子系统的对象循环
//方便在后期排查性能时可以分开进行性能监测
//提高代码的可读性
//提高整体系统的容错性，可以单独关闭某个系统模块来进行测试
/// </summary>

using UnityEngine;

public enum UpdateTypeEnum
{
    UPDATE,
    FIXED_UPDATE,
    LATE_UPDATE
}

public class UpdateManager : MonoBehaviour
{
	public static bool IsValid()
	{
		return _inst != null;
	}
    private static bool _init;
    private static UpdateManager _inst;
    public static UpdateManager I {
        get
        {
            if(_inst==null)
            {
                InitOnLoad();
            }
            return _inst;
        }
    }
    [UnityEngine.RuntimeInitializeOnLoadMethod]
    private static void InitOnLoad()
    {
        if(_init || !Application.isPlaying)
        {
            return;
        }
        var go = new GameObject("UpdateManager");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<UpdateManager>();
        _init = true;
    }

    private List<Action<bool>> _pauseList = new List<Action<bool>>();
    private List<Action> _updateList = new List<Action>();
    private List<Action> _lateUpdateList = new List<Action>();
    private List<Action> _fixedUpdateList = new List<Action>();
    private List<string> _updateNameList = new List<string>();
    private List<string> _lateUpdateNameList = new List<string>();
    private List<string> _fixedUpdateNameList = new List<string>();
    protected void Awake()
	{
        AppStatus.realtimeSinceStartup = Time.realtimeSinceStartup;        
    }
    private void OnApplicationPause(bool status)
    {
        for (int i = 0; i < _pauseList.Count; i++)
        {
            _pauseList[i].Invoke(status);
        }
    }
    public void AddPauseCallback(Action<bool> callback)
    {
        _pauseList.Add(callback);
    }
    public void RemovePauseCallback(Action<bool> callback)
    {
        _pauseList.Remove(callback);
    }
    public void AddCallback(Action action, string name, UpdateTypeEnum type = UpdateTypeEnum.UPDATE)
    {
        if (action == null)
        {
            return;
        }
        switch (type)
        {
            case UpdateTypeEnum.UPDATE:
                if (!_updateList.Contains(action))
                {
                    _updateList.Add(action);
                    _updateNameList.Add(name + ".Update");
                }
                break;

            case UpdateTypeEnum.FIXED_UPDATE:
                if (!_fixedUpdateList.Contains(action))
                {
                    _fixedUpdateList.Add(action);
                    _fixedUpdateNameList.Add(name + ".FixedUpdate");
                }
                break;

            case UpdateTypeEnum.LATE_UPDATE:
                if (!_lateUpdateList.Contains(action))
                {
                    _lateUpdateList.Add(action);
                    _lateUpdateNameList.Add(name + ".LateUpdate");
                }
                break;
        }
    }

    public void RemoveCallback(Action action, UpdateTypeEnum type = UpdateTypeEnum.UPDATE)
    {
        if (action == null)
        {
            return;
        }
        List<Action> list = null;
        List<string> nameList = null;
        switch (type)
        {
            case UpdateTypeEnum.UPDATE:
                list = _updateList;
                nameList = _updateNameList;
                break;

            case UpdateTypeEnum.FIXED_UPDATE:
                list = _fixedUpdateList;
                nameList = _fixedUpdateNameList;
                break;

            case UpdateTypeEnum.LATE_UPDATE:
                list = _lateUpdateList;
                nameList = _lateUpdateNameList;
                break;
        }
        for(int i=0; i<list.Count; i++)
        {
            if(list[i]==action)
            {
                list[i] = null;
                nameList[i] = null;
                break;
            }
        }
    }
    List<int> _removeList = new List<int>();
    private void _Invoke(List<Action> list, List<string> nameList)
    {
        var cnt = list.Count;
        
        for (int i = 0; i < cnt; i++)
        {
            if(list[i]==null)
            {
                continue;
            }
            UnityEngine.Profiling.Profiler.BeginSample(nameList[i]);
            list[i]();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        for(int i=cnt-1; i>=0; i--)
        {
            if(list[i]==null)
            {
                list.RemoveAt(i);
                nameList.RemoveAt(i);
            }
        }
        _removeList.Clear();
    }
    private void FixedUpdate()
    {
        _Invoke(_fixedUpdateList, _fixedUpdateNameList);
    }

    private void LateUpdate()
    {
        _Invoke(_lateUpdateList, _lateUpdateNameList);
    }

    private void Update()
    {
        AppStatus.realtimeSinceStartup = Time.realtimeSinceStartup;
        _Invoke(_updateList, _updateNameList);
    }
}