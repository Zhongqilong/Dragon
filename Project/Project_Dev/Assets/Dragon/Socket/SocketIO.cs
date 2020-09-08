
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Security.Cryptography;
using UnityEngine;
using Uqee.Utility;
using Uqee.Caches;

namespace Uqee.Socket
{
    public class SocketIO
    {
        const int MAX_SEND_COUNT = 5;

        private BetterList<Tuple<byte[], int, string>> _sendBytesList;
        private BetterList<Tuple<byte[], float>> _recycleList;

        private object _sendLock = new object();
        private object _socketMutex = new object();

        private Thread _recvThread;
        private Thread _sendThread;
        private SocketBuffer _buffer;

        private System.Net.Sockets.Socket _socket;
        private Action<SocketBuffer> _recvHandler;
        private byte[] _tmpBuffer;

        private volatile bool _needConnect = false;

        private volatile int _tryCount;
        private volatile float _lastRetryTime;
        private volatile int _totalRetry;

        private volatile float _retryTimeout;
        const int MAX_RETRY = 1;

        #region 初始化
        public SocketIO(Action<SocketBuffer> recvHandler)
        {
            SocketSetting.encrypt = GameCfg.cfg.GetBool("encrpty");
            SocketSetting.decrypt = GameCfg.cfg.GetBool("decrypt");
            _buffer = new SocketBuffer(SocketSetting.maxRecvSize);
            _tmpBuffer = new byte[SocketSetting.maxRecvSize];
            _sendBytesList = new BetterList<Tuple<byte[], int, string>>();
            _recycleList = new BetterList<Tuple<byte[], float>>();
            _recvHandler = recvHandler;
            this.InitClient();
            Loom.RunAsync(_SendThread);
            Loom.RunAsync(_ReciveThread);
        }


        //初始化客户端Socket信息
        public void InitClient(bool retry = false)
        {
            if(_connectTimeoutId>0)
            {
                //正在尝试连接中。。。
                return ;
            }
            _state = CLOSE;

            if (retry)
            {
                this._tryCount++;
                if( AppStatus.realtimeSinceStartup - _lastRetryTime<5)
                {
                    _totalRetry++;
                }
                else
                {
                    _totalRetry = 0;
                }
                _lastRetryTime = AppStatus.realtimeSinceStartup;
                if (this._tryCount > MAX_RETRY || this._totalRetry>MAX_RETRY)
                {
                    this._tryCount = 0;
                    this._totalRetry = 0;
                    this._lastRetryTime = 0;
                    //超过2次连接，不再重试，弹窗提示网络无法连接。
                    SocketStateManager.AddState(EventTypes.SOCKET_ERROR);
                    return;
                }
                Uqee.Debug.Log(string.Format("[SocketIO]try connect {0}/{1}", _tryCount, MAX_RETRY),Color.cyan);
            }
            else
            {
                if (this._socket != null)
                {
                    Uqee.Debug.Log("[SocketIO]Socket alread exists. Could't init again.",Color.cyan);
                    return;
                }
                this._tryCount = 0;
            }
            
            _needConnect = true;
            _buffer.Reset();
        }
        #endregion

        private void _AddToRecyle(byte[] o)
        {
            lock (_recycleList)
            {
                _recycleList.Add(new Tuple<byte[], float>(o, AppStatus.realtimeSinceStartup + 2));
            }
        }

        #region 接收

