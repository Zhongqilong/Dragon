using System.IO;

namespace Dragon.Pool
{
    public static class DataFactory<T> where T : new()
    {
        const int DATA_DESTROY_TIME = 100;
        static DataPool<T> _pool;
        static object _mutex = new object();

        /// <summary>
        /// 获取数据对象，如果没缓存，则创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get()
        {
            lock (_mutex)
            {
                if (_pool == null)
                {
                    _pool = new DataPool<T>();
                }
                return _pool.Get();
            }
        }

        // 说明：除了指定的协议数据，其它数据暂时不走缓存
        public static void Release(T data)
        {
            lock (_mutex)
            {
                if (data == null || _pool == null) return;
                if (data is MemoryStream)
                {
                    var ms = data as MemoryStream;
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.SetLength(0);
                }
                else if (data is System.Text.StringBuilder)
                {
                    (data as System.Text.StringBuilder).Length = 0;
                }
                _pool.Release(data);
            }
        }
    }
}