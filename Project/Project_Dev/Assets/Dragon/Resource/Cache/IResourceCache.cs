using UnityEngine;
using UnityEditor;

namespace Uqee.Resource
{
    public interface IResourceCache
    {
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="all">是否全部释放（切换场景时全部释放）</param>
        void ReleaseCache(bool all);
        void Dispose();
    }
}