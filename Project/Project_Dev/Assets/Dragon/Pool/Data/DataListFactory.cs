using System.Collections.Generic;

namespace Dragon.Pool
{
    public static class DataListFactory<T>
    {
        static DataPool<List<T>> _pool;
        static object _mutex = new object();

        /// <summary>
        /// 获取数据对象，如果没缓存，则创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> Get()
        {
            lock (_mutex)
            {
                if (_pool == null)
                {
                    _pool = new DataPool<List<T>>();
                }
                return _pool.Get();
            }
        }

        // 说明：除了指定的协议数据，其它数据暂时不走缓存
        public static void Release(List<T> data)
        {
            if (AppStatus.isApplicationQuit)
            {
                return;
            }
            lock (_mutex)
            {
                if (data == null || _pool == null) return;
                data.Clear();
                _pool.Release(data);
            }
        }
    }
}