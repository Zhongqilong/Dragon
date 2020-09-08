using UnityEngine;
using UnityEditor;
using System;

namespace Uqee.Socket
{

    public class SocketBuffer
    {
        private object mutex = new object();
        private byte[] _bytes;
        private byte[] _swapBytes;
        private volatile int _buffUsed;
        private volatile int _buffOffset;
        /// <summary>
        /// 触发数据对齐的数量大小。
        /// </summary>
        private int _alignHitSize;
        public SocketBuffer(int size)
        {
            _bytes = new byte[size];
            _swapBytes = new byte[size];
            _alignHitSize = (int)(size * 0.5f);
        }

        public void Reset()
        {
            _buffOffset = 0;
            _buffUsed = 0;
        }
        /// <summary>
        /// buffer中还可读取的数据长度
        /// </summary>
        public int Available
        {
            get
            {
                if (this._buffUsed == 0)
                {
                    return 0;
                }
                return this._buffUsed - this._buffOffset;
            }
        }
        /// <summary>
        /// buffer剩余可读取的空间长度
        /// </summary>
        public int Space
        {
            get
            {
                return _bytes.Length - _buffUsed;
            }
        }
        public void Skip(int count)
        {
            _buffOffset += count;
        }
        public int ReadInt()
        {
            int val;
            lock (mutex)
            {
                val = BitConverter.ToInt32(this._bytes, this._buffOffset);
                this._buffOffset += 4;
            }
            return val;
        }
        public short ReadShort()
        {
            short val;
            lock (mutex)
            {
                val = BitConverter.ToInt16(this._bytes, this._buffOffset);
                this._buffOffset += 2;
            }
            return val;

        }
        public byte ReadByte()
        {
            byte val;
            lock (mutex)
            {
                val = _bytes[_buffOffset];
                this._buffOffset++;
            }
            return val;
        }
        public char ReadChar()
        {
            char val;
            lock (mutex)
            {
                val = BitConverter.ToChar(this._bytes, this._buffOffset);
                this._buffOffset++;
            }
            return val;
        }

        public void Align(bool force = false)
        {
            if (_buffUsed > 0 && _buffOffset == _buffUsed)
            {
                _buffOffset = 0;
                _buffUsed = 0;
                //Log.d(LOG.NETWORK, "[SockeIO] _buffOffset:0");
            }
            else if (force || _buffUsed > _alignHitSize)
            {
                var swapSize = _buffUsed - _buffOffset;
                lock (mutex)
                {
                    Buffer.BlockCopy(_bytes, _buffOffset, _swapBytes, 0, swapSize);
                    Buffer.BlockCopy(_swapBytes, 0, _bytes, 0, swapSize);
                }
                _buffOffset = 0;
                _buffUsed = swapSize;
            }

        }
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="bytes">准备写入Buffer的数据</param>
        /// <param name="offset">数据的偏移量</param>
        /// <param name="count">要写入的长度</param>
        public void WriteBytes(byte[] bytes, int offset, int count)
        {
            int used = _buffUsed + count;

            lock (mutex)
            {
                if (used > _bytes.Length)
                {
                    Uqee.Debug.LogError($"buff pool over size:{_buffUsed + count}");
                }
                Buffer.BlockCopy(bytes, 0, _bytes, _buffUsed, count);
            }
            _buffUsed = used;
        }
        /// <summary>
        /// 读取数据到目标数组里
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void ReadBytes(byte[] bytes, int offset, int count)
        {
            lock (mutex)
            {
                Buffer.BlockCopy(_bytes, _buffOffset, bytes, offset, count);
            }
            _buffOffset += count;
        }
        public byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            ReadBytes(bytes, 0, count);
            return bytes;
        }
    }
}