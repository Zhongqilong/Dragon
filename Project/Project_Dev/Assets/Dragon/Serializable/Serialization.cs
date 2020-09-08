using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Serialization<T>
{
    [SerializeField]
    private List<T> target;

    public List<T> ToList()
    {
        return target;
    }

    public Serialization(List<T> target)
    {
        this.target = target;
    }
}

// Dictionary<TKey, TValue>
[Serializable]
public class Serialization<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys;

    [SerializeField]
    private List<TValue> values;

    private Dictionary<TKey, TValue> target;

    public Dictionary<TKey, TValue> ToDictionary()
    {
        return target;
    }

    public Serialization(Dictionary<TKey, TValue> target)
    {
        this.target = target;
    }

    public void OnBeforeSerialize()
    {
        keys = new List<TKey>(target.Keys);
        values = new List<TValue>(target.Values);
    }

    public void OnAfterDeserialize()
    {
        var count = Math.Min(keys.Count, values.Count);
        target = new Dictionary<TKey, TValue>(count);
        for (var i = 0; i < count; ++i)
        {
            target[keys[i]] = values[i];
        }
    }
}