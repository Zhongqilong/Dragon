using System.Collections.Generic;

public class DataDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public DataDictionary(string filePath)
    {
        _filePath = filePath;
    }

    private string _filePath;

    public new TValue this[TKey key]
    {
        get
        {
            if (!base.TryGetValue(key, out TValue val))
            {
                if (val == null)
                {
                    base[key] = val = ResManager.LoadStreamingJson<TValue>($"Data/{_filePath}{key}.json");
                }
            }
            return val;
        }

        set
        {
            base[key] = value;
        }
    }

    public new bool ContainsKey(TKey key)
    {
        return this[key] != null;
    }

    public new bool TryGetValue(TKey key, out TValue value)
    {
        value = this[key];
        return value != null;
    }
}