using System.Collections.Generic;
using System;

namespace Dragon.Events
{
    public class EventListenerData
    {
        private List<Action> _noneParamListenerList = new List<Action>();
        private List<Action<object>> _oneParamListenerList = new List<Action<object>>();
        private List<Action<object, object>> _twoParamListenerList = new List<Action<object, object>>();

        private List<bool> _noneParamOnceListenerList = new List<bool>(50);
        private List<bool> _oneParamOnceListenerList = new List<bool>(50);
        private List<bool> _twoParamOnceListenerList = new List<bool>(50);

        private List<string> _noneParamTagListenerList = new List<string>(50);
        private List<string> _oneParamTagListenerList = new List<string>(50);
        private List<string> _twoParamTagListenerList = new List<string>(50);
        
        private SortedSet<int> _noneParamRemoveList = new SortedSet<int>(IdxSorter.I);
        private SortedSet<int> _oneParamRemoveList = new SortedSet<int>(IdxSorter.I);
        private SortedSet<int> _twoParamRemoveList = new SortedSet<int>(IdxSorter.I);

        private bool _invoking;

        //public void RemoveAllListeners()
        //{
        //    _noneParamListenerList.Clear();
        //    _oneParamListenerList.Clear();
        //    _twoParamListenerList.Clear();

        //    _noneParamOnceListenerList.Clear();
        //    _oneParamOnceListenerList.Clear();
        //    _twoParamOnceListenerList.Clear();

        //    _noneParamTagListenerList.Clear();
        //    _oneParamTagListenerList.Clear();
        //    _twoParamTagListenerList.Clear();
        //}
        public int count
        {
            get
            {
                return _noneParamListenerList.Count + _oneParamListenerList.Count + _twoParamListenerList.Count;
            }
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
                Dragon.Debug.Error(ex);
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