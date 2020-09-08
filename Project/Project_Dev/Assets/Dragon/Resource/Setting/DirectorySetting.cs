using UnityEngine;
using System.IO;

namespace Uqee.Resource
{
    /// <summary>
    /// 资源相关目录设置
    /// </summary>
    public static class DirectorySetting
    {

        [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitOnLoad()
        {
            if (persistentDir != null)
            {
                return;
            }
            //cfg = Resources.Load<DirectorySettingAssets>("DirectorySetting");
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<DirectorySettingAssets>();
                Uqee.Debug.Log("[DirectorySetting] use default config");
            }

            persistentDir = Application.persistentDataPath;
#if UNITY_EDITOR
            persistentDir = persistentDir.Replace(Application.productName, "wly2");
#endif
            if (!Directory.Exists(persistentDir))
            {
                Directory.CreateDirectory(persistentDir);
            }

            cacheABDir = $"{persistentDir}/{cfg.abCacheFolder}/";
            if (!Directory.Exists(cacheABDir))
            {
                Directory.CreateDirectory(cacheABDir);
            }
            cacheVersionDir = $"{persistentDir}/{cfg.versionCacheFolder}/";
            if (!Directory.Exists(cacheVersionDir))
            {
                Directory.CreateDirectory(cacheVersionDir);
            }
            tempDir = Application.temporaryCachePath;
#if UNITY_EDITOR
            tempDir = tempDir.Replace(Application.productName, "wly2");
#endif
            cacheHttpDir = $"{tempDir}/{cfg.httpCacheFolder}/";

            if (!Directory.Exists(cacheHttpDir))
            {
                Directory.CreateDirectory(cacheHttpDir);
            }

            voiceDir = $"{tempDir}/{cfg.voiceFolder}/";

            if (!Directory.Exists(voiceDir))
            {
                Directory.CreateDirectory(voiceDir);
            }

            streamingDir = Application.streamingAssetsPath;
            cacheInternalDir = $"{streamingDir}/{cfg.abCacheFolder}/";

            Uqee.Debug.LogWarning($"cacheABDir = {cacheABDir}");
            Uqee.Debug.LogWarning($"cacheVersionDir = {cacheVersionDir}");
            Uqee.Debug.LogWarning($"cacheInternalDir = {cacheInternalDir}");
            Uqee.Debug.LogWarning($"cacheHttpDir = {cacheHttpDir}");
        }
        public static DirectorySettingAssets cfg;

        public static string persistentDir;
        public static string streamingDir;
        public static string tempDir;
        /// <summary>
        /// 热更版本保存目录
        /// </summary>
        public static string cacheVersionDir;
        /// <summary>
        /// 热更 AssetBundle 文件保存目录
        /// </summary>
        public static string cacheABDir;
        /// <summary>
        /// 内置数据缓存目录（streaming下面的子目录），保存随包打入的文件
        /// </summary>
        public static string cacheInternalDir;
        /// <summary>
        /// HTTP下载完成的保存文件的目录
        /// </summary>
        public static string cacheHttpDir;
        public static string voiceDir;
        public static string testVoicePath = "test_voice";
    }
}