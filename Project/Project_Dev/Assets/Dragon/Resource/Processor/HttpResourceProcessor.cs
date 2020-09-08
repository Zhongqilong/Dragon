using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Uqee.Utility;
using LitJson;
using System.Text;
using System.Threading.Tasks;
using System;
using Uqee.Http;

namespace Uqee.Resource
{
    /// <summary>
    /// Http下载资源，zip压缩包，json, 图片等
    /// </summary>
    public class HttpResourceProcessor : AbstractResourceProcessor<HttpResourceProcessor>
    {
        private int MAX_DOWNLOAD_COUNT = 3;
        private const string FILE_PATH = "file:///{0}";
        private int _retryCount;
        private int _downloadCount = 0;
        private Queue<HttpAssetRequest> _reqHttpList = new Queue<HttpAssetRequest>();


        public void AddHttpRequest(HttpAssetRequest req)
        {
            _reqHttpList.Enqueue(req);
        }
        private HttpAssetRequest _GetHttpRequest()
        {
            while (_reqHttpList.Count > 0)
            {
                var req = _reqHttpList.Dequeue();
                if(req.isAbort)
                {
                    req.InvokeError();
                } else
                {
                    return req;
                }
            }
            return null;
        }

        private async void _GetHttpAssets(object o)
        {
            HttpAssetRequest req = (HttpAssetRequest)o;
            try
            {
                await __GetHttpAssets(req);
            }
            catch (Exception e)
            {
                Uqee.Debug.LogError(e);
            }
        }

        private async Task __GetHttpAssets(HttpAssetRequest req)
        {
            int retry = 2;
            string url = req.assetName;
            bool useCdn = false;
            bool noCache = (req.httpContType & HttpAssetsContentType.NO_CACHE) != 0;
            if (!url.StartsWith("http"))
            {
                if (_retryCount == 0)
                {
                    _retryCount = CDNSetting.cdnSize * 2;
                }
                useCdn = true;
                retry = _retryCount;

                if (!noCache && string.IsNullOrEmpty(req.hashStr))
                {
                    req.hashStr = TimeUtils.GetMilliseconds().ToString();
                }
                if (string.IsNullOrEmpty(req.hashStr))
                {
                    url = $"{CDNSetting.currCDN}{req.assetName}";
                }
                else
                {
                    url = $"{CDNSetting.currCDN}{req.assetName}?v={req.hashStr}";
                }
            }
            Uqee.Debug.Log($"[Get HttpAssets] {url}", Color.yellow);
            bool isTexture = (req.httpContType & HttpAssetsContentType.TEXTURE) != 0;
            bool isJson = (req.httpContType & HttpAssetsContentType.JSON) != 0;
            bool isText = (req.httpContType & HttpAssetsContentType.TEXT) != 0;
            bool isGzip = (req.httpContType & HttpAssetsContentType.GZIP) != 0;
            bool saveFile = (req.httpContType & HttpAssetsContentType.SAVE_FILE) != 0;
            string filePath = Path.Combine(DirectorySetting.cacheHttpDir, Path.GetFileName(req.assetName));
            if (saveFile)
            {
                if (File.Exists(filePath))
                {
#if UNITY_EDITOR || UNITY_IOS
                    url = string.Format(FILE_PATH, filePath);
#else
                url = filePath;
#endif
                }
            }

            while (retry > 0)
            {
                req.loadedBytes = await HttpUtils.HttpDownloadAsync(url, req.OnDownload);
                string error = null;
                if (req.loadedBytes == null)
                {
                    error = "download fail";
                }
                else if (!string.IsNullOrEmpty(req.checkMd5))
                {
                    string md5 = CryptUtils.MD5Bytes(req.loadedBytes);
                    if (md5 != req.checkMd5)
                    {
                        error = $"invalid md5:{md5}, need:{req.checkMd5}";
                    }
                }
                if (error != null)
                {
                    Uqee.Debug.LogError($"[Get HttpAssets] failed {retry}/{_retryCount}. {url} :{error}");
                    retry--;
                    if (retry == 0)
                    {
                        req.error = error;
                        break;
                    }
                    else if (useCdn)
                    {
                        if (retry % 2 == 0)
                        {
                            if (!CDNSetting.NextCDN())
                            {
                                req.error = error;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (isGzip)
                    {
                        req.loadedBytes = GZipUtils.UnGzip(req.loadedBytes);
                    }

                    if (isText || isJson)
                    {
                        req.loadedText = Encoding.UTF8.GetString(req.loadedBytes);
                        if (isJson)
                        {
                            req.loadedJson = JsonMapper.ToObject(req.loadedText);
                        }
                    }
                    else if (isTexture)
                    {
                        var tex = new Texture2D(req.w, req.h);
                        tex.LoadImage(req.loadedBytes);

                        req.loadedObj = tex;
                    }
                    if (saveFile)
                    {
                        File.WriteAllBytes(filePath, req.loadedBytes);
                    }
                    break;
                }
            }

            await AsyncTools.ToMainThread();
            if (req.error == null)
            {
                req.InvokeComplete();
            }
            else
            {
                req.InvokeError();
            }
            _downloadCount--;
        }
        public override void Update()
        {
            if (CDNSetting.currCDN == null)
            {
                return;
            }
            if (_downloadCount < MAX_DOWNLOAD_COUNT)
            {
                //var t = TimeUtils.GetMilliseconds();
                //if (t >= _reqTime)
                {
                    var tmpReq = _GetHttpRequest();
                    if (tmpReq != null)
                    {
                        //_reqTime = t + 50;
                        _downloadCount++;
                        Task.Factory.StartNew(_GetHttpAssets, tmpReq);
                    }
                }
            }
        }
    }
}