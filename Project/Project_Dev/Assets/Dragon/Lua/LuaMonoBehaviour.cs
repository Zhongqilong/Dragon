/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using XLua;
using UnityEngine;

[CSharpCallLua]
public interface LuaMonoInterface
{
    void Awake(object obj);
    void Start();
    void FixUpdate();
    void Update();
    void OnDestroy();
    void OnEnable();
    void OnDisable();
    void SetView(object view);
}

[LuaCallCSharp]
public class LuaMonoBehaviour : MonoBehaviour 
{
    public string luaScript;
    protected string _luaMonoName;
    protected bool _luaLoaded = false;
    protected object _showData;
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

    virtual protected void _InitEnv()
    {
        _scriptEnv = LuaExtensions.GetScriptEnv(this, luaScript, out _luaMonoName);
    }

    private bool _LoadLua()
    {
        if (_luaLoaded)
        {
            return true;
        }
        if (string.IsNullOrEmpty(luaScript))
        {
            Dragon.Debug.Error(_luaMonoName + " lua script not set");
            return false;
        }
        _InitEnv();
        if (_scriptEnv == null)
        {
            return false;
        }
        _GetLuaMono();

        _luaMono?.SetView(this);
        _luaMono?.Awake(_showData);
        _luaLoaded = true;

        return true;
    }

    public LuaTable GetScriptEnv()
    {
        _LoadLua();
        return _scriptEnv;
    }

    private void Start()
    {
        if (!_LoadLua())
        {
            return;
        }
        _luaMono?.Start();
    }

    protected void OnDestroy()
    {   
        _luaMono?.OnDestroy();
        _luaMono = null;
        _scriptEnv?.Dispose();
        _scriptEnv = null;
    }
}