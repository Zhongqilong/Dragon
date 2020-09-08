
using Uqee.Pool;
public class TabItemData
{
    public string name;
    public bool isShow = true;
    /// <summary>
    /// 如果不设置该属性 会按照数组索引默认赋值
    /// </summary>
    public int index = -1;
    public bool useInPop = false;
    public bool isSquare = false;

    public static TabItemData Create(string name, System.Action<int, bool> a=null, int index = -1, bool isInPop = false, bool isSquare = false)
    {
        var data = DataFactory<TabItemData>.Get();
        data.name = name;
        data.index = index;
        data.useInPop = isInPop;
        data.isSquare = isSquare;
        return data;
    }
    public static void Release(TabItemData data)
    {
        if (data == null)
        {
            return;
        }
        DataFactory<TabItemData>.Release(data);
    }
}
