using System;
using UnityEngine;

namespace Uqee.Resource
{
    public class InstantiateRequest : AbstractResourceRequest<InstantiateRequest>
    {
        public Transform parent;

        override public void Release()
        {
            base.Release();
            parent = null;
        }

        public override void InvokeComplete()
        {
            isCompleted = true;
            try
            {
                onComplete?.Invoke(this);
            }
            catch (Exception ex)
            {
                Uqee.Debug.LogError(ex);
            }
            RequestPool.MarkRelease(this);
        }

        public override void InvokeError()
        {
            isCompleted = true;
            try
            {
                onError?.Invoke(this);
            }
            catch (Exception ex)
            {
                Uqee.Debug.LogError(ex);
            }
            RequestPool.MarkRelease(this);
        }
    }
}