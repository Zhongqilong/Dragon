using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Uqee.Utility;
using Uqee.Pool;

public struct TarInfo
{
    public string md5;
    public long size;
}

public class GZipUtils
{
    const int BUFF_SIZE = 500 * 1024;
    public static TarInfo Tar(string srcFolder, string destFolder, List<string> srcFileList, string destFileName)
    {
        if (string.IsNullOrEmpty(srcFolder)
            || string.IsNullOrEmpty(destFileName)
            || !System.IO.Directory.Exists(srcFolder)
            || srcFileList.Count == 0)
        {
            return new TarInfo();
        }

        string strOupFileAllPath = Path.Combine(destFolder, destFileName + ".tar");
        Uqee.Debug.Log(strOupFileAllPath);
        FileStream outStream = new FileStream(strOupFileAllPath, FileMode.OpenOrCreate);

        TarArchive archive = TarArchive.CreateOutputTarArchive(outStream, TarBuffer.DefaultBlockFactor);
        for (int i = 0; i < srcFileList.Count; i++)
        {
            string strSourceFolderAllPath = Path.Combine(srcFolder, srcFileList[i]);
            if (!System.IO.Directory.Exists(strSourceFolderAllPath))
            {
                continue;
            }
            GZip.baseDirectory = strSourceFolderAllPath;

            Uqee.Debug.Log(strSourceFolderAllPath);
            TarEntry entry = TarEntry.CreateEntryFromFile(strSourceFolderAllPath);
            archive.WriteEntry(entry, true);
        }
        archive.Close();

        outStream.Close();
        return new TarInfo()
        {
            md5 = CryptUtils.MD5File(strOupFileAllPath),
            size = new FileInfo(strOupFileAllPath).Length
        };
    }

    //private const int BUFF_SIZE = 2 * 1024 * 1024;

    /// <summary>
    /// 生成 ***.tar 文件
    /// </summary>
    /// <param name="srcFolder">文件基目录（源文件、生成文件所在目录）</param>
    /// <param name="destFileName">待压缩的源文件夹名</param>
    public static TarInfo Tar(string srcFolder, string destFolder, string destFileName)
    {
        if (string.IsNullOrEmpty(srcFolder)
            || string.IsNullOrEmpty(destFileName)
            || !System.IO.Directory.Exists(srcFolder)
            || !System.IO.Directory.Exists(Path.Combine(srcFolder, destFileName)))
        {
            return new TarInfo();
        }

        string strSourceFolderAllPath = Path.Combine(srcFolder, destFileName);
        GZip.baseDirectory = strSourceFolderAllPath;

        string strOupFileAllPath = Path.Combine(destFolder, destFileName + ".tar");
        Uqee.Debug.Log(strOupFileAllPath);
        FileStream outStream = new FileStream(strOupFileAllPath, FileMode.OpenOrCreate);

        TarArchive archive = TarArchive.CreateOutputTarArchive(outStream, TarBuffer.DefaultBlockFactor);
        TarEntry entry = TarEntry.CreateEntryFromFile(strSourceFolderAllPath);
        archive.WriteEntry(entry, true);

        if (archive != null)
        {
            archive.Close();
        }

        outStream.Close();
        return new TarInfo()
        {
            md5 = CryptUtils.MD5File(strOupFileAllPath),
            size = new FileInfo(strOupFileAllPath).Length
        };
    }

