namespace Uqee.Resource
{
    public class ResourceProcessorManager : AbstractProcessorManager<ResourceProcessorManager, IResourceProcessor>
    {
        public override void Dispose()
        {
            RequestPool.Release();
            base.Dispose();
        }

        public void SetFastLoad(bool val)
        {
            foreach (var processor in _processorList)
            {
                processor.SetFastMode(val);
            }
        }

        private float _lastReleaseTime;
        const int AUTO_RELEASE_DELAY = 5;
        protected override void _Update()
        {
            if (isStop)
            {
                return;
            }

            base._Update();


            if (isLoading || !isFree)
            {
                _lastReleaseTime = AppStatus.realtimeSinceStartup + AUTO_RELEASE_DELAY;
                return;
            }
            if (AppStatus.realtimeSinceStartup < _lastReleaseTime)
            {
                return;
            }

            _lastReleaseTime = AppStatus.realtimeSinceStartup + AUTO_RELEASE_DELAY;
        }
    }
}