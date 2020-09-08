using UnityEngine;
using System;
using System.Threading;

public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;
    static int numThreads;

    private static Loom _current;
    //private int _count;
    public static Loom Current
    {
        get
        {
            Initialize();
            return _current;
        }
    }

    void Awake()
    {
        _current = this;
        initialized = true;
    }

    static bool initialized;

    [UnityEngine.RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        if (!initialized)
        {
            initialized = true;
            var g = new GameObject("Loom");
            _current = g.AddComponent<Loom>();
            UnityEngine.Object.DontDestroyOnLoad(g);
        }

    }
    public struct NoDelayedQueueItem
    {
        public Action<object> action;
        public object param;
    }

    private BetterList<NoDelayedQueueItem> _actions = new BetterList<NoDelayedQueueItem>();
    public struct DelayedQueueItem
    {
        public float time;
        public Action<object> action;
        public object param;
    }
    private BetterList<DelayedQueueItem> _delayed = new BetterList<DelayedQueueItem>();

    private BetterList<DelayedQueueItem> _currentDelayed = new BetterList<DelayedQueueItem>();

    public static void QueueOnMainThread(Action<object> taction, object tparam)
    {
        QueueOnMainThread(taction, tparam, 0f);
    }
    public static void QueueOnMainThread(Action<object> taction, object tparam, float time)
    {
        if(Current==null)
        {
            return;
        }
        if (time != 0)
        {
            lock (Current._delayed)
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = taction, param = tparam });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(new NoDelayedQueueItem { action = taction, param = tparam });
            }
        }
    }

    public static Thread RunAsync(Action a)
    {
        Initialize();
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(100);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }

    }


    void OnDisable()
    {
        if (_current == this)
        {

            _current = null;
        }
    }

    BetterList<NoDelayedQueueItem> _currentActions = new BetterList<NoDelayedQueueItem>();

    // Update is called once per frame
    void Update()
    {
        if (_actions.size > 0)
        {
            lock (_actions)
            {
                _currentActions.Clear();
                for (int i = 0; i < _actions.size; i++)
                {
                    _currentActions.Add(_actions[i]);
                }
                _actions.Clear();
            }
            for (int i = 0; i < _currentActions.size; i++)
            {
                _currentActions[i].action(_currentActions[i].param);
            }
        }

        if (_delayed.size > 0)
        {
            lock (_delayed)
            {
                _currentDelayed.Clear();
                var t = Time.time;                
                for (int i = 0; i < _delayed.size; i++)
                {
                    var act = _delayed[i];
                    if( act.time<=t )
                    {
                        _currentDelayed.Add(_delayed[i]);
                        _delayed.RemoveAt(i);
                        i--;
                    }                    
                }
            }

            for (int i = 0; i < _currentDelayed.size; i++)
            {
                _currentDelayed[i].action(_currentDelayed[i].param);
            }
        }
    }
}