
namespace Uqee.Resource
{
    public interface IManifestCache:IResourceCache
    {
        void AddCache(ManifestInfo info);
        ManifestInfo GetCache(string path);
        void RemoveCache(string path);
    }
}