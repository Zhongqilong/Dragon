//using System;
//using System.Collections.Generic;

///// <summary>
///// Base interface for Signals
///// </summary>
//public interface ISignal
//{
//    string Hash { get; }
    
//    void SetUpdateHandler(System.Action<ISignal, object> onAdd, System.Action<ISignal, object> onRemove);
//}

//public interface IRemoveable
//{
//    void RemoveHandler(object handler);
//}

///// <summary>
///// Signals main facade class
///// inner+class:A+B;
///// </summary>
//public static class Signals
//{
//    private static Dictionary<Type, ISignal> signals = new Dictionary<Type, ISignal>();
//    private static Dictionary<object, string> handlerDict = new Dictionary<object, string>();

//    [XLua.BlackList]
//    public static SType Get<SType>() where SType : ISignal, new()
//    {
//        Type signalType = typeof(SType);
//        return (SType)_internal_Get(signalType);
//    }

//    public static ISignal Get(string typeName)
//    {
//        Type signalType = Type.GetType(typeName);
//        return _internal_Get(signalType) as ISignal;
//    }

//    /// <summary>只Get不创建,无法知道创建</summary>
//    public static ISignal GetByHash(string hashName)
//    {
//        foreach(var t in signals.Values)
//        {
//            if(t.Hash == hashName)
//            {
//                return t;
//            }
//        }
//        return null;
//    }

//    private static ISignal _internal_Get(Type signalType)
//    {
//        ISignal signal;
//        if (signals.TryGetValue(signalType, out signal))
//        {
//            return signal as ISignal;
//        }
//        return Bind(signalType) as ISignal;
//    }

//    public static void RemoveHandler(System.Object handler)
//    {
//        if (handler == null)
//            return;
//        if (handlerDict.ContainsKey(handler))
//        {
//            var signal = GetSignalByHash(handlerDict[handler]);
//            (signal as IRemoveable)?.RemoveHandler(handler);
//        }
//    }

//    private static ISignal Bind(Type signalType)
//    {
//        ISignal signal;
//        if (signals.TryGetValue(signalType, out signal))
//        {
//            Uqee.Debug.LogError(string.Format("Signal already registered for type {0}", signalType.ToString()));
//            return signal;
//        }

//        signal = (ISignal)Activator.CreateInstance(signalType);
//        signals.Add(signalType, signal);
//        signal.SetUpdateHandler(_OnAddHandler,_RemoveHandler);
//        return signal;
//    }

//    private static void _OnAddHandler(ISignal signal,System.Object handler)
//    {
//        //UnityEngine.Debug.Assert(!handlerDict.ContainsKey(handler)|| handlerDict[handler] == signal.Hash,"duplicate add");
//        handlerDict[handler] = signal.Hash;
//    }

//    private static void _RemoveHandler(ISignal signal, System.Object handler)
//    {
//        handlerDict.Remove(signal);
//    }

//    private static ISignal Bind<T>() where T : ISignal, new()
//    {
//        return Bind(typeof(T));
//    }

//    private static ISignal GetSignalByHash(string signalHash)
//    {
//        foreach (ISignal signal in signals.Values)
//        {
//            if (signal.Hash == signalHash)
//            {
//                return signal;
//            }
//        }
//        return null;
//    }
//}

///// <summary>
///// Abstract class for Signals, provides hash by type functionality
///// </summary>
//public abstract class ABaseSignal : ISignal
//{
//    protected string _hash;
//    protected System.Action<ISignal, object> onAdd;
//    protected System.Action<ISignal, object> onRemove;

//    /// <summary>
//    /// Unique id for this signal
//    /// </summary>
//    public string Hash
//    {
//        get
//        {
//            if (string.IsNullOrEmpty(_hash))
//            {
//                _hash = this.GetType().ToString();
//            }
//            return _hash;
//        }
//    }

//    public void SetUpdateHandler(System.Action<ISignal, object> onAdd, System.Action<ISignal, object> onRemove)
//    {
//        this.onAdd = onAdd;
//        this.onRemove = onRemove;
//    }

//    protected void _CheckCompareableType(Type type)
//    {
//        if (type.IsValueType)
//        {
//            //int CompareTo(T other)
//            var method = type.GetMethod("CompareTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
//            var parameters = method.GetParameters();
//            UnityEngine.Debug.Assert(parameters.Length == 1);
//            UnityEngine.Debug.Assert(parameters[0].ParameterType == type);
//        }
//    }
//}

///// <summary>
///// Strongly typed messages with no parameters
///// </summary>
//public abstract class ASignal : ABaseSignal, IRemoveable
//{
//    private List<System.Action> callback = new List<System.Action>(4);

//    /// <summary>
//    /// Adds a listener to this Signal
//    /// </summary>
//    /// <param name="handler">Method to be called when signal is fired</param>
//    public System.Action AddListener(System.Action handler,bool useOnce)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//           "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!callback.Contains(handler))
//        {
//            callback.Add(handler);
//            onAdd(this,handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action handler)
//    {
//        if (callback.Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action);
//    }

