
namespace Uqee.Resource
{
    public interface IResourceRequest
    {
        void InvokeError();
        void InvokeComplete();
        void Release();
    }
}