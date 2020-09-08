using System.Collections;

namespace Uqee.Resource
{
    /// <summary>
    /// 加载资源适配器（Resources/AssetBundle）
    /// </summary>
    public interface ILoadAdapter
    {
        T LoadSync<T>(AssetRequest req) where T : UnityEngine.Object;
        IEnumerator LoadAsync(AssetRequest req);
    }
}