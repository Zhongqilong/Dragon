using UnityEngine;
using System.IO;
using System.Text;
using System;

namespace Uqee.Resource
{
    public enum StreamingReadMode
    {
        /// <summary>
        /// 自动模式 。先读取热更目录，如果热更目录没有，读取StreaingAssets目录
        /// </summary>
        AUTO = 0,
        /// <summary>
        /// 内部模式 。读取打在包里的文件（StreamingAssets/>
        /// </summary>
        INTERNAL = 1,
        /// <summary>
        /// 热更模式 。读取热更目录的文件 
        /// </summary>
        HOTFIX=2
    }
    /// <summary>
    /// <para>如果热更目录中有文件，则读取热更目录中的文件</para>
    /// <para>没有热更文件，则读取 StreamingAssets/目录下的文件</para>
    /// </summary>
    public class StreamingResourceProcessor : AbstractResourceProcessor<StreamingResourceProcessor>
    {
        public IStreamingAdapter loadAdapter;

        public override void Dispose()
        {
            base.Dispose();
            loadAdapter = null;
        }
        /// <summary>
        /// 保存文件到热更目录
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        public void SaveStreamingText(string fileName, string text)
        {
            fileName = Path.Combine(DirectorySetting.persistentDir, fileName);
            Uqee.Debug.Log($"[Write Text] {fileName}");
            File.WriteAllText(fileName, text, Encoding.UTF8);
        }
        /// <summary>
        /// 保存文件到热更目录
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        public void SaveStreamingBytes(string fileName, byte[] buff, bool replace=true)
        {
            fileName = Path.Combine(DirectorySetting.persistentDir, fileName);
            if(!replace)
            {
                if(File.Exists(fileName))
                {
                    return;
                }
            }
            Uqee.Debug.Log($"[Write Buffer] {fileName}");
            File.WriteAllBytes(fileName, buff);
        }
        /// <summary>
        /// 读取热更目录下的文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] ReadStreamingBytes(string fileName)
        {
            fileName = Path.Combine(DirectorySetting.persistentDir, fileName);
            if (File.Exists(fileName))
                return File.ReadAllBytes(fileName);
            else
                return null;
        }

        public T LoadStreamingJson<T>(string fileName)
        {
            var text = LoadStreamingText(fileName);
            if (text != null)
            {
                try
                {
                    return JsonUtility.FromJson<T>(text);
                }
                catch (Exception ex)
                {
                    Uqee.Debug.LogError($"{fileName} 解析失败。{ex.Message}");
                }
            }
            return default;
        }

        /// <summary>
        /// 根据文件名和读取模式，获取实际加载路径。
        /// 
        /// </summary>
        /// <param name="fileName">文件名(相对路径)</param>
        /// <param name="mode">读取模式</param>
        /// <param name="path">实际加载路径</param>
        /// <returns>返回是否需要使用适配器读取</returns>
        private bool _GetStreamingPath(string fileName, StreamingReadMode mode, out string path)
        {
            path = null;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                path = Path.Combine(Application.streamingAssetsPath, fileName);
                return false;
            }
#endif

            if (mode != StreamingReadMode.INTERNAL && CDNSetting.useCDN)
            {
                //读热更目录下面的
                path = Path.Combine(DirectorySetting.persistentDir, fileName);
            }
            if (path == null || !File.Exists(path))
            {
                if (mode == StreamingReadMode.HOTFIX)
                {
                    path = null;
                    return false;
                }
                //读内置目录下面的
                if (loadAdapter != null)
                {
                    //使用适配器读取的，不传路径，由适配器决定路径
                    path = fileName;
                    return true;
                }
                else
                {
                    path = Path.Combine(DirectorySetting.streamingDir, fileName);
                }
            }
            return false;
        }
        /// <summary>
        /// <para>如果热更目录中有文件，则读取热更目录中的文件</para>
        /// <para>没有热更文件，则读取 StreamingAssets/目录下的文件</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mode">文件读取模式</param>
        /// <returns></returns>
        public string LoadStreamingText(string fileName, StreamingReadMode mode = StreamingReadMode.AUTO)
        {
            string path = null;
            var ret = _GetStreamingPath(fileName, mode, out path);
            if (path == null)
            {
                return null;
            }
            string text = null;
            Uqee.Debug.Log($"[LoadStreamingText]{path}", Color.yellow);
            UnityEngine.Profiling.Profiler.BeginSample(fileName);
            if (ret)
            {
                text = loadAdapter.GetStreamingText(path);
            }
            else
            {
                text = File.ReadAllText(path, Encoding.UTF8);
            }
            UnityEngine.Profiling.Profiler.EndSample();

            return text;
        }
        /// <summary>
        /// <para>如果热更目录中有文件，则读取热更目录中的文件</para>
        /// <para>没有热更文件，则读取 StreamingAssets/目录下的文件</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mode">文件读取模式</param>
        /// <returns></returns>
        public byte[] LoadStreamingBytes(string fileName, StreamingReadMode mode = StreamingReadMode.AUTO)
        {
            string path = null;
            var ret = _GetStreamingPath(fileName, mode, out path);
            if (path == null)
            {
                return null;
            }
            byte[] bytes = null;
            Uqee.Debug.Log($"[LoadStreamingBytes]{path}", Color.yellow);
            UnityEngine.Profiling.Profiler.BeginSample(fileName);
            if (ret)
            {
                bytes = loadAdapter.GetStreamingBytes(path);
            }
            else
            {
                bytes = File.ReadAllBytes(path);
            }
            UnityEngine.Profiling.Profiler.EndSample();

            return bytes;
        }
    }
}