using UnityEngine;
using System.Collections.Generic;

namespace Uqee.Resource
{
    public class AbstractProcessorManager<T, TProcessor> : Singleton<T> where T : class, new()
                                                                        where TProcessor : IProcessor
    {
        protected List<TProcessor> _processorList = new List<TProcessor>();
        public void AddProcessor(TProcessor processor)
        {
            if (_processorList.Contains(processor))
            {
                return;
            }
            _processorList.Add(processor);

            processor.Init();
        }
        protected override void Init()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            UpdateManager.I.AddCallback(_Update, typeof(T).Name);
        }
        public bool isFree
        {
            get
            {
                foreach (var processor in _processorList)
                {
                    if (!processor.IsFree())
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool isLoading
        {
            get
            {
                foreach (var processor in _processorList)
                {
                    if (processor.IsWorking())
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void Dispose()
        {
            foreach (var processor in _processorList)
            {
                processor.Dispose();
            }
            _processorList.Clear();
            base.Dispose();
        }

        public bool isStop { get; private set; }
        /// <summary>
        /// 设置是否停止处理
        /// </summary>
        /// <param name="val"></param>
        public void SetStop(bool val)
        {
            if (val)
            {
                foreach (var processor in _processorList)
                {
                    processor.SetStop(true);
                }
            }
            else
            {
                foreach (var processor in _processorList)
                {
                    processor.SetStop(false);
                }
            }
            isStop = val;
        }

        protected virtual void _Update()
        {
            if (isStop)
            {
                return;
            }

            foreach (var processor in _processorList)
            {
                processor.Update();
            }
        }
    }
}