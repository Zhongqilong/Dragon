using System.Collections.Generic;
using System.Text;

public static partial class ListExtensions
{
    private static StringBuilder _strBuf = new StringBuilder(96);
    public static bool Contains<T>(this T[] array, T val)
    {
        if (array == null)
        {
            return false;
        }
        var len = array.Length;
        for (int i = 0; i < len; i++)
        {
            if (array[i].Equals(val)) return true;
        }
        return false;
    }
    public static string ToJoinString<T>(this T[] array, string split = ",", string format = null)
    {
        _strBuf.Clear();
        int i = 0;
        int num = array.Length;
        while (i < num)
        {
            _ToJoinString<T>(array[i], split, format, i == num - 1);
            i++;
        }
        return _strBuf.ToString();
    }

    public static string ToJoinString<T>(this List<T> list, string split = ",", string format = null)
    {
        _strBuf.Clear();
        int i = 0;
        int count = list.Count;
        while (i < count)
        {
            _ToJoinString<T>(list[i], split, format, i == count - 1);
            i++;
        }
        return _strBuf.ToString();
    }

    private static void _ToJoinString<T>(T element, string split, string format, bool last)
    {
        if (format != null && element is int)
        {
            _strBuf.Append(((int)((object)element)).ToString(format));
        }
        else if (format != null && element is float)
        {
            _strBuf.Append(((float)((object)element)).ToString(format));
        }
        else
        {
            _strBuf.Append(element.ToString());
        }
        if (!last)
        {
            _strBuf.Append(split);
        }
    }
    public static bool IsNullOrEmpty<T>(this IList<T> dataList)
    {
        return dataList == null || dataList.Count == 0;
    }

    public static bool IsNullOrEmpty<T>(this T[] dataList)
    {
        return dataList == null || dataList.Length == 0;
    }


    public static bool IsNullOrEmpty(this string dataList)
    {
        return dataList == null || dataList.Length == 0;
    }

    public static void RemoveNull<T>(this List<T> dataList) where T : class
    {
        if (dataList.IsNullOrEmpty())
            return;
        var len = dataList.Count;
        int j = 0;
        for (int i = 0; i < len; i++)
        {
            if (dataList[i] != null)
            {
                dataList[j++] = dataList[i];
            }
        }
        dataList.RemoveRange(j, len - j);
    }

    public static Google.Protobuf.Collections.RepeatedField<T> ToProto3List<T>(this T[] arr)
    {
        var list = new Google.Protobuf.Collections.RepeatedField<T>();
        list.Add(arr);
        return list;
    }

    public static bool IsNullOrEmpty<T>(this Google.Protobuf.Collections.RepeatedField<T> dataList)
    {
        return dataList == null || dataList.Count == 0;
    }
    public static T[] ToArray<T>(this Google.Protobuf.Collections.RepeatedField<T> dataList)
    {
        if (dataList == null) return null;
        var len = dataList.Count;
        T[] arr = new T[len];
        for(int i=0; i<len; i++)
        {
            arr[i] = dataList[i];
        }
        return arr;
    }
    public static List<T> ReverseList<T>(this Google.Protobuf.Collections.RepeatedField<T> dataList)
    {
        if (dataList == null) return null;
        var len = dataList.Count;
        List<T> arr = new List<T>(len);
        for(int i= dataList.Count-1; i>=0; i--)
        {
            arr.Add( dataList[i] );
        }
        return arr;
    }
    public static List<T> ToList<T>(this Google.Protobuf.Collections.RepeatedField<T> dataList)
    {
        if (dataList == null) return null;
        var len = dataList.Count;
        List<T> arr = new List<T>(len);
        for (int i = 0; i < len; i++)
        {
            arr.Add(dataList[i]);
        }
        return arr;
    }
    public static T Find<T>(this Google.Protobuf.Collections.RepeatedField<T> dataList, System.Func<T, bool> finder)
    {
        if (dataList == null) return default;
        var cnt = dataList.Count;
        for (int i = 0; i < cnt; i++)
        {
            if(finder(dataList[i]))
            {
                return dataList[i];
            }
        }
        return default;
    }
    public static void Sort<T>(this Google.Protobuf.Collections.RepeatedField<T> dataList, System.Comparison<T> comparison)
    {
        if (dataList == null) return ;
        var list = dataList.ToList();
        list.Sort(comparison);
        var len = list.Count;
        for(int i=0; i<len; i++)
        {
            dataList[i] = list[i];
        }
    }
}