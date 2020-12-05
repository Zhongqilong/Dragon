using System.Collections.Generic;

namespace Uqee.Pool
{
    public static class DataPoolManager
    {
        private static List<IResourceCache> _poolList = new List<IResourceCache>();
        public static void AddPool(IResourceCache pool)
        {
            _poolList.Add(pool);
        }
        public static void ReleaseCache(bool all)
        {
            foreach (var pool in _poolList)
            {
                pool.ReleaseCache(all);
            }
        }
    }
}
