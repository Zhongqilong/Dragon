using System;

public abstract class Singleton<T> where T : class, new () {
    private static T _instance;

    public static T I {
        get {
            if (_instance == null) {
                _instance = new T ();
                if (_instance != null) {
                    (_instance as Singleton<T>).Init ();
                }
            }
            return _instance;
        }
    }

    protected static void Release () {
        if (_instance != null) {
            (_instance as Singleton<T>).Dispose ();
        }
    }

    protected virtual void Init () {}

    public virtual void Dispose () {
        _instance = null;
    }

    public static bool IsValid () {
        return _instance != null;
    }
}