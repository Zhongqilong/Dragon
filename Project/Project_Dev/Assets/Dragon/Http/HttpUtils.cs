using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Uqee.Pool;

namespace Uqee.Http
{
    public delegate void HttpDownloadDelegate(int bytes);
    public static class HttpUtils
    {
        //HttpRequest超时时间，毫秒
        private static int _httpReqTimeout = 10000;
        //程序定义的超时中断时间，12秒
        private static int _httpTimeout = 12;

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            UpdateManager.I.AddCallback(_Update, "HttpUtils");
        }

        private static ConcurrentQueue<Action<string>> _callbackList = new ConcurrentQueue<Action<string>>();
        private static ConcurrentQueue<string> _callbackParamList = new ConcurrentQueue<string>();

        private static Action<string> _callbackTmp;
        private static string _paramTmp;
        private static void _Update()
        {
            //多线程调用的Http请求，在Update主线程中处理回调
            while (!_callbackList.IsEmpty)
            {
                _callbackList.TryDequeue(out _callbackTmp);
                _callbackParamList.TryDequeue(out _paramTmp);
                if (_callbackTmp != null)
                {
                    _callbackTmp.Invoke(_paramTmp);
                }
            }

            //HttpRequsetData dat;
            //while (!_toReleaseList.IsEmpty)
            //{
            //    _toReleaseList.TryDequeue(out dat);
            //    DataFactory<HttpRequsetData>.Release(dat);
            //}
        }


        private static Dictionary<string, string> _postCharsetDict = new Dictionary<string, string>();
        private static Dictionary<string, string> _getCharsetDict = new Dictionary<string, string>();

        /// <summary>
        /// 获取http头里面的contentType设置
        /// </summary>
        /// <param name="charset"></param>
        /// <param name="isPost"></param>
        /// <returns></returns>
        private static string _GetContType(string charset, bool isPost = false)
        {
            var dict = isPost ? _postCharsetDict : _getCharsetDict;
            string contType = null;
            dict.TryGetValue(charset, out contType);
            if (contType == null)
            {
                contType = string.Format(isPost ? HttpConst.HTTP_POST_CHARSET : HttpConst.HTTP_GET_CHARSET, charset);
                dict[charset] = contType;
            }
            return contType;
        }

        #region Http多线程请求
        private static async void _HttpRequest(object o)
        {
            try
            {
                await __HttpRequest(o);
            }
            catch (Exception e)
            {
                Uqee.Debug.LogError(e);
            }
        }

        private static async Task<string> __HttpRequest(object o)
        {
            HttpRequsetData reqData = (HttpRequsetData)o;
            if (!NetworkManager.HasNetwork())
            {
                //没有网络，不处理，直接返回
                //await 0;
                if (reqData.callback != null)
                {
                    _callbackList.Enqueue(reqData.callback);

                    if ((reqData.contType & HttpAssetsContentType.JSON) != 0)
                    {
                        _callbackParamList.Enqueue(HttpConst.NETWORK_ERROR_JSON);
                    }
                    else
                    {
                        _callbackParamList.Enqueue(string.Empty);
                    }
                }
                reqData.Release();
                return null;
            }
            Uqee.Debug.Log(string.Format("[HttpUtils]request:{0} - {1} {2}", reqData.method, reqData.url, reqData.postDataStr == null ? string.Empty : reqData.postDataStr), UnityEngine.Color.cyan);
            HttpWebRequest request = null;
            if (reqData.method == HttpConst.HTTP_POST)
            {
                //POST提交的处理
                byte[] buf = Encoding.UTF8.GetBytes(reqData.postDataStr);

                request = _CreateHttp(reqData.url);

                request.Method = HttpConst.HTTP_POST;
                request.AllowWriteStreamBuffering = true;
                var stream = request.GetRequestStream();
                await stream.WriteAsync(buf, 0, buf.Length);
                stream.Flush();
            }
            else
            {
                //Get请求的处理

                //拼接参数URL
                string url = null;
                if (!string.IsNullOrEmpty(reqData.postDataStr))
                {
                    var urlBuilder = DataFactory<StringBuilder>.Get();
                    urlBuilder.Append(reqData.url);
                    urlBuilder.Append("?");
                    urlBuilder.Append(reqData.postDataStr);
                    url = urlBuilder.ToString();
                    DataFactory<StringBuilder>.Release(urlBuilder);
                }
                else
                {
                    url = reqData.url;
                }
                request = _CreateHttp(url);

                request.Method = HttpConst.HTTP_GET;
                if ((reqData.contType & HttpAssetsContentType.NO_CACHE) != 0)
                {
                    request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                }
            }

            request.ContentType = _GetContType(reqData.charset, reqData.method == HttpConst.HTTP_POST);

            //等待返回
            string retString = null;
            var bytes = await _GetResponse(request);

            if (bytes == null)
            {
                //没有返回数据
                Uqee.Debug.LogWarning($"[HttpUtils]request failed:{reqData.url}", Color.red);
                if (reqData.callback != null)
                {
                    _callbackList.Enqueue(reqData.callback);
                    if ((reqData.contType & HttpAssetsContentType.JSON) != 0)
                    {
                        _callbackParamList.Enqueue(HttpConst.ERROR_JSON);
                    }
                    else
                    {
                        _callbackParamList.Enqueue(string.Empty);
                    }
                }
            }
            else
            {
                if (reqData.charset == HttpConst.CHARSET_UTF8)
                {
                    retString = Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    retString = Encoding.GetEncoding(reqData.charset).GetString(bytes);
                }

                Uqee.Debug.Log(string.Format("[HttpUtils]response: {0}", retString), UnityEngine.Color.cyan);
                if (reqData.callback != null)
                {
                    _callbackList.Enqueue(reqData.callback);
                    _callbackParamList.Enqueue(retString);
                }
            }
            reqData.Release();
            return retString;
        }
        #endregion

