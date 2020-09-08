using System.Collections.Generic;

namespace Uqee.Resource
{
    public static class RequestPool
    {
        public static void Release()
        {
            while (_reqReleaseList.Count > 0)
            {
                PushToPool(_reqReleaseList.Dequeue());
            }
        }

        private static Queue<IResourceRequest> _reqReleaseList = new Queue<IResourceRequest>();

        public static void MarkRelease(IResourceRequest req)
        {
            _reqReleaseList.Enqueue(req);
        }


        public static void PushToPool(IResourceRequest req)
        {
            req.Release();
        }
    }
}