    public static IEnumerator UnTar(byte[] bytes, string unpackDir)
    {
        yield return _UnTarStream(unpackDir, new MemoryStream(bytes));
    }
    public static string error;
    /// <summary>
    /// tar包解压
    /// </summary>
    /// <param name="tarPath">tar包路径</param>
    /// <param name="unpackDir">解压到的目录</param>
    /// <returns></returns>
    public static IEnumerator UnTar(string tarPath, string unpackDir)
    {
        if (!File.Exists(tarPath))
        {
            error = string.Format("tar not exist:{0}", tarPath);
            Uqee.Debug.LogError(error);
        }
        else
        {
            unpackDir = unpackDir.ReplaceChar(Path.DirectorySeparatorChar, '/');
            if (!unpackDir.EndsWith("/"))
            {
                unpackDir += "/";
            }

            Uqee.Debug.Log(string.Format("untar:{0}", tarPath));
            FileStream fr = new FileStream(tarPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            yield return _UnTarStream(unpackDir, fr);
//#if !UNITY_EDITOR && UNITY_ANDROID
//            SDKUtils.UnTar(tarPath, unpackDir);
//            yield return null;
//#else
//            FileStream fr = new FileStream(tarPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
//            yield return _UnTarStream(unpackDir, fr);
//#endif
        }
    }

    private static IEnumerator _UnTarStream(string unpackDir, Stream inStream)
    {
        if (!Directory.Exists(unpackDir))
        {
            Directory.CreateDirectory(unpackDir);
        }
        TarInputStream tarIn = new TarInputStream(inStream);
        TarEntry theEntry;
        int count = 0;

        while ((theEntry = tarIn.GetNextEntry()) != null)
        {
            string directoryName = Path.GetDirectoryName(theEntry.Name);
            string fileName = Path.GetFileName(theEntry.Name);

            if (directoryName != String.Empty && directoryName != "/")
            {
                Directory.CreateDirectory(unpackDir + directoryName);
            }
            if (fileName != string.Empty)
            {
                var destFile = unpackDir + theEntry.Name;
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
                //fileName = theEntry.Name.Replace(theEntry.Name.Split('.').Last(), "");
                int fileSize = 0;
                //var files = Directory.GetFiles(unpackDir, fileName + "*");
                //foreach (var f in files)
                //{
                //    File.Delete(f);
                //}
                try
                {
                    FileStream streamWriter = File.Create(destFile);

                    int size = 0;
                    byte[] data = StreamBufferPool.GetBuffer(BUFF_SIZE);
                    while (true)
                    {
                        size = tarIn.Read(data, 0, BUFF_SIZE);
                        if (size > 0)
                        {
                            fileSize += size;
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    StreamBufferPool.RecycleBuffer(data);
                    streamWriter.Close();
                }
                catch (Exception ex)
                {
                    error = string.Format("untar fail:{0}", ex.Message);
                    Uqee.Debug.LogError(ex);
                    inStream.Close();
                    yield break;
                }
                count++;
                //Log.Info("{0} {1}kb", "#FFFFFF", fileName, fileSize/1024f );
                if (count % 10 == 0)
                {
                    yield return null;
                }
            }
        }
        tarIn.Close();

        Uqee.Debug.Log(string.Format("untar {0} files use.", count));
    }

    /// <summary>
    /// 生成 ***.gz 文件
    /// </summary>
    /// <param name="path">待压缩的源文件</param>
    public static bool Gzip(string path)
    {
        if (string.IsNullOrEmpty(path)
            || !System.IO.File.Exists(path))
        {
            return false;
        }

        string strOupFileAllPath = path + ".gz";
        Uqee.Debug.Log(strOupFileAllPath);
        Stream outTmpStream = new FileStream(strOupFileAllPath, FileMode.OpenOrCreate);

        //注意此处源文件大小大于4096KB
        GZipOutputStream outStream = new GZipOutputStream(outTmpStream);
        byte[] binary = File.ReadAllBytes(path);
        outStream.Write(binary, 0, binary.Length);

        outStream.Flush();
        outStream.Close();

        return true;
    }

    /// <summary>
    /// 解压 ***.gz 文件
    /// </summary>
    /// <param name="gzPath">解压缩的源文件</param>
    public static byte[] UnGzip(string gzPath)
    {
        if (string.IsNullOrEmpty(gzPath)
            || !System.IO.File.Exists(gzPath))
        {
            return null;
        }
        byte[] ret = null;
        FileStream fr = new FileStream(gzPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        try
        {
            GZipInputStream gzi = new GZipInputStream(fr);

            MemoryStream msOut = DataFactory<MemoryStream>.Get();
            int count = 0;
            byte[] data = StreamBufferPool.GetBuffer(BUFF_SIZE);
            while ((count = gzi.Read(data, 0, BUFF_SIZE)) != 0)
            {
                msOut.Write(data, 0, count);
            }
            StreamBufferPool.RecycleBuffer(data);
            msOut.Flush();
            ret = msOut.ToArray();

            gzi.Close();
            DataFactory<MemoryStream>.Release(msOut);
        }
        catch (Exception ex)
        {
            fr.Close();
            ret = new byte[0];
            Uqee.Debug.LogError(ex);
        }

        return ret;
    }

    /// <summary>
    /// 解压 ***.gz 文件
    /// </summary>
    /// <param name="path">解压缩的源文件</param>
    public static byte[] UnGzip(byte[] bytes)
    {
        MemoryStream msIn = new MemoryStream(bytes);
        GZipInputStream gzi = new GZipInputStream(msIn);

        MemoryStream msOut = DataFactory<MemoryStream>.Get();
        int count = 0;
        byte[] data = StreamBufferPool.GetBuffer(BUFF_SIZE);
        while ((count = gzi.Read(data, 0, BUFF_SIZE)) != 0)
        {
            msOut.Write(data, 0, count);
        }
        StreamBufferPool.RecycleBuffer(data);
        msOut.Flush();
        byte[] ret = msOut.ToArray();

        gzi.Close();
        DataFactory<MemoryStream>.Release(msOut);

        return ret;
    }

    /// <summary>
    /// 生成 ***.tar.gz 文件
    /// </summary>
    /// <param name="strBasePath">文件基目录（源文件、生成文件所在目录）</param>
    /// <param name="strSourceFolderName">待压缩的源文件夹名</param>
    public static bool TarGz(string strBasePath, string strSourceFolderName)
    {
        if (string.IsNullOrEmpty(strBasePath)
            || string.IsNullOrEmpty(strSourceFolderName)
            || !System.IO.Directory.Exists(strBasePath)
            || !System.IO.Directory.Exists(Path.Combine(strBasePath, strSourceFolderName)))
        {
            return false;
        }

        string strSourceFolderAllPath = Path.Combine(strBasePath, strSourceFolderName);
        GZip.baseDirectory = strBasePath;

        string strOupFileAllPath = Path.Combine(strBasePath, strSourceFolderName + ".tar.gz");
        Uqee.Debug.Log(strOupFileAllPath);
        Stream outTmpStream = new FileStream(strOupFileAllPath, FileMode.OpenOrCreate);

        //注意此处源文件大小大于4096KB
        GZipOutputStream outStream = new GZipOutputStream(outTmpStream);
        TarArchive archive = TarArchive.CreateOutputTarArchive(outStream, TarBuffer.DefaultBlockFactor);
        TarEntry entry = TarEntry.CreateEntryFromFile(strSourceFolderAllPath);

        archive.WriteEntry(entry, true);

        if (archive != null)
        {
            archive.Close();
        }
        outStream.Close();

        return true;
    }

    /// <summary>
    /// tar包解压
    /// </summary>
    /// <param name="targzPath">tar包路径</param>
    /// <param name="unpackDir">解压到的目录</param>
    /// <returns></returns>
    public static bool UnpackTarGz(string targzPath, string unpackDir)
    {
        try
        {
            if (!File.Exists(targzPath))
            {
                return false;
            }

            unpackDir = unpackDir.ReplaceChar(Path.DirectorySeparatorChar, '/');
            if (!unpackDir.EndsWith("/"))
            {
                unpackDir += "/";
            }

            if (!Directory.Exists(unpackDir))
            {
                Directory.CreateDirectory(unpackDir);
            }

            FileStream fr = new FileStream(targzPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            TarInputStream s = new TarInputStream(fr);
            TarEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);

                if (directoryName != String.Empty)
                    Directory.CreateDirectory(unpackDir + directoryName);

                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(unpackDir + theEntry.Name);

                    int size = 0;
                    byte[] data = StreamBufferPool.GetBuffer(BUFF_SIZE);
                    while (true)
                    {
                        size = s.Read(data, 0, BUFF_SIZE);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                    StreamBufferPool.RecycleBuffer(data);
                }
            }
            s.Close();

            return true;
        }
        catch (Exception ex)
        {
            Uqee.Debug.LogError(ex);
            return false;
        }
    }
}