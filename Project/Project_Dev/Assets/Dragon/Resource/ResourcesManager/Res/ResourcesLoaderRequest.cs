using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Uqee.Resource
{
    public class ResourcesLoaderRequest : LoadRequestBase
    {

        /// <summary>
        /// 异步ResourceRequest
        /// </summary>
        public ResourceRequest request;


        /// <summary>
        /// 当前异步对象
        /// </summary>
        private AsyncOperation async;

        public string assetname;

        public System.Action<Object> onAssetLoaded;

        public override bool isDone
        {
            get
            {
                return async.isDone;
            }
        }

        public override float progress
        {
            get
            {
                return async.progress;
            }
        }

        public override int priority
        {
            get
            {
                return async.priority;
            }
            set
            {
                async.priority = value;
            }
        }


        public override bool allowSceneActivation
        {
            get { return async.allowSceneActivation; }
            set { async.allowSceneActivation = value; }
        }

        public UnityEngine.Object asset
        {
            get
            {
                if (request != null)
                {
                    return request.asset;
                }
                return null;
            }
        }


        public T GetAsset<T>() where T : UnityEngine.Object
        {
            return asset as T;
        }

        public ResourcesLoaderRequest(ResourceRequest asyncOperation, System.Action<UnityEngine.Object> onAssetLoaded)
        {
            request = asyncOperation;
            async = request;
            async.completed += Request_completed;
            this.onAssetLoaded = onAssetLoaded;
        }


        private void Request_completed(AsyncOperation obj)
        {
            async.completed -= Request_completed;
            onAssetLoaded?.Invoke(asset);
        }


    }
}