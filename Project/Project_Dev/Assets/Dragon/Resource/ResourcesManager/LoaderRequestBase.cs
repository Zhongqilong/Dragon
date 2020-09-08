using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Uqee.Resource
{
    public abstract class LoadRequestBase
    {
        /// <summary>
        /// 所有资源是否都加载完毕
        /// </summary>
        public virtual bool isDone
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 所有资源合计进度
        /// </summary>
        public virtual float progress
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// 所有资源优先度
        /// </summary>
        public virtual int priority
        {
            get; set;
        }

        /// <summary>
        /// 所有资源是否立即加载到场景
        /// </summary>
        public virtual bool allowSceneActivation
        {
            get; set;
        }

        /// <summary>
        /// 资源个体是否加载完成
        /// </summary>
        public virtual bool isSelfDone
        {
            get;
        }

        /// <summary>
        /// 开始加载资源
        /// </summary>
        public virtual void StartLoad()
        {

        }
    }
}