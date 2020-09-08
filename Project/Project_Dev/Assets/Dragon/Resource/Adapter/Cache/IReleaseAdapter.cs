using UnityEngine;
using UnityEditor;

namespace Uqee.Resource
{
    public interface IReleaseAdapter
    {
        bool CanRelease(AssetRequest req);
        bool CanAutoRelease(AssetRequest req);
        void UnloadAssets(string category, UnityEngine.Object asset, bool unloadAggressively);
    }
}