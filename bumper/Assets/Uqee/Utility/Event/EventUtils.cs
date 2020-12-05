using UnityEngine;
using UnityEditor;
using System;
using Uqee.Events;
using UnityEngine.EventSystems;

namespace Uqee.Utility
{
    public static class EventUtils
    {
        private static EventManager _evtMgr;
        [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitOnLoad()
        {            
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                var go = new GameObject("EventSystem");
                UnityEngine.Object.DontDestroyOnLoad(go);
                var sys = go.AddComponent<EventSystem>();
                sys.pixelDragThreshold = 5;
                var inputMod = go.AddComponent<StandaloneInputModule>();
            }
            _evtMgr = new EventManager();
            EventManager.current = _evtMgr;
        }

        public static bool IsInited()
        {
            return _evtMgr != null;
        }

        public static void RemoveListener(string eventType, Action callback)
        {
            _evtMgr.RemoveListener(eventType, callback);
        }

        public static void RemoveListener(string eventType, Action<object> callback)
        {
            _evtMgr.RemoveListener(eventType, callback);
        }

        public static void RemoveListener(string eventType, Action<object, object> callback)
        {
            _evtMgr.RemoveListener(eventType, callback);
        }

        public static void RemoveListenerByTag(string eventType, string tag)
        {
            _evtMgr.RemoveListenerByTag(eventType, tag);
        }
        public static void AddListener(string eventType, Action callback, bool once = false, string tag = null)
        {
            _evtMgr.AddListener(eventType, callback, once, tag);
        }

        public static void AddListener(string eventType, Action<object> callback, bool once = false, string tag = null)
        {
            _evtMgr.AddListener(eventType, callback, once, tag);
        }

        public static void AddListener(string eventType, Action<object, object> callback, bool once = false, string tag = null)
        {
            _evtMgr.AddListener(eventType, callback, once, tag);
        }

        public static void Dispatch(string eventType, object param1=null, object param2 = null)
        {
            _evtMgr.Dispatch(eventType, param1, param2);
        }


        #region 泛型侦听
        public static void AddListener<T>(Action<T> callback, bool once=false, string tag=null) where T : class, new()
        {
            EventWrapperManager.AddListener(callback, once, tag);
        }
        public static void Dispatch<T>(T msg) where T:class, new()
        {
            EventWrapperManager.Dispatch(msg);
        }
        public static void RemoveListener<T>(Action<T> callback) where T : class, new()
        {
            EventWrapperManager.RemoveListener(callback);
        }
        public static void RemoveListenerByTag<T>(string tag) where T : class, new()
        {
            EventWrapperManager.RemoveListenerByTag<T>(tag);
        }
        /// <summary>
        /// 只移除泛型侦听
        /// </summary>
        /// <param name="tag"></param>
        public static void RemoveListenerWrapperByTag(string tag)
        {
            EventWrapperManager.RemoveByTag(tag);
        }
        #endregion
    }
}