using System.Collections.Generic;
using System;

namespace Uqee.Events
{
    public static class OneParamEventWrapper<T>
    {
        private static OneParamListenerData<T> _listenerData;
        private static OneParamListenerData<T> listenerData
        {
            get
            {
                if (_listenerData == null)
                {
                    _listenerData = new OneParamListenerData<T>();
                    EventWrapperManager.Register(_listenerData);
                }

                return _listenerData;
            }
        }

        public static void RemoveListener(Action<T> callback)
        {
            listenerData.RemoveListener(callback);
        }
        public static void AddListener(Action<T> callback, bool once, string tag)
        {
            listenerData.AddListener(callback, once, tag);
        }
        public static void Dispatch(T p1)
        {
            try
            {
                listenerData.Invoke(p1);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        public static void RemoveByTag(string tag)
        {
            listenerData.RemoveByTag(tag);
        }
    }
    public static class TwoParamEventWrapper<T1, T2>
    {
        private static TwoParamListenerData<T1, T2> _listenerData;
        private static TwoParamListenerData<T1, T2> listenerData
        {
            get
            {
                if (_listenerData == null)
                {
                    _listenerData = new TwoParamListenerData<T1, T2>();
                    EventWrapperManager.Register(_listenerData);
                }

                return _listenerData;
            }
        }

        public static void RemoveListener(Action<T1, T2> callback)
        {
            listenerData.RemoveListener(callback);
        }
        public static void AddListener(Action<T1, T2> callback, bool once, string tag)
        {
            listenerData.AddListener(callback, once, tag);
        }
        public static void Dispatch(T1 p1, T2 p2)
        {
            listenerData.Invoke(p1, p2);
        }

        public static void RemoveByTag(string tag)
        {
            listenerData.RemoveByTag(tag);
        }
    }
    public static class ThreeParamEventWrapper<T1, T2, T3>
    {
        private static ThreeParamListenerData<T1, T2, T3> _listenerData;
        private static ThreeParamListenerData<T1, T2, T3> listenerData
        {
            get
            {
                if (_listenerData == null)
                {
                    _listenerData = new ThreeParamListenerData<T1, T2, T3>();
                    EventWrapperManager.Register(_listenerData);
                }

                return _listenerData;
            }
        }

        public static void RemoveListener(Action<T1, T2, T3> callback)
        {
            listenerData.RemoveListener(callback);
        }
        public static void AddListener(Action<T1, T2, T3> callback, bool once, string tag)
        {
            listenerData.AddListener(callback, once, tag);
        }
        public static void Dispatch(T1 p1, T2 p2, T3 p3)
        {
            listenerData.Invoke(p1, p2, p3);
        }

        public static void RemoveByTag(string tag)
        {
            listenerData.RemoveByTag(tag);
        }
    }
    class OneParamListenerData<T> : AbstractListenerData
    {
        private List<Action<T>> _listenerList = new List<Action<T>>();

        protected override void _RemoveAt(int i)
        {
            _listenerList.RemoveAt(i);
            base._RemoveAt(i);
        }

        public void RemoveListener(Action<T> callback)
        {
            if (callback == null) return;
            for (int i = _listenerList.Count - 1; i >= 0; i--)
            {
                if (_listenerList[i] == callback)
                {
                    _CheckRemove(i);
                    break;
                }
            }
        }

        public void AddListener(Action<T> callback, bool once, string tag)
        {
            if (callback == null || _listenerList.Contains(callback)) return;
            _listenerList.Add(callback);
            _Add(once, tag);
        }
        public void Invoke(T p1)
        {
            _InvokeBegin();
            for (int i = 0; i < _listenerList.Count; i++)
            {
                var callback = _listenerList[i];
                if (_CanInvoke(i))
                {
                    callback.Invoke(p1);
                }
            }

            _InvokeEnd();
        }
    }
    class TwoParamListenerData<T1, T2> : AbstractListenerData
    {
        private List<Action<T1, T2>> _listenerList = new List<Action<T1, T2>>();


        protected override void _RemoveAt(int i)
        {
            _listenerList.RemoveAt(i);
            base._RemoveAt(i);
        }

        public void RemoveListener(Action<T1, T2> callback)
        {
            if (callback == null) return;
            for (int i = _listenerList.Count - 1; i >= 0; i--)
            {
                if (_listenerList[i] == callback)
                {
                    _CheckRemove(i);
                    break;
                }
            }
        }

        public void AddListener(Action<T1, T2> callback, bool once, string tag)
        {
            if (callback == null || _listenerList.Contains(callback)) return;
            _listenerList.Add(callback);
            _Add(once, tag);
        }
        public void Invoke(T1 p1, T2 p2)
        {
            _InvokeBegin();
            for (int i = 0; i < _listenerList.Count; i++)
            {
                var callback = _listenerList[i];
                if (_CanInvoke(i))
                {
                    callback.Invoke(p1, p2);
                }
            }

            _InvokeEnd();
        }
    }
    class ThreeParamListenerData<T1, T2, T3> : AbstractListenerData
    {
        private List<Action<T1, T2, T3>> _listenerList = new List<Action<T1, T2, T3>>();

        protected override void _RemoveAt(int i)
        {
            _listenerList.RemoveAt(i);
            base._RemoveAt(i);
        }

        public void RemoveListener(Action<T1, T2, T3> callback)
        {
            if (callback == null) return;
            for (int i = _listenerList.Count - 1; i >= 0; i--)
            {
                if (_listenerList[i] == callback)
                {
                    _CheckRemove(i);
                    break;
                }
            }
        }

        public void AddListener(Action<T1, T2, T3> callback, bool once, string tag)
        {
            if (callback == null || _listenerList.Contains(callback)) return;
            _listenerList.Add(callback);
            _Add(once, tag);
        }
        public void Invoke(T1 p1, T2 p2, T3 p3)
        {
            _InvokeBegin();
            for (int i = 0; i < _listenerList.Count; i++)
            {
                var callback = _listenerList[i];
                if (_CanInvoke(i))
                {
                    callback.Invoke(p1, p2, p3);
                }
            }

            _InvokeEnd();
        }
    }
}