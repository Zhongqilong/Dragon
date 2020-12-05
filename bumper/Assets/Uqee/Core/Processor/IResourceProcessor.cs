
namespace Uqee.Resource
{
    public interface IProcessor
    {
        /// <summary>
        /// 初始化方法
        /// </summary>
        void Init();
        /// <summary>
        /// Manager Update时调用
        /// </summary>
        void Update();
        /// <summary>
        /// Manager Dispose时调用
        /// </summary>
        void Dispose();
        /// <summary>
        /// 设置停止处理
        /// </summary>
        /// <param name="val"></param>
        void SetStop(bool val);
        bool IsStop();
        /// <summary>
        /// 是否空闲状态（没有请求需要处理）
        /// </summary>
        /// <returns></returns>
        bool IsFree();
        /// <summary>
        /// 是否有请求正在处理中（切换场景时，会等待所有请求处理完毕再切换）
        /// </summary>
        /// <returns></returns>
        bool IsWorking();
    }
    public abstract class AbstractProcessor<T> : Singleton<T>, IProcessor where T : class, IProcessor, new()
    {
        public bool isInited { get; private set; }
        public virtual new void Init()
        {
            if (isInited) return;
            isInited = true;
        }
        //public virtual void Dispose()
        //{

        //}
        public virtual void Update()
        {

        }
        public virtual bool IsFree()
        {
            return true;
        }
        public virtual bool IsWorking()
        {
            return false;
        }
        private bool _isStop;
        public bool IsStop()
        {
            return _isStop;
        }
        public virtual void SetStop(bool val)
        {
            _isStop = val;
        }
    }
    public interface IResourceProcessor: IProcessor
    {
        /// <summary>
        /// 清空请求（Manager停止处理时会调用）
        /// </summary>
        void ClearRequest();
        ///// <summary>
        ///// 释放资源
        ///// </summary>
        ///// <param name="all">是否全部释放（切换场景时全部释放）</param>
        //void ReleaseAssets(bool all);
        /// <summary>
        /// 快速模式切换。
        /// </summary>
        /// <param name="val"></param>
        void SetFastMode(bool val);
    }
    public abstract class AbstractResourceProcessor<T> : AbstractProcessor<T>, IResourceProcessor where T : class, IResourceProcessor, new()
    {
        public virtual void ClearRequest()
        {

        }
        //public virtual void ReleaseAssets(bool all)
        //{

        //}
        public virtual void SetFastMode(bool val)
        {

        }
        public override void SetStop(bool val)
        {
            if(val)
            {
                ClearRequest();
            }
            base.SetStop(val);
        }
        //public override void Dispose()
        //{
        //    ReleaseAssets(true);
        //    base.Dispose();
        //}
    }
}