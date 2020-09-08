
using System;

namespace Uqee.Http
{
    public class HttpRequsetData
    {
        public HttpAssetsContentType contType;
        public string url;
        public string postDataStr;
        public string method;
        public string charset;
        public Action<string> callback;


        public void Release()
        {
            callback = null;
        }
        public static HttpRequsetData Create(string url, string postDataStr, Action<string> callback, HttpAssetsContentType contType = HttpAssetsContentType.TEXT, string method = HttpConst.HTTP_GET, string charset = HttpConst.CHARSET_UTF8)
        {
            var data = new HttpRequsetData();// DataFactory<HttpRequsetData>.Get();
            data.url = url;
            data.postDataStr = postDataStr;
            data.callback = callback;
            data.method = method.ToUpper();
            data.charset = charset;
            data.contType = contType;
            return data;
        }
    }
}