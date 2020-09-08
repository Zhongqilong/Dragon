using System;
using UnityEngine;
using Uqee.Pool;

public struct TabViewParam
{
    //TAB颜色切换，0=selected, 1=un selected
    public Color[] colorArr;
    //0第一个节点的"层级"最大,1不处理层级，其他：最后一个节点的层级最大
    public int zorderType;
    public int defaultTabIdx;
    public Action<int> callback;
    public TabViewCfg[] cfgArr;
    public static TabViewParam Create(TabViewCfg[] cfgArr, Action<int> callback = null, Color[] colorArr = null, int zorderType = 1, int defaultTabIdx = 0)
    {
        var param = DataFactory<TabViewParam>.Get();
        param.cfgArr = cfgArr;
        param.colorArr = colorArr;
        param.callback = callback;
        param.zorderType = zorderType;
        param.defaultTabIdx = defaultTabIdx;
        return param;
    }
    public static void Release(TabViewParam param)
    {
        param.callback = null;
        param.colorArr = null;
        if (param.cfgArr != null)
        {
            for (int i = 0; i < param.cfgArr.Length; i++)
            {
                TabViewCfg.Release(param.cfgArr[i]);
            }
            param.cfgArr = null;
        }
        DataFactory<TabViewParam>.Release(param);
    }
}