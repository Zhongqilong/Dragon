
namespace Uqee.Resource
{
    public interface IStreamingAdapter
    {
        string GetStreamingText(string path);
        byte[] GetStreamingBytes(string path);
    }
}