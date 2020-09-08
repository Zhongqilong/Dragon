
using System;
using Uqee.Pool;

public struct TabViewCfg
{
    public string tabName;
    public string viewName;

    //对应 functionopen.xml中的key
    public int funcId;
    public Func<int, bool> onOpenCheck;
    //功能未开放时是否隐藏。
    public bool hideWhenClose;
    public bool isLua;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewName">标签内容加载的prefab名称</param>
    /// <param name="tabName">标签页节点名称，空的话按配置顺序获取</param>
    /// <param name="funcId">开放功能key,显示/隐藏对应的Tab</param>
    /// <param name="isLua"></param>
    /// <returns></returns>
    public static TabViewCfg Create(string viewName, string tabName = null, int funcId = 0, bool hideWhenClose = true, bool isLua = false, Func<int, bool> onOpenCheck =null)
    {
        var cfg = DataFactory<TabViewCfg>.Get();
        cfg.tabName = tabName;
        cfg.viewName = viewName;
        cfg.funcId = funcId;
        cfg.isLua = isLua;
        cfg.hideWhenClose = hideWhenClose;
        cfg.onOpenCheck = onOpenCheck;
        return cfg;
    }
    public static void Release(TabViewCfg cfg)
    {
        cfg.tabName = null;
        cfg.viewName = null;
        cfg.funcId = 0;
        cfg.onOpenCheck = null;
        DataFactory<TabViewCfg>.Release(cfg);
    }
}