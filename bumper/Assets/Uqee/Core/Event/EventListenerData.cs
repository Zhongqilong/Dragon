using System.Collections.Generic;
using System;

namespace Uqee.Events
{
    /// <summary>
    /// 此类是添加事件，移除事件，Invoke事件的处理类，目前只设置三种情况，有需求可以自己加（无参，一个参数，两个参数）
    /// </summary>
    public class EventListenerData
    {
        /// <summary>
        /// 单个参数的监听设置
        /// </summary>
        /// <typeparam name="Action"></typeparam>
        /// <returns></returns>
        private List<Action> _noneParamListenerList = new List<Action>();
        private List<bool> _noneParamOnceListenerList = new List<bool>(50);
        private List<string> _noneParamTagListenerList = new List<string>(50);
        private SortedSet<int> _noneParamRemoveList = new SortedSet<int>(SortBase.I);

        private List<Action<object>> _oneParamListenerList = new List<Action<object>>();
        private List<bool> _oneParamOnceListenerList = new List<bool>(50);
        private List<string> _oneParamTagListenerList = new List<string>(50);
        private SortedSet<int> _oneParamRemoveList = new SortedSet<int>(SortBase.I);

        private List<Action<object, object>> _twoParamListenerList = new List<Action<object, object>>();
        private List<bool> _twoParamOnceListenerList = new List<bool>(50);
        private List<string> _twoParamTagListenerList = new List<string>(50);
        private SortedSet<int> _twoParamRemoveList = new SortedSet<int>(SortBase.I);

        //是否需要执行
        private bool _invoking;
        //当前监听数量
        public int count {get {return _noneParamOnceListenerList.Count + _oneParamListenerList.Count + _twoParamListenerList.Count;}}

        public void AddListener(Action callback, bool once, string tag=null)
        {
            if (callback == null || _noneParamListenerList.Contains(callback)) return;
            _noneParamListenerList.Add(callback);
            _noneParamOnceListenerList.Add(once);
            _noneParamTagListenerList.Add(tag);
        }
        public void AddListener(Action<object> callback, bool once, string tag = null)
        {
            if (callback == null || _oneParamListenerList.Contains(callback)) return;
            _oneParamListenerList.Add(callback);
            _oneParamOnceListenerList.Add(once);
            _oneParamTagListenerList.Add(tag);
        }
        public void AddListener(Action<object, object> callback, bool once, string tag = null)
        {
            if (callback == null || _twoParamListenerList.Contains(callback)) return;
            _twoParamListenerList.Add(callback);
            _twoParamOnceListenerList.Add(once);
            _twoParamTagListenerList.Add(tag);
        }

