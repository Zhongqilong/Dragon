using System;

namespace Uqee.Resource
{
    [Serializable]
    public class ManifestInfo
    {
        public string path;
        public string hash;
        public string[] deps;
    }
}