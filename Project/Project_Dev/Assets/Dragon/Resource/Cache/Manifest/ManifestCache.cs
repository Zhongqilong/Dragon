using System.Collections.Generic;

namespace Uqee.Resource
{
    public class ManifestCache : Singleton<ManifestCache>, IManifestCache
    {
        private Dictionary<string, ManifestInfo> _dict = new Dictionary<string, ManifestInfo>();
        public void AddCache(ManifestInfo info)
        {
            _dict[info.path] = info;
        }

        public ManifestInfo GetCache(string path)
        {
            ManifestInfo info = null;
            _dict.TryGetValue(path, out info);
            return info;
        }

        public void ReleaseCache(bool all)
        {
            _dict.Clear();
        }

        public void RemoveCache(string path)
        {
            //throw new System.NotImplementedException();
        }
    }
}