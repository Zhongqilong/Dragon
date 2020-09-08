namespace Uqee.Http
{
    public static class HttpConst
    {
        public const string ERROR_JSON = "{\"code\":1001}";
        public const string NETWORK_ERROR_JSON = "{\"code\":9999}";
        public const string HTTP_DOWNLOAD = "application/octet-stream";
        public const string HTTP_GET_CHARSET = "text/plain;charset={0}";
        public const string HTTP_POST_CHARSET = "application/x-www-form-urlencoded;charset={0}";
        public const string HTTP_GET = "GET";
        public const string HTTP_POST = "POST";
        public const string CHARSET_GB2312 = "gb2312";
        public const string CHARSET_UTF8 = "utf-8";
        public const int BUFF_SIZE = 500 * 1024;
    }
}