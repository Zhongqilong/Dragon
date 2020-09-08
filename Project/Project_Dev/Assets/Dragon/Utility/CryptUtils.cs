using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Uqee.Utility
{
    public static class CryptUtils
    {
        public static string MD5File(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            return MD5Bytes(bytes);
        }

        public static string MD5Str(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            return MD5Bytes(bytes);
        }

        public static string MD5Bytes(byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(bytes);
            StringBuilder strbul = new StringBuilder(32);
            for (int i = 0; i < result.Length; i++)
            {
                strbul.Append(result[i].ToString("x2"));//加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
            }
            return strbul.ToString();

            //return Encoding.Default.GetString(result);
        }

        public static byte[] AESEncrypt(byte[] bytes, string key, int byteLen)
        {
            using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
            {
                //Log.d(key + "," + bytes.Length);
                //aesProvider.KeySize = 256;
                //aesProvider.BlockSize = 512;
                aesProvider.IV = new byte[16];
                //aesProvider.IV = GetIV(bytes.Length);
                aesProvider.Key = Encoding.UTF8.GetBytes(key);
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor())
                {
                    byte[] results = cryptoTransform.TransformFinalBlock(bytes, 0, byteLen==-1?bytes.Length: byteLen);
                    aesProvider.Clear();
                    aesProvider.Dispose();
                    return results;
                }
            }
        }

        public static byte[] AESDecrypt(byte[] bytes, string key, int byteLen)
        {
            using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
            {
                aesProvider.Key = Encoding.UTF8.GetBytes(key);
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.IV = new byte[16];
                aesProvider.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor())
                {
                    byte[] results = cryptoTransform.TransformFinalBlock(bytes, 0, byteLen==-1?bytes.Length:byteLen);
                    aesProvider.Clear();
                    return results;
                }
            }
        }

        private static Dictionary<int, byte[]> _ivDict = new Dictionary<int, byte[]>();
        private static string _fillStr = "";

        private static byte[] GetIV(int len)
        {
            if (_fillStr == "")
            {
                for (int i = 0; i < 128; i++)
                {
                    _fillStr += "0";
                }
            }
            if (!_ivDict.ContainsKey(len))
            {
                uint iv = (uint)(len >> 4) + 1;
                string hex = Convert.ToString(iv, 16);
                string ivStr = "";

                for (int k = 0; k < hex.Length; k++)
                {
                    ivStr += "0" + hex;
                }
                ivStr = _fillStr.Substring(0, 128 - ivStr.Length) + ivStr;

                _ivDict[len] = HexStringToBytes(ivStr);
            }
            return _ivDict[len];
        }

        public static byte[] HexStringToBytes(string hex)
        {
            if ((hex.Length & 1) == 1) hex = "0" + hex;

            int block = 2;
            int len = hex.Length / block;
            byte[] result = new byte[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = Byte.Parse(hex.Substring(i * block, block), System.Globalization.NumberStyles.HexNumber);
            }

            return result;
        }
    }
}