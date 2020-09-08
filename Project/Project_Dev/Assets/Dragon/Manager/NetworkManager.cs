using UnityEngine;
using Dragon.Events;

public static class NetworkManager
{
    public static NetworkReachability internetReachability;

    [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitOnLoad()
    {
        internetReachability = Application.internetReachability;
        UpdateManager.I.AddCallback(_Update, "NetworkManager");
    }
    private static float _lastCheckInternet = 0;
    private static bool hasNetwork;
    
    /// <summary>
    /// 网络是否已连接，没有连接时，发出事件 EventTypes.NETWORK_NOT_REACHABLE
    /// </summary>
    /// <returns></returns>
    public static bool HasNetwork()
    {
        if( internetReachability == UnityEngine.NetworkReachability.NotReachable)
        {
            if (hasNetwork)
            {
                hasNetwork = false;
                EventManager.current.Dispatch(EventTypes.NETWORK_NOT_REACHABLE);
            }
            return false;
        }
        hasNetwork = true;
        return true;
            
    }
    public static bool Is4G()
    {
        return internetReachability == UnityEngine.NetworkReachability.ReachableViaCarrierDataNetwork;
    }
    public static void ForceUpdate()
    {
        _lastCheckInternet = 0;
        _Update();
    }

    private static void _Update()
    {
        var t = AppStatus.realtimeSinceStartup;
        if (t - _lastCheckInternet > 1)
        {
            internetReachability = Application.internetReachability;
            _lastCheckInternet = t;
        }
    }
}
