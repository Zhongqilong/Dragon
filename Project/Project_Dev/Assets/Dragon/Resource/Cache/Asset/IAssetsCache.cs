
namespace Uqee.Resource
{
    public interface IAssetsCache : IResourceCache
    {
        void AddCache(AssetRequest req);
        AssetRequest GetCache(string assetPath);
        void RemoveCache(string assetPath, bool unloadAggressively = false);
        UnityEngine.Object GetObject(string assetPath, string assetName);
    }
}