        /// <summary>
        /// 获取http连接对象
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static HttpWebRequest _CreateHttp(string url)
        {
            var request = WebRequest.CreateHttp(url);

            request.KeepAlive = false;
            request.ReadWriteTimeout = _httpReqTimeout;
            request.Timeout = _httpReqTimeout;
            request.UseDefaultCredentials = true;
            return request;
        }
        /// <summary>
        /// http下载
        /// </summary>
        /// <param name="url"></param>
        public static async Task<byte[]> HttpDownloadAsync(string url, HttpDownloadDelegate onProgress)
        {
            if (!NetworkManager.HasNetwork())
            {
                await 0;
                return null;
            }
            try
            {
                var request = _CreateHttp(url);
                request.Method = HttpConst.HTTP_GET;
                request.ContentType = HttpConst.HTTP_DOWNLOAD;
                return await _GetResponse(request, onProgress);
            }
            catch (Exception ex)
            {
                Uqee.Debug.LogWarning($"[HttpUtils]{url} download fail:{ex.Message}", Color.red);
            }
            return null;
        }

        private static async Task<byte[]> _GetResponse(HttpWebRequest request, HttpDownloadDelegate onProgress = null)
        {
            HttpWebResponse response = null;
            byte[] bytes = null;
            onProgress?.Invoke(0);
            //设置一个超时时间取消请求
            var timeId = JobScheduler.I.SetTimeOut(() =>
            {
                Uqee.Debug.LogWarning($"连接超时15秒.{request.RequestUri.ToString()}", Color.red);
                try
                {
                    request.Abort();
                    //if( _httpTimeout<20 )
                    //{
                    //    _httpTimeout += 5;
                    //    _httpReqTimeout += 5000;
                    //}
                }
                catch (Exception) { }
            }, _httpTimeout);

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
                JobScheduler.I.ClearTimer(timeId);
                var stream = response.GetResponseStream();
                //if(_httpTimeout>12)
                //{
                //    _httpTimeout--;
                //    _httpReqTimeout -= 1000;
                //}
                //开辟临时缓存内存
                byte[] byteArrayRead = StreamBufferPool.GetBuffer(HttpConst.BUFF_SIZE); //  1字节*1024 = 1k 1k*100 = 100K内存
                bytes = new byte[(int)stream.Length];
                int byteOffset = 0;

                while (true)
                {
                    int readCount = await stream.ReadAsync(byteArrayRead, 0, HttpConst.BUFF_SIZE);
                    if (readCount > 0)
                    {
                        onProgress?.Invoke(byteOffset);

                        Buffer.BlockCopy(byteArrayRead, 0, bytes, byteOffset, readCount);
                        byteOffset += readCount;
                    }

                    if (byteOffset == bytes.Length)
                    {
                        break;
                    }
                }
                StreamBufferPool.RecycleBuffer(byteArrayRead);
            }
            catch (Exception ex)
            {
                JobScheduler.I.ClearTimer(timeId);
                Uqee.Debug.LogWarning(ex, Color.red);
            }


            response?.Close();
            request?.Abort();

            return bytes;
        }

        public static void GetJson(string url, string postDataStr = null, Action<string> callback = null, string requestMethod = HttpConst.HTTP_GET)
        {
            Task.Factory.StartNew(_HttpRequest, HttpRequsetData.Create(url, postDataStr, callback, HttpAssetsContentType.JSON, requestMethod));
        }
        public static void HttpGet(string url, string postDataStr = null, Action<string> callback = null)
        {
            Task.Factory.StartNew(_HttpRequest, HttpRequsetData.Create(url, postDataStr, callback));
        }

        public static void GetHtml(string url, Action<string> callback = null)
        {
            Task.Factory.StartNew(_HttpRequest, HttpRequsetData.Create(url, "", callback, HttpAssetsContentType.JSON, HttpConst.HTTP_GET, HttpConst.CHARSET_GB2312));
        }

        /// <summary>
        /// http post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postDataStr"></param>
        /// <param name="callback"></param>
        public static void HttpPost(string url, Dictionary<string, string> data = null, Action<string> callback = null, bool isPostUrl = false)
        {
            var _strBuff = DataFactory<StringBuilder>.Get();
            if (data != null)
            {
                foreach (var param in data)
                {
                    _strBuff.Append(param.Key);
                    _strBuff.Append("=");
                    _strBuff.Append(Uri.EscapeDataString(param.Value));
                    _strBuff.Append("&");
                }
                if (_strBuff.Length > 0)
                {
                    _strBuff.Remove(_strBuff.Length - 1, 1);
                    // postDataStr = postDataStr.Substring(0, postDataStr.Length - 1);
                    if (isPostUrl)
                    {
                        url += "?" + _strBuff.ToString();
                        _strBuff.Clear();
                    }
                }
            }
            Task.Factory.StartNew(_HttpRequest, HttpRequsetData.Create(url, _strBuff.ToString(), callback, HttpAssetsContentType.TEXT, HttpConst.HTTP_POST));
            DataFactory<StringBuilder>.Release(_strBuff);
            //ResManagerStartCoroutine(_HttpRequest(url, _strBuff.ToString(), callback, HTTP_POST));
        }
    }
}