//MonoBehaviour lua层拓展

using System;
using System.Collections.Generic;
using System.Diagnostics;
using XLua;
using UnityEngine;

[CSharpCallLua]
public interface LuaMonoInterface
{
    void Awake();
    void Start();
    void OnDestroy();
    void OnShow(object data);
}

[LuaCallCSharp]
public class LuaMonoBehaviour : MonoBehaviour
{
    public string luaScript;
    protected string _luaMonoName;
    protected Stopwatch _watch = new Stopwatch();
    protected bool _luaLoaded = false;
    protected LuaTable _scriptEnv;
    protected LuaMonoInterface _luaMono;

    virtual protected void Awake()
    {
        _LoadLua();
    }

    virtual protected void _GetLuaMono()
    {
        _scriptEnv.Get(_luaMonoName, out _luaMono);
    }

    private bool _LoadLua()
    {
        if (_luaLoaded)
        {
            return true;
        }
        if (string.IsNullOrEmpty(luaScript))
        {
            return false;
        }
        _scriptEnv = XLuaManager.Instance.GetScriptEnv(this, luaScript, out _luaMonoName);

        if (_scriptEnv == null)
        {
            return false;
        }
        _GetLuaMono();

        try
        {
            _luaMono?.Awake();
        }
        catch (Exception e)
        {
            Logger.Log(string.Format("{0}.lua  {1}", _luaMonoName, "Awake"));
            Logger.LogError(e);
        }
        _luaLoaded = true;

        return true;
    }

    public void OnShow(object data = null)
    {
        try
        {
            _luaMono?.OnShow(data);
        }
        catch (Exception e)
        {
            Logger.Log( string.Format("{0}.lua  {1}", _luaMonoName, "OnShow"));
            Logger.LogError(e);
        }
    }

    public LuaTable GetScriptEnv()
    {
        _LoadLua();
        return _scriptEnv;
    }

    // Use this for initialization
    private void Start()
    {
        if (!_LoadLua())
        {
            return;
        }
        try
        {
            _luaMono?.Start();
        }
        catch (Exception e)
        {
            Logger.Log(string.Format("{0}.lua  {1}", _luaMonoName, "Start"));
            Logger.LogError(e);
        }
    }

    protected void OnDestroy()
    {
        try
        {
            _luaMono?.OnDestroy();
        }
        catch (Exception) 
        { 

        }
        _luaMono = null;
        _scriptEnv?.Dispose();
        _scriptEnv = null;
    }
}