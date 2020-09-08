using UnityEngine;
using UnityEditor;

namespace Uqee.Resource
{
    public static class CDNSetting
    {
        public static bool isCDNReady = true;
        public static bool useCDN = true;
        private static string[] _cdnArr;
        private static int _cdnIdx = -1;
        private static string _currCDN;
        public static void SetHost(string[] arr)
        {
            _cdnArr = arr;
        }
        public static bool NextCDN()
        {
            if (_cdnArr == null || _cdnArr.Length == 0)
            {
                return false;
            }
            bool hasNext = true;
            _cdnIdx++;
            if (_cdnIdx >= _cdnArr.Length)
            {
                _cdnIdx = 0;
                hasNext = false;
            }
            _currCDN = string.Format("{0}/{1}/", _cdnArr[_cdnIdx], osName);
            return hasNext;
        }
        private static string osName
        {
            get
            {
#if UNITY_ANDROID
                return "android";
#elif UNITY_IPHONE || UNITY_IOS
            return "ios";
#else
            return "android";
#endif
            }
        }
        public static int cdnSize
        {
            get
            {
                if (_cdnArr == null)
                {
                    return 0;
                }
                return _cdnArr.Length;
            }
        }
        public static string currCDN
        {
            get
            {
                if (_cdnIdx < 0)
                {
                    NextCDN();
                }

                return _currCDN;
            }
        }
    }
}