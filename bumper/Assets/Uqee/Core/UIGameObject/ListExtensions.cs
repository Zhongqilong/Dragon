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
}