//    /// <summary>
//    /// Dispatch this signal
//    /// </summary>
//    public void Dispatch()
//    {
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch");
//#endif
//        if (callback != null)
//        {
//            for (int i = callback.Count - 1; i >= 0; i--)
//            {
//                callback[i]?.Invoke();
//            }
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}

///// <summary>
///// Strongly typed messages with 1 parameter
///// </summary>
///// <typeparam name="T">Parameter type</typeparam>
//public abstract class ASignal<T> : ABaseSignal, IRemoveable
//{
//    private List<System.Action<T>> callback = new List<System.Action<T>>(4);

//    /// <summary>
//    /// Adds a listener to this Signal
//    /// </summary>
//    /// <param name="handler">Method to be called when signal is fired</param>
//    public System.Action<T> AddListener(System.Action<T> handler)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//            "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!callback.Contains(handler))
//        {
//            callback.Add(handler);
//            onAdd(this, handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action<T> handler)
//    {
//        if(callback.Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action<T>);
//    }

//    /// <summary>
//    /// Dispatch this signal with 1 parameter
//    /// </summary>
//    public void Dispatch(T arg1)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch");
//#endif
//        if (callback != null)
//        {
//            for (int i = callback.Count - 1; i >= 0; i--)
//            {
//                callback[i]?.Invoke(arg1);
//            }
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}

///// <summary>
///// Strongly typed messages with 2 parameters
///// </summary>
///// <typeparam name="T">First parameter type</typeparam>
///// <typeparam name="U">Second parameter type</typeparam>
//public abstract class ASignal<T, U> : ABaseSignal, IRemoveable
//{
//    private List<System.Action<T, U>> callback = new List<System.Action<T, U>>(4);

//    /// <summary>
//    /// Adds a listener to this Signal
//    /// </summary>
//    /// <param name="handler">Method to be called when signal is fired</param>
//    public System.Action<T, U> AddListener(System.Action<T, U> handler)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//            "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!callback.Contains(handler))
//        {
//            callback.Add(handler);
//            onAdd(this, handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action<T, U> handler)
//    {
//        if (callback.Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action<T, U>);
//    }

//    /// <summary>
//    /// Dispatch this signal
//    /// </summary>
//    public void Dispatch(T arg1, U arg2)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch");
//#endif
//        if (callback != null)
//        {
//            for (int i = callback.Count - 1; i >= 0; i--)
//            {
//                callback[i]?.Invoke(arg1, arg2);
//            }
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}

///// <summary>
///// Strongly typed messages with 3 parameter
///// </summary>
///// <typeparam name="T">First parameter type</typeparam>
///// <typeparam name="U">Second parameter type</typeparam>
///// <typeparam name="V">Third parameter type</typeparam>
//public abstract class ASignal<T, U, V> : ABaseSignal,IRemoveable
//{
//    private List<System.Action<T, U, V>> callback = new List<System.Action<T, U, V>>(4);

//    /// <summary>
//    /// Adds a listener to this Signal
//    /// </summary>
//    /// <param name="handler">Method to be called when signal is fired</param>
//    public System.Action<T, U, V> AddListener(System.Action<T, U, V> handler)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//            "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!callback.Contains(handler))
//        {
//            callback.Add(handler);
//            onAdd(this, handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action<T, U, V> handler)
//    {
//        if (callback.Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action<T, U, V>);
//    }

//    /// <summary>
//    /// Dispatch this signal
//    /// </summary>
//    public void Dispatch(T arg1, U arg2, V arg3)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch");
//#endif
//        if (callback != null)
//        {
//            for (int i = callback.Count - 1; i >= 0; i--)
//            {
//                callback[i]?.Invoke(arg1, arg2, arg3);
//            }
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}

//public abstract class AbstractDataSignal<T> : ABaseSignal, IRemoveable
//{
//    private Dictionary<T, List<System.Action<T>>> protoHandlerDict = new Dictionary<T, List<System.Action<T>>>();
//    private Dictionary<object, T> handlerDict = new Dictionary<object, T>();

//    public System.Action<T> AddListener(T proto_id, System.Action<T> handler)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//            "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!protoHandlerDict.ContainsKey(proto_id))
//        {
//            protoHandlerDict[proto_id] = new List<System.Action<T>>(4);
//        }
//        if (!protoHandlerDict[proto_id].Contains(handler))
//        {
//            //UnityEngine.Debug.Assert(!handlerDict.ContainsKey(handler) || (handlerDict[handler].GetHashCode() == proto_id.GetHashCode()), "one handler multi key");
//            handlerDict[handler] = proto_id;
//            protoHandlerDict[proto_id].Add(handler);
//            onAdd(this, handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action<T> handler)
//    {
//        if (handlerDict.ContainsKey(handler))
//        {
//            var proto_id = handlerDict[handler];
//            RemoveListener(proto_id, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action<T>);
//    }

