using UnityEngine;

namespace Uqee.Resource
{
    [CreateAssetMenu(fileName = "DirectorySetting", menuName = "Create DirectorySetting")]
    [System.Serializable]
    public class DirectorySettingAssets : ScriptableObject
    {
        /// <summary>
        /// 热更文件保存目录 (Application.persistentDataPath/下面的子目录)
        /// 内置数据缓存目录（Application.streamingAssetsPath/下面的子目录），保存随包打入的文件
        /// </summary>
        public string abCacheFolder = "AssetBundle";
        public string versionCacheFolder = "Version";
        /// <summary>
        /// HTTP下载完成的保存文件的目录
        /// </summary>
        public string httpCacheFolder = "Http";
        public string voiceFolder = "Voice";
    }
}