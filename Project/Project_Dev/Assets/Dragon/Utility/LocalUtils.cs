using System.Collections.Generic;

namespace Uqee.Utility
{

    public static class LocalUtils
    {

        private static Dictionary<string, string> _dict = new Dictionary<string, string>();

        public static void Init(KeyValueData obj)
        {
            _dict.Clear();
            for (var i = obj.keys.Length - 1; i >= 0; i--)
            {
                _dict[obj.keys[i]] = obj.values[i];
            }
        }

        public static string GetValue(string key)
        {
            if (_dict == null)
            {
                return key;
            }
            if (!_dict.ContainsKey(key))
            {
                return key;
            }
            var txt = _dict[key];
            return txt;
        }

        public static string GetValue(string key, object arg0)
        {
            if (_dict == null)
            {
                return string.Empty;
            }
            if (!_dict.ContainsKey(key))
            {
                return string.Empty;
            }
            var txt = _dict[key];
            try
            {
                if (string.IsNullOrEmpty(txt))
                {
                    return key;
                }
                return string.Format(txt, arg0);
            }
            catch (System.Exception ex)
            {
                Uqee.Debug.LogError($"格式化失败，参数长度不正确.args.len=1, text={txt}");
                Uqee.Debug.LogError(ex);
            }
            return txt;
        }

        public static string GetValue(string key, object arg0, object arg1)
        {
            if (_dict == null || !_dict.ContainsKey(key))
            {
                return string.Empty;
            }
            var txt = _dict[key];
            try
            {
                return string.Format(txt, arg0, arg1);
            }
            catch (System.Exception ex)
            {
                Uqee.Debug.LogError($"格式化失败，参数长度不正确.args.len=2, text={txt}");
                Uqee.Debug.LogError(ex);
            }
            return txt;
        }

        public static string GetValue(string key, object arg0, object arg1, object arg2)
        {
            if (_dict == null || !_dict.ContainsKey(key))
            {
                return string.Empty;
            }
            var txt = _dict[key];
            try
            {
                return string.Format(txt, arg0, arg1, arg2);
            }
            catch (System.Exception ex)
            {
                Uqee.Debug.LogError($"格式化失败，参数长度不正确.args.len=3, text={txt}");
                Uqee.Debug.LogError(ex);
            }
            return txt;
        }

        public static string GetValue(string key, params object[] args)
        {
            if (_dict == null || !_dict.ContainsKey(key))
            {
                return string.Empty;
            }
            var txt = _dict[key];
            if (args.Length > 0)
            {
                try
                {
                    return string.Format(txt, args);
                }
                catch (System.Exception ex)
                {
                    Uqee.Debug.LogError($"格式化失败，参数长度不正确.args.len={args.Length}, text={txt}");
                    Uqee.Debug.LogError(ex);
                }
            }
            return txt;
        }
    }
}