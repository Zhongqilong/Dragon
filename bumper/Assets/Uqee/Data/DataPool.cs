using System.Collections.Generic;
using System.IO;

public interface IResourceCache {
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="all">是否全部释放（切换场景时全部释放）</param>
    void ReleaseCache (bool all);
    void Dispose ();
}

namespace Uqee.Pool {
    internal class DataPool<T> : IResourceCache where T : new () {
        public DataPool () {
            DataPoolManager.AddPool (this);
        }

        private const int RELEASE_SECONDS = 60;
        object _mutex = new object ();
        Queue<T> pool = new Queue<T> (32);
        Queue<float> timePool = new Queue<float> (32);

        public T Get () {
            lock (_mutex) {
                if (pool.Count > 0 && timePool.Count > 0) {
                    timePool.Dequeue ();
                    return pool.Dequeue ();
                }
                return new T ();
            }
        }

        public void ReleaseCache (bool all) {
            if (!all || AppStatus.isApplicationQuit) {
                return;
            }
            lock (_mutex) {
                while (pool.Count > 0) {
                    if (timePool.Peek () > AppStatus.realtimeSinceStartup) {
                        break;
                    }
                    timePool.Dequeue ();
                    var obj = pool.Dequeue ();
                    if (obj is Stream) {
                        (obj as Stream).Close ();
                    }
                }
            }
        }

        public void Release (T data) {
            if (AppStatus.isApplicationQuit) {
                return;
            }
            if (data != null) {
                lock (_mutex) {
                    if (pool.Contains (data)) {
                        return;
                    }
                    pool.Enqueue (data);
                    timePool.Enqueue (AppStatus.realtimeSinceStartup + RELEASE_SECONDS);
                }
            }
        }

        public void Dispose () { }
    }
}