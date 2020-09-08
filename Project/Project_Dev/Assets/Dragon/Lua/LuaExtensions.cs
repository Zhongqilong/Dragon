using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using XLua;

public partial class LuaExtensions
{    
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
        const string LUADLL = "__Internal";
#else
    private const string LUADLL = "xlua";
#endif
    public static bool luaAsSystemAssets = false;

    public delegate int lua_CSFunction(IntPtr L);

    public static LuaFunction setInstance;
    public static LuaFunction setGOInstance;

    private static LuaEnv _luaenv;

    public static LuaEnv luaEnv
    {
        get
        {
            if (_luaenv == null)
            {
                _luaenv = new LuaEnv();
                _luaenv.AddBuildin("lpeg", LoadLpeg);
                _luaenv.AddBuildin("rapidjson", LoadRapidJson);
                _luaenv.AddLoader(_LuaLoader);
            }
            return _luaenv;
        }
    }
    
    private static byte[] _LuaLoader(ref string filename)
    {
        var path = filename.ReplaceChar('.', '/');
        return ResUtils.LoadLuaSync($"{path}.lua");
    }

    public static LuaTable GetScriptEnv(MonoBehaviour view, string luaScript, out string monoName)
    {
        if (string.IsNullOrEmpty(luaScript))
        {
            monoName = string.Empty;
            return null;
        }
        var _scriptEnv = LuaExtensions.luaEnv.NewTable();

        LuaTable meta = LuaExtensions.luaEnv.NewTable();
        meta.Set("__index", LuaExtensions.luaEnv.Global);
        _scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        monoName = Path.GetFileNameWithoutExtension(luaScript);

        var luaAsset = ResUtils.LoadLuaSync(luaScript);
        if (luaAsset == null)
        {
            return null;
        }

        var tempArr = luaScript.Split('/');
        var fileName = tempArr[tempArr.Length - 1];
        //LuaExtensions.luaEnv.DoString(luaAsset, "LuaBehaviour", _scriptEnv);
        
        LuaExtensions.luaEnv.DoString(luaAsset, fileName, _scriptEnv);
        return _scriptEnv;
    }


    [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaopen_lpeg(System.IntPtr L);

    [MonoPInvokeCallback(typeof(lua_CSFunction))]
    public static int LoadLpeg(System.IntPtr L)
    {
        return luaopen_lpeg(L);
    }

    [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaopen_rapidjson(System.IntPtr L);

    [MonoPInvokeCallback(typeof(lua_CSFunction))]
    public static int LoadRapidJson(System.IntPtr L)
    {
        return luaopen_rapidjson(L);
    }
}