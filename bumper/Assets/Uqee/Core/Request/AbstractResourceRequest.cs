using System;
using Uqee.Pool;

namespace Uqee.Resource
{
    public abstract class AbstractResourceRequest<T>: IResourceRequest where T : class, new()
    {
        public string category;
        public volatile bool isCompleted;
        /// <summary>
        /// 在切换场景时，不要清除掉请求
        /// </summary>
        public bool donotClear;
        public bool isError => error != null;

        /// <summary>
        /// 错误信息，加载出错时 isComplete=true, error!=null
        /// </summary>
        public string error;

        public string assetName;
        private string _prefabName;

        public string prefabName
        {
            get
            {
                if (_prefabName == null && !string.IsNullOrEmpty(assetName))
                {
                    _prefabName = assetName.GetSplitLast('/');
                }
                return _prefabName;
            }
        }

        /// <summary>
        /// 根据 category和assetName生成的路径
        /// </summary>
        public volatile string assetPath;

        public UnityEngine.Object loadedObj;

        /// <summary>
        /// 加载完成时回调
        /// </summary>
        public Action<T> onComplete;

        public Action<T> onError;
        
        public virtual void Release()
        {
            category = null;
            assetPath = null;
            isCompleted = false;
            donotClear = false;
            error = null;

            loadedObj = null;
            assetName = null;
            onComplete = null;
            onError = null;
            _prefabName = null;

            DataFactory<T>.Release(this as T);
        }
        public virtual void InvokeError()
        {
        }

        public virtual void InvokeComplete()
        {
        }
    }
}