using UnityEngine;
using System.Collections;

namespace Uqee.Resource
{
    public interface IDependenciesAdapter 
    {
        /// <summary>
        /// 检查请求的依赖，并添加依赖的请求(异步)
        /// </summary>
        /// <param name="req"></param>
        void CheckDepsAsync(AssetRequest req);
        /// <summary>
        /// 添加依赖的异步请求
        /// </summary>
        /// <param name="req"></param>
        /// <param name="deps"></param>
        /// <param name="depCategory"></param>
        /// <param name="needCovertCategory"></param>
        void LoadDepsAsync(AssetRequest req, string[] deps, string depCategory);

        /// <summary>
        /// 检查请求的依赖，并添加依赖的请求(同步)
        /// </summary>
        /// <param name="category"></param>
        /// <param name="assetName"></param>
        /// <param name="isSystemAssets"></param>
        void CheckDepsSync(AssetRequest req);
        /// <summary>
        /// 添加依赖的同步请求
        /// </summary>
        /// <param name="deps"></param>
        /// <param name="depCategory"></param>
        /// <param name="isSystemAssets"></param>
        void LoadDepsSync(string[] deps, string depCategory, bool isSystemAssets = false);
    }
}