        void _ReciveThread()
        {
            while (true)
            {
                if (AppStatus.isApplicationQuit)
                {
                    break;
                }
                try
                {
                    switch (_state)
                    {
                        case CONNECT:
                        case WAITING:
                        case READY:
                            _OnReceiveUpdate();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Uqee.Debug.LogError(e);
                }
                Thread.Sleep(SocketSetting.sleepTime);
            }
        }
        private void _OnReceiveUpdate()
        {
            if (_socket == null)
            {
                return;
            }

            int recvCount = 0;
            try
            {
                lock (_socketMutex)
                {
                    if (_socket == null)
                    {
                        return;
                    }
                    if (_socket.Available > 0)
                    {
                        var byteRead = _socket.Receive(_tmpBuffer, recvCount, Math.Min(_buffer.Space, _socket.Available), 0);
                        if (byteRead > 0)
                        {
                            recvCount += byteRead;
#if UNITY_EDITOR
                            var boo = SimpleCache.isFiltered(-1);
                            if (!boo)
#endif
                                Uqee.Debug.Log($"[SocketIO]received:{byteRead} bytes.", Color.cyan);
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                this._OnError(ex);
                return;
            }
            if (recvCount > 0)
            {
                _buffer.WriteBytes(_tmpBuffer, 0, recvCount);
            }
            if (_buffer.Available > 0)
            {
                _recvHandler(_buffer);
                _buffer.Align();
            }
        }

        /// <summary>
        /// 从缓冲区里面读取，如果有设置加密，自自动处理好解密
        /// 返回的数据是长度与len接近的数组，实际应该读取的长度，使用len处理。
        /// 如果直接使用返回的bytes.Length,可能得不到预期的结果
        /// </summary>
        /// <param name="len">读取的长度</param>
        /// <returns></returns>
        public byte[] ReadBytes(int len)
        {
            byte[] bytes = StreamBufferPool.GetBuffer(len);
            _buffer.ReadBytes(bytes, 0, len);

            byte[] ret;
            if (SocketSetting.decrypt)
            {
                ret = _Decrypt(bytes, len);
            }
            else
            {
                ret = bytes;
            }
            _AddToRecyle(bytes);
            return ret;
        }
        #endregion

        #region 发送
        void _SendThread()
        {
            while (true)
            {
                if (AppStatus.isApplicationQuit)
                {
                    break;
                }
                try
                {
                    _OnSendUpdate();
                }
                catch (Exception e)
                {
                    Uqee.Debug.LogError(e);
                }
                Thread.Sleep(SocketSetting.sleepTime);
            }
        }
        private void _OnSendUpdate()
        {
            var t = AppStatus.realtimeSinceStartup;
            lock (_recycleList)
            {
                for (int i = _recycleList.size - 1; i >= 0; i--)
                {
                    if (_recycleList[i].Item2 > t)
                    {
                        continue;
                    }
                    StreamBufferPool.RecycleBuffer(_recycleList[i].Item1);
                    _recycleList.RemoveAt(i);
                }
            }
            switch (_state)
            {
                case CONNECT:
                case WAITING:
                case READY:
                    int size = 0;
                    lock (_sendLock)
                    {
                        size = Math.Min(MAX_SEND_COUNT, _sendBytesList.size);
                        if (size > 0)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                if (_socket == null)
                                {
                                    //Send过程可能会socket断开，需要要循环中判断一下
                                    break;
                                }
                                var tuple = _sendBytesList[0];
                                _SendBytes(tuple.Item1, tuple.Item2, tuple.Item3);
                                _sendBytesList.RemoveAt(0);
                            }
                        }
                    }
                    break;
                case RETRY:
                    if (_retryTimeout < t)
                    {
                        _state = CLOSE;
                        Loom.QueueOnMainThread((o)=>{
                            InitClient(true);
                        }, null);
                    }
                    break;
                case CLOSE:
                    if (_needConnect)
                    {
                        _needConnect = false;
                        _ConnectSocket();
                    }
                    break;
            }
        }
        public void SendBytes(byte[] bytes, int protoId, string name)
        {
            lock (_sendLock)
            {
                _sendBytesList.Add(new Tuple<byte[], int, string>(bytes, protoId, name));
            }
        }

        private void _SendBytes(byte[] bytes, int protoId, string protoName)
        {
            if (isConnected)
            {
                var byteLen = bytes.Length;

                int buffLen = byteLen + 4;
                byte[] tmp = StreamBufferPool.GetBuffer(buffLen);
                byte[] id = BitConverter.GetBytes(protoId);
                Buffer.BlockCopy(id, 0, tmp, 0, id.Length);
                Buffer.BlockCopy(bytes, 0, tmp, id.Length, byteLen);

                byte[] buffer = null;
                //异步发送消息请求
                if (SocketSetting.encrypt)
                {
                    buffer = _Encrypt(tmp, false, 0, buffLen);

                    buffLen = buffer.Length;
                }
                else
                {
                    buffer = tmp;
                }

                int length = buffLen + 4;
                byte[] head = BitConverter.GetBytes(length);
                byte[] data = StreamBufferPool.GetBuffer(length);
                Buffer.BlockCopy(head, 0, data, 0, head.Length);
                Buffer.BlockCopy(buffer, 0, data, head.Length, buffLen);


                try
                {
                    lock (_socketMutex)
                    {
#if UNITY_EDITOR
                        var boo = SimpleCache.isFiltered(protoId);
                        if (!boo)
#endif
                        {
                            Uqee.Debug.Log(string.Format("[SocketIO]send:{0},{1}, size:{2}", protoId, protoName, buffLen), Color.cyan);
                        }
                        _socket?.Send(data, 0, length, SocketFlags.None);
                    }
                    _AddToRecyle(data);
                }
                catch (SocketException ex)
                {
                    _OnError(ex);
                }

                StreamBufferPool.RecycleBuffer(tmp);
            }
        }
        public void SendNative(byte[] bytes)
        {
            try
            {
                lock (_socketMutex)
                {
                    _socket?.Send(bytes);
                }
            }
            catch (Exception ex)
            {
                _OnError(ex);
            }
        }
        public void SendNative(byte[] bytes, int offset, int length, bool encrypt = false)
        {
            if (encrypt)
            {
                bytes = _Encrypt(bytes, true, offset, length);
            }
            try
            {
                lock (_socketMutex)
                {
                    _socket?.Send(bytes, offset, length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                _OnError(ex);
            }
        }
        #endregion

        #region 连接
        private IAsyncResult _connectAsyncResult;
        private uint _connectTimeoutId;
        private void _ResetConnectTimeout(bool endConnect = true)
        {
            JobScheduler.I.ClearTimer(_connectTimeoutId);
            _connectTimeoutId = 0;

            if (_connectAsyncResult != null)
            {
                if (endConnect)
                {
                    _socket.EndConnect(_connectAsyncResult);
                }
                _connectAsyncResult = null;
            }
        }
        private void _OnConnectTimeout()
        {
            Uqee.Debug.LogWarning("连接服务器超时。", Color.red);
            _ResetConnectTimeout();

            _tryCount = MAX_RETRY;
            _OnError(null);
        }
        private void _ConnectSocket()
        {
            SocketSetting.sleepTime = 17;
            _connectTimeoutId = JobScheduler.I.SetTimeOut(_OnConnectTimeout, 20);
            try
            {
                IPAddress[] addr = Dns.GetHostAddresses(SocketSetting.host);
                IPAddress address = addr[0];

                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    Uqee.Debug.Log("Connect IPv6");
                    this._socket = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                }
                else
                {
                    Uqee.Debug.Log("Connect IPv4");
                    this._socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                }
                Uqee.Debug.Log(string.Format("[SocketIO]socket connecting.... {0}:{1}", SocketSetting.host, SocketSetting.port), Color.cyan);

                lock (_socketMutex)
                {
                    if (_socket != null)
                    {
                        this._socket.Connect(address, SocketSetting.port);
                        if (this._socket.Connected)
                        {
                            _ConnectCallback();
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                _ResetConnectTimeout();
                _OnError(ex);
            }
        }
        private void _ConnectCallback()
        {
            try
            {
                Uqee.Debug.Log("[SocketIO]socket connect success.", Color.cyan);
                _state = CONNECT;
                SocketStateManager.AddState(EventTypes.SOCKET_CONNECT);
            }
            catch (SocketException ex)
            {
                this._OnError(ex);
            }
        }
        #endregion

        #region 状态切换
        private volatile int _state;
        const int RETRY = 4;
        const int READY = 3;
        const int WAITING = 2;
        const int CONNECT = 1;
        const int CLOSE = 0;
        const int ERROR = 5;
        public bool isClose
        {
            get
            {
                return _socket == null || _state == CLOSE || _state == ERROR;
            }
        }
        public bool isConnected
        {
            get
            {
                return _socket != null && (_state == READY||_state==WAITING);
            }
        }

        public bool isReady
        {
            get {
                return _socket != null && _state == READY;
            }
        }


        private void _OnError(Exception ex)
        {
            if (_state == READY|| _state == WAITING)
            {
                _state = ERROR;
            }
            if (ex != null)
            {
                Uqee.Debug.LogError($"[SocketIO]{ex.Message}");
            }
            this.Close();
            SocketStateManager.AddState(EventTypes.SOCKET_DISCONNECT, 0);
        }
        /// <summary>
        /// 错误状态.关闭连接，发出 SOCKET_DISCONNECT 事件
        /// </summary>
        public void Error()
        {
            _OnError(null);
        }
        /// <summary>
        /// GateState状态正常，可以正常发包，设置为Ready状态
        /// </summary>
        public void Ready()
        {
            _tryCount = 0;
            _state = READY;
            SocketStateManager.AddState(EventTypes.SOCKET_READY);
        }
        public void Waiting()
        {
            _tryCount = 0;
            _state = WAITING;
            SocketStateManager.AddState(EventTypes.SOCKET_WAITING);
        }
        /// <summary>
        /// GateState正常连接状态
        /// </summary>
        public void Connect()
        {
            _ResetConnectTimeout(false);
        }

        public void TryConnect()
        {
            Uqee.Debug.Log("TryConnect",Color.cyan);
            _state = RETRY;
            _retryTimeout = AppStatus.realtimeSinceStartup + 1.5f;
        }
        #endregion

        #region 关闭、销毁
        public void Destroy()
        {
            if (_recvThread == null)
            {
                return;
            }
            _recvThread.Abort();
            _sendThread.Abort();

            _recvThread = null;
            _sendThread = null;
            Close();
        }
        public void Clear()
        {
            _tryCount = 0;
            _state = CLOSE;
            _needConnect = false;
            _buffer.Reset();
            _retryTimeout = 0;
        }
        //关闭Socket 
        public void Close()
        {
            SocketStateManager.ClearState();

            lock (_socketMutex)
            {
                if (this._socket != null)
                {
                    if (this._socket.Connected)
                    {
                        try
                        {
                            this._socket.Shutdown(SocketShutdown.Both);
                            this._socket.Close();
                        }
                        catch (Exception)
                        {

                        }
                        Uqee.Debug.Log("[SocketIO]Socket close.",Color.cyan);
                        SocketStateManager.AddState(EventTypes.SOCKET_CLOSE);
                    }

                    this._socket = null;
                }
            }
            lock (_sendLock)
            {
                _sendBytesList.Clear();
            }
        }
        #endregion

        #region 加密、解密
        private int _sendTime = 0;
        private byte[] _Encrypt(byte[] bytes, bool isProxy = false, int byteOffset=0, int byteLen = -1)
        {
            string key = isProxy ? SocketSetting.recvKey : SocketSetting.sendKey;
            if (!isProxy)
            {
                //var str = BitConverter.ToString(bytes);
                //Uqee.Debug.LogError("Before MD5 : " + str);
                if (byteLen == -1)
                {
                    byteLen = bytes.Length;
                }
                _sendTime++;
                MD5 md5 = MD5.Create();
                byte[] time = BitConverter.GetBytes(_sendTime);
                byte[] hash = md5.ComputeHash(bytes, byteOffset, byteLen);
                md5.Dispose();
                //var str2 = BitConverter.ToString(hash);
                //Uqee.Debug.LogError("MD5 HASH : " + str2);

                var len = time.Length + hash.Length + byteLen;
                byte[] data = StreamBufferPool.GetBuffer(len);

                Buffer.BlockCopy(time, 0, data, 0, time.Length);
                Buffer.BlockCopy(hash, 0, data, time.Length, hash.Length);
                Buffer.BlockCopy(bytes, byteOffset, data, time.Length + hash.Length, byteLen);

                bytes = CryptUtils.AESEncrypt(data, key, len);

                StreamBufferPool.RecycleBuffer(data);
            }
            else
            {
                bytes = CryptUtils.AESEncrypt(bytes, key, byteLen);
            }
            //var str1 = BitConverter.ToString(bytes);
            //Uqee.Debug.LogError("After MD5 : " + str1);
            return bytes;
        }

        private byte[] _Decrypt(byte[] bytes, int len = -1)
        {
            return CryptUtils.AESDecrypt(bytes, SocketSetting.recvKey, len);
        }
        #endregion
    }
}