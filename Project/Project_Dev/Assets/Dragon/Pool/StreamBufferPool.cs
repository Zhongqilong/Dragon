using System;
using System.Collections.Generic;

/// <summary>
/// 说明：
///     1）封装字节buffer以及MemoryStream、BinaryRender、BinaryWrite操作
///     2）对如上对象执行缓存管理
///     3）多线程安全
/// 
/// by wsh @ 2017-07-03
/// </summary>

public sealed class StreamBufferPool
{
    const int BUFFER_POOL_SIZE = 500;
    //static Dictionary<int, Queue<StreamBuffer>> streamPool = new Dictionary<int, Queue<StreamBuffer>>();
    static Dictionary<int, Queue<byte[]>> bufferPool = new Dictionary<int, Queue<byte[]>>();
    //volatile static int streamCount = 0;
    volatile static int bufferCount = 0;
    public static void ClearPool()
    {
        lock(bufferPool)
        {
            foreach(var pairs in bufferPool)
            {
                pairs.Value.Clear();
            }
            bufferPool.Clear();
        }
    }

    private static int _GetNearestSize(int expectedSize)
    {
        //取最接近的块大小，按照  1K, 10K, 100K, 500K, 1M
        if (expectedSize <= 1024)
        {
            return 1024;
        }
        else if (expectedSize <= 10240)
        {
            return 10240;
        }
        else if (expectedSize <= 102400)
        {
            return 102400;
        }
        else if (expectedSize <= 512000)
        {
            return 512000;
        }
        else if (expectedSize <= 1024000)
        {
            return 1024000;
        }
        return (int)Math.Ceiling(expectedSize / 1024000f);
    }
    public static byte[] GetBuffer(int expectedSize)
    {
        if (expectedSize < 0) throw new Exception("expectedSize must >= 0!");
        expectedSize = _GetNearestSize(expectedSize);
        Queue<byte[]> bufferCache = null;
        byte[] buffer = null;
        lock (bufferPool)
        {
            if (!bufferPool.TryGetValue(expectedSize, out bufferCache))
            {
                bufferCache = new Queue<byte[]>();
                bufferPool.Add(expectedSize, bufferCache);
            }
            if (bufferCache.Count > 0)
            {
                bufferCount--;
                buffer = bufferCache.Dequeue();
            }
        }
        if (buffer == null) buffer = new byte[expectedSize];
        return buffer;
    }

    public static byte[] DeepCopy(byte[] bytes)
    {
        if (bytes == null) return null;
        if (bytes.Length == 0) return new byte[0];
        byte[] newBytes = GetBuffer(bytes.Length);
        Buffer.BlockCopy(bytes, 0, newBytes, 0, bytes.Length);
        return newBytes;
    }

    public static void RecycleBuffer(byte[] buffer)
    {
        if (buffer == null || buffer.Length == 0) return;
        if (bufferCount > BUFFER_POOL_SIZE) return;

        Queue<byte[]> bufferCache = null;
        lock (bufferPool)
        {
            if (!bufferPool.TryGetValue(buffer.Length, out bufferCache))
            {
                bufferCache = new Queue<byte[]>();
                bufferPool.Add(buffer.Length, bufferCache);
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)0;
            }
            bufferCount++;
            bufferCache.Enqueue(buffer);
        }
    }
}
