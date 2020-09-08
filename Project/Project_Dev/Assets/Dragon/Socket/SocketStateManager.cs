using UnityEngine;
using Uqee.Events;

public static class SocketStateManager
{
    [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitOnLoad()
    {
        UpdateManager.I.AddCallback(_Update, "SocketStateManager");
    }
    private static BetterList<string> _stateList = new BetterList<string>();
    private static object _stateMutex = new object();
    public static void AddState(string state, int idx = -1)
    {
        lock (_stateMutex)
        {
            if (idx == -1)
            {
                _stateList.Add(state);
            }
            else
            {
                _stateList.Insert(idx, state);
            }
        }
    }
    public static void ClearState()
    {
        lock (_stateMutex)
        {
            _stateList.Clear();
        }
    }
    private static void _Update()
    {
        string[] tmpList = null;
        lock (_stateMutex)
        {
            if (_stateList.size > 0)
            {
                tmpList = new string[_stateList.size];
                for (int i = 0; i < _stateList.size; i++)
                {
                    tmpList[i] = _stateList[i];
                }
                _stateList.Clear();
            }
        }
        if (tmpList != null)
        {
            for (int i = 0; i < tmpList.Length; i++)
            {
                Uqee.Debug.Log($"Socket State:{tmpList[i]}");
                EventManager.current.Dispatch(tmpList[i]);
            }
        }
    }
}