                public void RemoveByTag(string tag)
        {
            for(int i=_noneParamTagListenerList.Count-1; i>=0; i--)
            {
                if(_noneParamTagListenerList[i]==tag)
                {
                    if (_invoking)
                    {
                        if (!_noneParamRemoveList.Contains(i))
                        {
                            _noneParamRemoveList.Add(i);
                        }
                    }
                    else
                    {
                        _noneParamListenerList.RemoveAt(i);
                        _noneParamOnceListenerList.RemoveAt(i);
                        _noneParamTagListenerList.RemoveAt(i);
                    }
                }
            }
            for (int i = _oneParamTagListenerList.Count - 1; i >= 0; i--)
            {
                if (_oneParamTagListenerList[i] == tag)
                {
                    if (_invoking)
                    {
                        if (!_oneParamRemoveList.Contains(i))
                        {
                            _oneParamRemoveList.Add(i);
                        }
                    }
                    else
                    {
                        _oneParamListenerList.RemoveAt(i);
                        _oneParamOnceListenerList.RemoveAt(i);
                        _oneParamTagListenerList.RemoveAt(i);
                    }
                }
            }
            for (int i = _twoParamTagListenerList.Count - 1; i >= 0; i--)
            {
                if (_twoParamTagListenerList[i] == tag)
                {
                    if (_invoking)
                    {
                        if (!_twoParamRemoveList.Contains(i))
                        {
                            _twoParamRemoveList.Add(i);
                        }
                    }
                    else
                    {
                        _twoParamListenerList.RemoveAt(i);
                        _twoParamOnceListenerList.RemoveAt(i);
                        _twoParamTagListenerList.RemoveAt(i);
                    }
                }
            }
        }
        public void RemoveListener(Action callback)
        {
            if (callback == null) return;
            for (int i = _noneParamListenerList.Count - 1; i >= 0; i--)
            {
                if (_noneParamListenerList[i] == callback)
                {
                    if (_invoking)
                    {
                        if (!_noneParamRemoveList.Contains(i))
                        {
                            _noneParamRemoveList.Add(i);
                        }
                    }
                    else
                    {
                        _noneParamListenerList.RemoveAt(i);
                        _noneParamOnceListenerList.RemoveAt(i);
                        _noneParamTagListenerList.RemoveAt(i);
                    }
                    break;
                }
            }
        }
        public void RemoveListener(Action<object> callback)
        {
            if (callback == null) return;
            for (int i = _oneParamListenerList.Count - 1; i >= 0; i--)
            {
                if (_oneParamListenerList[i] == callback)
                {
                    if (_invoking)
                    {
                        if (!_oneParamRemoveList.Contains(i))
                        {
                            _oneParamRemoveList.Add(i);
                        }
                    }
                    else
                    {
                        _oneParamListenerList.RemoveAt(i);
                        _oneParamOnceListenerList.RemoveAt(i);
                        _oneParamTagListenerList.RemoveAt(i);
                    }
                    break;
                }
            }
        }
        public void RemoveListener(Action<object, object> callback)
        {
            if (callback == null) return;
            for (int i = _twoParamListenerList.Count - 1; i >= 0; i--)
            {
                if (_twoParamListenerList[i] == callback)
                {
                    if (_invoking)
                    {
                        if (!_twoParamRemoveList.Contains(i))
                        {
                            _twoParamRemoveList.Add(i);
                        }
                    }
                    else
                    {
                        _twoParamListenerList.RemoveAt(i);
                        _twoParamOnceListenerList.RemoveAt(i);
                        _twoParamTagListenerList.RemoveAt(i);
                    }
                    break;
                }
            }
        }

        public void Invoke(object p1, object p2)
        {
            _invoking = true;
            try
            {
                for (int i = 0; i < _noneParamListenerList.Count; i++)
                {
                    var callback = _noneParamListenerList[i];
                    if (_noneParamRemoveList.Contains(i))
                    {
                        continue;
                    }
                    if (_noneParamOnceListenerList[i])
                    {
                        _noneParamRemoveList.Add(i);
                    }
                    callback.Invoke();
                }
                for (int i = 0; i < _oneParamListenerList.Count; i++)
                {
                    var callback = _oneParamListenerList[i];
                    if (_oneParamRemoveList.Contains(i))
                    {
                        continue;
                    }
                    if (_oneParamOnceListenerList[i])
                    {
                        _oneParamRemoveList.Add(i);
                    }
                    callback.Invoke(p1);
                }
                for (int i = 0; i < _twoParamListenerList.Count; i++)
                {
                    var callback = _twoParamListenerList[i];
                    if (_twoParamRemoveList.Contains(i))
                    {
                        continue;
                    }
                    if (_twoParamOnceListenerList[i])
                    {
                        _twoParamRemoveList.Add(i);
                    }
                    callback.Invoke(p1, p2);
                }
            }
            catch (Exception ex)            
            {
                UnityEngine.Debug.LogError(ex);
            }
            foreach(var idx in _noneParamRemoveList)
            {
                _noneParamListenerList.RemoveAt(idx);
                _noneParamOnceListenerList.RemoveAt(idx);
                _noneParamTagListenerList.RemoveAt(idx);
            }
            foreach (var idx in _oneParamRemoveList)
            {
                _oneParamListenerList.RemoveAt(idx);
                _oneParamOnceListenerList.RemoveAt(idx);
                _oneParamTagListenerList.RemoveAt(idx);
            }
            foreach (var idx in _twoParamRemoveList)
            {
                _twoParamListenerList.RemoveAt(idx);
                _twoParamOnceListenerList.RemoveAt(idx);
                _twoParamTagListenerList.RemoveAt(idx);
            }
            _noneParamRemoveList.Clear();
            _oneParamRemoveList.Clear();
            _twoParamRemoveList.Clear();

            _invoking = false;
        }
    }
}