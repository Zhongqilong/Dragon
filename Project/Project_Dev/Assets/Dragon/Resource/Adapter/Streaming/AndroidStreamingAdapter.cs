#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace Uqee.Resource
{
    public class AndroidStreamingAdapter : IStreamingAdapter
    {
        private AndroidJavaClass _assetsAdapter;
        public AndroidStreamingAdapter()
        {
            _assetsAdapter = new AndroidJavaClass("com.uqee.AssetsAdapter");
        }
        public string GetStreamingText(string path)
        {
            //var assetName = string.Format("assets/{0}", System.IO.Path.GetFileName(path));
            var buff = _assetsAdapter.CallStatic<string>("getStreamingText", path);
            if (buff == null)
            {
                Uqee.Debug.LogError(string.Format("StreamingAsset load faile:{0}, asset not exist.", path));
                return null;
            }
            else
            {
                Uqee.Debug.Log(string.Format("StreamingAsset loaded:{0}.", path));
                return buff;
            }
        }
        public byte[] GetStreamingBytes(string path)
        {
            //var assetName = string.Format("assets/{0}", System.IO.Path.GetFileName(path));
            var buff = _assetsAdapter.CallStatic<byte[]>("getStreamingAssets", path);
            if (buff == null)
            {
                Uqee.Debug.LogWarning(string.Format("StreamingAsset load failed:{0}, asset not exist.", path));
                return null;
            }
            else
            {
                Uqee.Debug.Log(string.Format("StreamingAsset loaded:{0}.", path));
                return buff;
            }
        }
    }
}
#endif