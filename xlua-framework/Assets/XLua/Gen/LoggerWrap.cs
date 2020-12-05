#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class LoggerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(Logger);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 9, 9, 7);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "Log", _m_Log_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LogToMainThread", _m_LogToMainThread_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Assert", _m_Assert_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LogError", _m_LogError_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LogErrorToMainThread", _m_LogErrorToMainThread_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LogStackTrace", _m_LogStackTrace_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckReportError", _m_CheckReportError_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Watch", _m_Watch_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "useTime", _g_get_useTime);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "useMemory", _g_get_useMemory);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "clientVerstion", _g_get_clientVerstion);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "loginUid", _g_get_loginUid);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "localIP", _g_get_localIP);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "platName", _g_get_platName);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "sceneName", _g_get_sceneName);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "DEBUG_BUILD_VER", _g_get_DEBUG_BUILD_VER);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "platChannel", _g_get_platChannel);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "clientVerstion", _s_set_clientVerstion);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "loginUid", _s_set_loginUid);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "localIP", _s_set_localIP);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "platName", _s_set_platName);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "sceneName", _s_set_sceneName);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "DEBUG_BUILD_VER", _s_set_DEBUG_BUILD_VER);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "platChannel", _s_set_platChannel);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new Logger();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to Logger constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Log_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<object>(L, 1)) 
                {
                    object _o = translator.GetObject(L, 1, typeof(object));
                    
                    Logger.Log( _o );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count >= 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaTypes.LUA_TNONE == LuaAPI.lua_type(L, 2) || translator.Assignable<object>(L, 2))) 
                {
                    string _s = LuaAPI.lua_tostring(L, 1);
                    object[] _p = translator.GetParams<object>(L, 2);
                    
                    Logger.Log( _s, _p );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to Logger.Log!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LogToMainThread_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _s = LuaAPI.lua_tostring(L, 1);
                    object[] _p = translator.GetParams<object>(L, 2);
                    
                    Logger.LogToMainThread( _s, _p );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Assert_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    bool _condition = LuaAPI.lua_toboolean(L, 1);
                    string _s = LuaAPI.lua_tostring(L, 2);
                    object[] _p = translator.GetParams<object>(L, 3);
                    
                    Logger.Assert( _condition, _s, _p );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LogError_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<object>(L, 1)) 
                {
                    object _o = translator.GetObject(L, 1, typeof(object));
                    
                    Logger.LogError( _o );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count >= 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaTypes.LUA_TNONE == LuaAPI.lua_type(L, 2) || translator.Assignable<object>(L, 2))) 
                {
                    string _s = LuaAPI.lua_tostring(L, 1);
                    object[] _p = translator.GetParams<object>(L, 2);
                    
                    Logger.LogError( _s, _p );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to Logger.LogError!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LogErrorToMainThread_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _s = LuaAPI.lua_tostring(L, 1);
                    object[] _p = translator.GetParams<object>(L, 2);
                    
                    Logger.LogErrorToMainThread( _s, _p );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LogStackTrace_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _str = LuaAPI.lua_tostring(L, 1);
                    
                    Logger.LogStackTrace( _str );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckReportError_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    Logger.CheckReportError(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Watch_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    Logger.Watch(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_useTime(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushint64(L, Logger.useTime);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_useMemory(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.useMemory);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_clientVerstion(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.clientVerstion);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_loginUid(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.loginUid);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_localIP(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.localIP);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_platName(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.platName);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_sceneName(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.sceneName);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_DEBUG_BUILD_VER(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.DEBUG_BUILD_VER);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_platChannel(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, Logger.platChannel);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_clientVerstion(RealStatePtr L)
        {
		    try {
                
			    Logger.clientVerstion = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_loginUid(RealStatePtr L)
        {
		    try {
                
			    Logger.loginUid = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_localIP(RealStatePtr L)
        {
		    try {
                
			    Logger.localIP = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_platName(RealStatePtr L)
        {
		    try {
                
			    Logger.platName = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_sceneName(RealStatePtr L)
        {
		    try {
                
			    Logger.sceneName = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_DEBUG_BUILD_VER(RealStatePtr L)
        {
		    try {
                
			    Logger.DEBUG_BUILD_VER = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_platChannel(RealStatePtr L)
        {
		    try {
                
			    Logger.platChannel = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
