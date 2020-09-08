using Uqee.Http;
using LitJson;
using System;

namespace Uqee.Resource
{

    public class HttpAssetRequest : AbstractResourceRequest<HttpAssetRequest>
    {

        public string hashStr;
        /// <summary>
        /// [加载HTTP资源]校验的MD5值
        /// </summary>
        public string checkMd5;
        /// <summary>
        /// [加载HTTP资源]是否json格式，如果true，则设置到loadedJson
        /// </summary>
        //public bool isJson = false;
        /// <summary>
        /// [加载HTTP资源]是否gzip格式，如果true，则解压后设置到loadedBytes
        /// </summary>
        //public bool isGzip = false;
        public HttpAssetsContentType httpContType = HttpAssetsContentType.RAW;
        public volatile int downloadBytes;
        public int w;
        public int h;
        public JsonData loadedJson;
        public byte[] loadedBytes;
        public string loadedText;
        public bool isAbort { get; private set; }

        /// <summary>
        /// [加载HTTP资源]下载进度，返回资源名和下载的长度
        /// </summary>
        public Action<HttpAssetRequest> onProgress;

        override public void Release()
        {
            base.Release();

            httpContType = HttpAssetsContentType.RAW;
            downloadBytes = 0;

            checkMd5 = null;
            loadedBytes = null;
            loadedText = null;
            loadedJson = null;
            hashStr = null;

            onProgress = null;
        }

        public void OnDownload(int bytes)
        {
            downloadBytes = bytes;
            onProgress?.Invoke(this);
        }

        public override void InvokeError()
        {
            isCompleted = true;
            onError?.Invoke(this);

            onComplete = null;
            onError = null;
            RequestPool.MarkRelease(this);
        }

        public override void InvokeComplete()
        {
            isCompleted = true;
            onComplete?.Invoke(this);

            onComplete = null;
            onError = null;

            RequestPool.MarkRelease(this);
        }
        public void Abort()
        {
            isAbort = true;
        }
    }
}