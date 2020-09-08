using UnityEngine;
using UnityEditor;

namespace Uqee.Resource
{
    public interface IInstantiateCache : IResourceCache
    {
        bool IsInPool(string category, string name);
        bool CanSpawn(string category, string name);
        /// <summary>
        /// 返回是否回收成功
        /// </summary>
        /// <param name="go"></param>
        /// <param name="worldPositionStays"></param>
        /// <returns></returns>
        bool Despawn(GameObject go, bool worldPositionStays = false);
        Transform Spawn(string category, string name, Transform parent);
        void AddInstantiatePool(string category, GameObject prefabObj);
    }
}