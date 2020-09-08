
namespace Uqee.Socket
{
    public static class SocketSetting
    {
        /// <summary>
        /// 服务器状态是否开启
        /// </summary>
        public static bool open = true;
        /// <summary>
        /// 是否使用服务器代理
        /// </summary>
        public static bool proxy = false;
        /// <summary>
        /// sock4a协议
        /// </summary>
        public static bool isSock4a = true;
        /// <summary>
        /// 数据是否加密
        /// </summary>
        public static bool encrypt = false;
        public static bool decrypt = false;
        /// <summary>
        /// 服务器端口
        /// </summary>
        public static int port = 9118;
        /// <summary>
        /// 服务器地址
        /// </summary>
        public static string host;
        /// <summary>
        /// 服务器名称
        /// </summary>
        public static string name;
        public static string selectServerId;
        /// <summary>
        /// 发送密钥
        /// </summary>
        public static string sendKey = string.Empty;
        /// <summary>
        /// 接收密钥
        /// </summary>
        public static string recvKey = "P%2BViyZLtO^gRT2Huxqx#5Vygbfl$8m";

        /// <summary>
        /// 每次最大接收数据的缓冲区大小
        /// </summary>
        public static int maxRecvSize = 200 * 1024;
        /// <summary>
        /// 数据接收间隔
        /// </summary>
        public static int sleepTime = 17;
    }
}