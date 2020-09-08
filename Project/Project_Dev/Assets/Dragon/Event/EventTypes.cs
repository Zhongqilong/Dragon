
public static class EventTypes
{
    //底层socket的连接状态变更到断线状态，后台会进行自动重连，
    //只有一些界面需要断开时就要关掉的，要侦听这个事件
    //如果重连成功可以继续操作，需要更新数据的，侦听 RELOGIN_ACCOUNT 事件
    public const string SOCKET_CLOSE = "socket_close";
    //连接出错,状态未变更到 CONNECT
    public const string SOCKET_ERROR = "socket_error";
    //连接断开,同时可能抛出 SOCKET_CLOSE 或 SOCKET_ERROR 事件
    public const string SOCKET_DISCONNECT = "socket_disconnect";
    //socket连接
    public const string SOCKET_CONNECT = "socket_connect";
    //连接上Gate，可以正常发送逻辑协议
    public const string SOCKET_READY = "socket_ready";
    //等服务器返回可否连接
    public const string SOCKET_WAITING = "socket_waiting";
    //断线后帐号在其他地方登录，返回登录界面
    public const string SWITCH_ACCOUNT = "switch_account";
    //断线后重连成功，数据不变
    public const string RELOGIN_ACCOUNT = "relogin_account";
    public const string NETWORK_NOT_REACHABLE = "NETWORK_NOT_REACHABLE";
    public const string TAB_VIEW_SELECT_CHANGE = "tab_view_select_change";

    public const string EMPTY_SCENE_LOADED = "EMPTY_SCENE_LOADED";
    public const string ALL_SCENE_LOADED = "ALL_SCENE_LOADED";
    public const string REFRESH_PANDORA = "REFRESH_PANDORA";
}