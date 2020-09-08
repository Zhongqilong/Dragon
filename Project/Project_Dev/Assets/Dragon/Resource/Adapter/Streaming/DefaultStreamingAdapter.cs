using UnityEngine;
using UnityEditor;
using System.IO;

namespace Uqee.Resource
{
    public class DefaultStreamingAdapter : IStreamingAdapter
    {
        public byte[] GetStreamingBytes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            var realPath = Path.Combine(DirectorySetting.streamingDir, path);
            if (!File.Exists(realPath))
            {
                Uqee.Debug.LogWarning($"[LoadStreamingText]file not exist:{path}", Color.yellow);
                return null;
            }
            return File.ReadAllBytes(realPath);
        }

        public string GetStreamingText(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            var realPath = Path.Combine(DirectorySetting.streamingDir, path);
            if (!File.Exists(realPath))
            {
                Uqee.Debug.LogWarning($"[LoadStreamingText]file not exist:{path}", Color.yellow);
                return null;
            }
            return File.ReadAllText(realPath);
        }
    }
}