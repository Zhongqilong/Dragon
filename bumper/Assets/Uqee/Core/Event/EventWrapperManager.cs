
using System;
using System.Collections.Generic;

namespace Uqee.Events
{
    public static class EventWrapperManager
    {
        private static List<IListenerData> _list = new List<IListenerData>(100);
        public static void Register(IListenerData data)
        {
            _list.Add(data);
        }
        public static void RemoveByTag(string tag)
        {
            int cnt = _list.Count;
            for (int i = 0; i < cnt; i++)
            {
                _list[i].RemoveByTag(tag);
            }
        }

        public static void AddListener<T>(Action<T> callback, bool once = false, string tag = null)
        {
            var eventType = typeof(T).Name;
            OneParamEventWrapper<T>.AddListener(callback, once, tag);
        }
        public static void Dispatch<T>(T msg) where T : class, new()
        {
            OneParamEventWrapper<T>.Dispatch(msg);
        }
        public static void RemoveListener<T>(Action<T> callback)
        {
            OneParamEventWrapper<T>.RemoveListener(callback);
        }
        public static void RemoveListenerByTag<T>(string tag)
        {
            OneParamEventWrapper<T>.RemoveByTag(tag);
        }
    }
}