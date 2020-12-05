using System;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _inst;

	public static T I
	{
		get
		{
			if (_inst == null)
			{
				_inst = FindObjectOfType<T>();
				if (_inst == null)
				{
					//Log.Error(LOG.SYSTEM, typeof(T) + " is nothing", new object[0]);
				}
			}
			return _inst;
		}
	}

	private void OnDestroy()
	{
        if (!AppStatus.isApplicationQuit)
        {
            _OnDestroy();
        }
        if (_inst == this)
		{
			OnDestroySingleton();
			_inst = null;
		}
	}

	protected virtual void _OnDestroy()
	{
	}

	protected virtual void OnDestroySingleton()
	{
	}

	protected virtual void Awake()
	{
		Check_inst();
        OnAwake();

    }

    protected virtual void OnAwake()
    {
       
    }

    protected bool Check_inst()
	{
		if (this == I)
		{
			return true;
		}
        if (this != null)
        {
            Destroy(this);
        }
		return false;
	}

	public static bool IsValid()
	{
		return _inst != null;
	}
}
