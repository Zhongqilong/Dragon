
using System;
using System.Collections.Generic;

/// <summary>
/// 说明：proto网络数据缓存池工厂类
/// 
/// @by wsh 2017-07-01
/// </summary>

public static class ProtoFactory
{
    /// <summary>
    /// 获取协议数据对象，如果没缓存，则创建
    /// 注意：游戏逻辑代码中，最好使用这个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Get<T>() where T : class, new()
    {
        return new T();
    }
}
