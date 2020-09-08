using System.Collections.Generic;

public class SafeGetList<T> : List<T>
{
    public SafeGetList(int capacity = 32) : base(capacity)
    {
    }

    public new T this[int index]
    {
        get
        {
            CheckOrGrow(index);
            return base[index];
        }

        set
        {
            CheckOrGrow(index);
            base[index] = value;
        }
    }

    private void CheckOrGrow(int index)
    {
        if (index >= this.Count)
        {
            for (int i = Count; i < index + 1; i++)
            {
                this.Add(default(T));
            }
        }
    }
}