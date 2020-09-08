using System.Collections.Generic;

namespace Uqee.Resource
{
    public static class CacheSetting
    {
        private static volatile int _stopAutoRelease;
        /// <summary>
        /// 暂停缓存自动回收，当值为true时，自动回收停止
        /// </summary>
        public static bool stopAutoRelease
        {
            get
            {
                return _stopAutoRelease > 0;
            }
            set
            {
                if (value)
                {
                    _stopAutoRelease++;
                }
                else
                {
                    _stopAutoRelease--;
                }
            }
        }
        private static Dictionary<string, bool> _donotReleaseDict = new Dictionary<string, bool>();
        public static void ClearDonotRelease()
        {
            _donotReleaseDict.Clear();
        }
        public static void AddDonotRelease(string key)
        {
            _donotReleaseDict[key.ToLower()] = true;
        }
        public static void RemoveDonotRelease(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                return;
            }
            _donotReleaseDict.Remove(key.ToLower());
        }
        public static bool IsDonotRelease(string key)
        {
            return _donotReleaseDict.ContainsKey(key.ToLower());
        }
    }
}