using System.Collections.Generic;
using System.Text;
using Dragon.Pool;

public static class StringExtendsions
{
    public static string ReplaceChar(this string str, char[] arr, char newChar)
    {
        var sb = DataFactory<StringBuilder>.Get();
        var len = str.Length;
        char c;
        for (int m = 0; m < len; m++)
        {
            c = str[m];
            if (arr.Contains(c))
            {
                sb.Append(newChar);
            }
            else
            {
                sb.Append(c);
            }
        }

        var tmpStr = sb.ToString();
        DataFactory<StringBuilder>.Release(sb);
        return tmpStr;
    }
    public static string ReplaceChar(this string str, char oldChar, char newChar)
    {
        var sb = DataFactory<StringBuilder>.Get();
        var len = str.Length;
        char c;
        for (int m = 0; m < len; m++)
        {
            c = str[m];
            if (oldChar == c)
            {
                sb.Append(newChar);
            }
            else
            {
                sb.Append(c);
            }
        }

        var tmpStr = sb.ToString();
        DataFactory<StringBuilder>.Release(sb);
        return tmpStr;
    }
    /// <summary>
    /// 返回分隔符可以被拆分成多少个字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static int GetSplitCount(this string str, char split)
    {
        var count = 1;
        var len = str.Length;
        for (int j = 0; j < len; j++)
        {
            if (str[j] == split)
            {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// 返回分隔符第一段的字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string GetSplitFirst(this string str, char split)
    {
        var len = str.Length;
        int j = 0;
        for (; j < len; j++)
        {
            if (str[j] == split)
            {
                return str.Substring(0, j);
            }
        }
        return str;
    }
    /// <summary>
    /// 返回分隔符最后一段的字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string GetSplitLast(this string str, char split)
    {
        var len = str.Length;
        int j = len - 1;

        for (; j >= 0; j--)
        {
            if (str[j] == split)
            {
                return str.Substring(j + 1);
            }
        }
        return str;
    }
    /// <summary>
    /// 删除字符串中的某个字符
    /// </summary>
    /// <param name="str"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public static string DeleteChar(this string str, char c)
    {
        var len = str.Length;

        var sb = DataFactory<StringBuilder>.Get();
        for (var j = 0; j < len; j++)
        {
            if (str[j] != c)
            {
                sb.Append(str[j]);
            }
        }
        var tmp = sb.ToString();
        DataFactory<StringBuilder>.Release(sb);
        return tmp;
    }
}