using System;
using System.Collections.Generic;
using Dragon.Pool;

namespace Dragon.Events
{
    public class EventManager
    {
        public static EventManager current;
        private Dictionary<string, EventListenerData> _eventDict = new Dictionary<string, EventListenerData>();
    
        private EventListenerData _GetEvent(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                return null;
            }
            EventListenerData evt = null;
            _eventDict.TryGetValue(eventType, out evt);
            if (evt==null)
            {
                evt = DataFactory<EventListenerData>.Get();
                _eventDict.Add(eventType, evt);
            }
            return evt;
        }

        //public virtual void RemoveAllListener(string eventType)
        //{
        //    if (string.IsNullOrEmpty(eventType))
        //    {
        //        return;
        //    }
        //    _GetEvent(eventType)?.RemoveAllListeners();
        //}

        public virtual void RemoveListener(string eventType, Action callback)
        {
            var evtData = _GetEvent(eventType);
            evtData.RemoveListener(callback);
            if(evtData.count==0)
            {
                _eventDict.Remove(eventType);
                DataFactory<EventListenerData>.Release(evtData);
            }
        }

        public virtual void RemoveListener(string eventType, Action<object> callback)
        {
            var evtData = _GetEvent(eventType);
            evtData.RemoveListener(callback);
            if (evtData.count == 0)
            {
                _eventDict.Remove(eventType);
                DataFactory<EventListenerData>.Release(evtData);
            }
        }

        public virtual void RemoveListener(string eventType, Action<object, object> callback)
        {
            var evtData = _GetEvent(eventType);
            evtData.RemoveListener(callback);
            if (evtData.count == 0)
            {
                _eventDict.Remove(eventType);
                DataFactory<EventListenerData>.Release(evtData);
            }
        }

        public virtual void AddListener(string eventType, Action callback, bool once = false, string tag = null)
        {
            _GetEvent(eventType)?.AddListener(callback, once, tag);
        }

        public virtual void AddListener(string eventType, Action<object> callback, bool once = false, string tag = null)
        {
            _GetEvent(eventType)?.AddListener(callback, once, tag);
        }

        public virtual void AddListener(string eventType, Action<object, object> callback, bool once = false, string tag=null)
        {
            _GetEvent(eventType)?.AddListener(callback, once, tag);
        }
        public virtual void RemoveListenerByTag(string eventType, string tag)
        {
            if( _eventDict.ContainsKey(eventType))
            {
                _eventDict[eventType].RemoveByTag(tag);
            }
        }
        public virtual void RemoveListenerByTag(string tag)
        {
            foreach(var evtData in _eventDict.Values)
            {
                evtData.RemoveByTag(tag);
            }
        }
        //public virtual void RemoveListener(string eventType, object callback)
        //{
        //    if (callback != null)
        //    {
        //        if (callback is Action)
        //        {
        //            RemoveListener(eventType, (Action)callback);
        //        }
        //        else if (callback is Action<object>)
        //        {
        //            RemoveListener(eventType, (Action<object>)callback);
        //        }
        //        else if (callback is Action<object, object>)
        //        {
        //            RemoveListener(eventType, (Action<object, object>)callback);
        //        }
        //    }
        //}

        public virtual void Dispatch(string eventType, object param1 = null, object param2 = null)
        {
            _GetEvent(eventType)?.Invoke(param1, param2);
        }
    }
}