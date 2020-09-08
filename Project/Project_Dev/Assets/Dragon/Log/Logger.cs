using System.Diagnostics;

namespace Dragon
{
    class Debug
    {
        [Conditional("ENABLE_LOG")]
        public static void Log(object info)
        {
            UnityEngine.Debug.Log(string.Format("<color=white>{0}</color>", info.ToString()));
        }

        [Conditional("ENABLE_LOG")]
        public static void Error(object info)
        {
            UnityEngine.Debug.Log(string.Format("<color=red>{0}</color>", info.ToString()));
        }
    }
}