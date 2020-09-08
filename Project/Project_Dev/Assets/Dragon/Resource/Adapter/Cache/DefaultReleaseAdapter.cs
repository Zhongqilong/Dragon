using UnityEngine;
using UnityEditor;

namespace Uqee.Resource
{
    public class DefaultReleaseAdapter : IReleaseAdapter
    {
        public bool CanAutoRelease(AssetRequest req)
        {
            return true;
        }
        public bool CanRelease(AssetRequest req)
        {
            return true;
        }

        public void UnloadAssets(string category, UnityEngine.Object asset, bool unloadAggressively)
        {
            if (asset is AssetBundle)
            {
                (asset as AssetBundle).Unload(unloadAggressively);
            }
            else if (asset is GameObject)
            {
            }
            else
            {
                Resources.UnloadAsset(asset);
            }
        }
    }
}