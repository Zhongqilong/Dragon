using System;
using XLua;

public abstract class Singleton<T>:IHotFix where T : class, new()
{
    private static T _inst;

    public static T I
    {
        get
        {
            if (_inst == null)
            {
                _inst = new T();
                if (_inst != null)
                {
                    (_inst as Singleton<T>).Init();
                }
            }

            return _inst;
        }
    }

    public static void Release()
    {
        if (_inst != null)
        {
            (_inst as Singleton<T>).Dispose();
            _inst = null;
        }
    }

    protected virtual void Init()
    {
    }

    public virtual void Dispose()
    {
        Dragon.Debug.Log( GetType().Name + ".Dispose()");
        _inst = null;
    }

    public static bool IsValid()
	{
		return _inst != null;
	}
}