//    private void RemoveListener(T proto_id, System.Action<T> handler)
//    {
//        handlerDict.Remove(handler);
//        if (protoHandlerDict.ContainsKey(proto_id) && protoHandlerDict[proto_id].Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    /// <summary>
//    /// Dispatch this signal
//    /// </summary>
//    public void Dispatch(T proto_id)
//    {
//        #if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch"); 
//        #endif

//        if (protoHandlerDict.ContainsKey(proto_id))
//        {
//            foreach (var handler in protoHandlerDict[proto_id])
//                handler?.Invoke(proto_id);
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}

//public abstract class AbstractDataSignal<T, U> : ABaseSignal, IRemoveable
//{
//    private Dictionary<T, List<System.Action<T,U>>> protoHandlerDict = new Dictionary<T, List<System.Action<T,U>>>();
//    private Dictionary<object, T> handlerDict = new Dictionary<object, T>();

//    public System.Action<T,U> AddListener(T proto_id, System.Action<T,U> handler)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//            "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!protoHandlerDict.ContainsKey(proto_id))
//        {
//            protoHandlerDict[proto_id] = new List<System.Action<T,U>>();
//        }
//        if (!protoHandlerDict[proto_id].Contains(handler))
//        {
//            //UnityEngine.Debug.Assert(!handlerDict.ContainsKey(handler)|| (handlerDict[handler].GetHashCode() == proto_id.GetHashCode()),"one handler multi key");
//            handlerDict[handler] = proto_id;
//            protoHandlerDict[proto_id].Add(handler);
//            onAdd(this, handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action<T,U> handler)
//    {
//        if (handlerDict.ContainsKey(handler))
//        {
//            var proto_id = handlerDict[handler];
//            RemoveListener(proto_id, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action<T,U>);
//    }

//    private void RemoveListener(T proto_id, System.Action<T,U> handler)
//    {
//        handlerDict.Remove(handler);
//        if (protoHandlerDict.ContainsKey(proto_id) && protoHandlerDict[proto_id].Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    /// <summary>
//    /// Dispatch this signal
//    /// </summary>
//    public void Dispatch(T proto_id, U data)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch");
//#endif
//        if (protoHandlerDict.ContainsKey(proto_id))
//        {
//            for (int i = protoHandlerDict[proto_id].Count - 1; i >= 0; i--)
//            {
//                protoHandlerDict[proto_id][i]?.Invoke(proto_id,data);
//            }
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}

//public abstract class AbstractDataSignal<T, U,V> : ABaseSignal, IRemoveable
//{
//    private Dictionary<T, List<System.Action<T,U, V>>> protoHandlerDict = new Dictionary<T, List<System.Action<T,U, V>>>();
//    private Dictionary<object, T> handlerDict = new Dictionary<object, T>();
    
//    public System.Action<T,U,V> AddListener(T proto_id, System.Action<T,U,V> handler)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Debug.Assert(!handler.Method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false),
//            "Adding anonymous delegates as Signal callbacks is not supported (you wouldn't be able to unregister them later).");
//#endif
//        if (!protoHandlerDict.ContainsKey(proto_id))
//        {
//            protoHandlerDict[proto_id] = new List<System.Action<T,U, V>>(4);
//        }
//        if (!protoHandlerDict[proto_id].Contains(handler))
//        {
//            //UnityEngine.Debug.Assert(!handlerDict.ContainsKey(handler) || (handlerDict[handler].GetHashCode() == proto_id.GetHashCode()), "one handler multi key");
//            handlerDict[handler] = proto_id;
//            protoHandlerDict[proto_id].Add(handler);
//            onAdd(this, handler);
//        }
//        return handler;
//    }

//    /// <summary>
//    /// Removes a listener from this Signal
//    /// </summary>
//    /// <param name="handler">Method to be unregistered from signal</param>
//    public void RemoveListener(System.Action<T,U,V> handler)
//    {
//        if (handlerDict.ContainsKey(handler))
//        {
//            var proto_id = handlerDict[handler];
//            RemoveListener(proto_id, handler);
//        }
//    }

//    void IRemoveable.RemoveHandler(object handler)
//    {
//        RemoveListener(handler as System.Action<T,U,V>);
//    }

//    private void RemoveListener(T proto_id, System.Action<T,U,V> handler)
//    {
//        handlerDict.Remove(handler);
//        if (protoHandlerDict.ContainsKey(proto_id) && protoHandlerDict[proto_id].Remove(handler))
//        {
//            onRemove(this, handler);
//        }
//    }

//    /// <summary>
//    /// Dispatch this signal
//    /// </summary>
//    public void Dispatch(T proto_id, U data,V param)
//    {
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.BeginSample("Signals Dispatch");
//#endif
//        if (protoHandlerDict.ContainsKey(proto_id))
//        {
//            for (int i = protoHandlerDict[proto_id].Count - 1; i >= 0; i--)
//            {
//                protoHandlerDict[proto_id][i]?.Invoke(proto_id, data, param);
//            }
//        }
//#if UNITY_EDITOR
//        UnityEngine.Profiling.Profiler.EndSample();
//#endif
//    }
//}