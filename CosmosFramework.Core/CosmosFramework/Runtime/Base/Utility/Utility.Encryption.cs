﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.IO;
namespace Cosmos
{
    public static partial class Utility
    {
        /// <summary>
        /// 加密工具
        /// </summary>
        public static class Encryption
        {
            static StringBuilder stringBuilderCache = new StringBuilder(1024);
            public enum GUIDFormat
            {
                N, D, B, P, X
            }
            public static string GUID(GUIDFormat format)
            {
                return Guid.NewGuid().ToString(format.ToString());
            }
            /// <summary>
            /// MD5加密，返回16位加密后的大写16进制字符
            /// </summary>
            /// <param name="strData">需要加密的字符</param>
            /// <returns>加密后的结果</returns>
            public static string MD5Encrypt16(string strData)
            {
                byte[] md5Bytes = Encoding.UTF8.GetBytes(strData);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] cryptString = md5.ComputeHash(md5Bytes);
                stringBuilderCache.Clear();
                for (int i = 4; i < 12; i++)
                {
                    stringBuilderCache.Append(cryptString[i].ToString("X2"));
                }
                return stringBuilderCache.ToString();
            }
            /// <summary>
            /// MD5加密，返回32位加密后的大写16进制字符
            /// </summary>
            /// <param name="strData">需要加密的字符</param>
            /// <returns>加密后的结果</returns>
            public static string MD5Encrypt32(string strData)
            {
                byte[] md5Bytes = Encoding.UTF8.GetBytes(strData);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] cryptString = md5.ComputeHash(md5Bytes);
                stringBuilderCache.Clear();
                int length = cryptString.Length;
                for (int i = 0; i < length; i++)
                {
                    //X大写的16进制，x小写
                    stringBuilderCache.Append(cryptString[i].ToString("X2"));
                }
                return stringBuilderCache.ToString();
            }
            /// <summary>
            /// Base64加密，返回24位加密后的字符
            /// </summary>
            /// <param name="strData">需要加密的字符</param>
            /// <returns>加密后的结果</returns>
            public static string Base64Encrypt(string strData)
            {
                byte[] md5Bytes = Encoding.UTF8.GetBytes(strData);
                MD5 md5 = MD5.Create();
                byte[] cryptString = md5.ComputeHash(md5Bytes);
                return Convert.ToBase64String(cryptString);
            }
            /// <summary>
            /// 生成 8 位密钥
            /// Data Encryption Standard
            /// initialization vector
            /// </summary>
            /// <param name="key">需要生成的Key</param>
            /// <returns></returns>
            public static byte[] GenerateIV(string key)
            {
                var result = Encoding.UTF8.GetBytes(key);
                byte[] iv;
                if (result.Length > 8)
                {
                    iv = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        iv[i] = result[i];
                    }
                    return iv;
                }
                else if (result.Length < 8)
                {
                    iv = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        if (result.Length > i)
                            iv[i] = result[i];
                        else
                            iv[i] = 0;
                        return iv;
                    }
                }
                return result;
            }
            /// <summary>
            /// 加密算法HmacSHA256  
            /// </summary>
            /// <param name="encrpytedStr">被加密的数据</param>
            /// <param name="strKey">加密密码</param>
            /// <returns>加密后的字段</returns>
            public static string HmacSHA256(string encrpytedStr, string strKey)
            {
                string encrpytedResult = string.Empty;
                using (HMACSHA256 mac = new HMACSHA256(Encoding.UTF8.GetBytes(strKey)))
                {
                    byte[] hashMsg = mac.ComputeHash(Encoding.UTF8.GetBytes(encrpytedStr));
                    encrpytedResult = Convert.ToBase64String(hashMsg);
                }
                return encrpytedResult;
            }
            /// <summary>
            /// AES对称加密byte类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要解密的数组</param>
            /// <param name="key">对称密码,</param>
            /// <returns>加密后的内容</returns>
            public static string AESEncryptByteToString(byte[] context, byte[] key)
            {
                if (context == null)
                    throw new ArgumentNullException("context is invalid !");
                if (key == null)
                    throw new ArgumentNullException("key is invalid !");
                using (var aes = new AesCryptoServiceProvider() { Key = key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
                {
                    aes.GenerateIV();
                    var iv = aes.IV;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(iv, 0, iv.Length);
                        using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, iv), CryptoStreamMode.Write))
                        using (var writer = new BinaryWriter(cryptoStream))
                        {
                            writer.Write(context);
                            cryptoStream.FlushFinalBlock();
                        }
                        var buf = ms.ToArray();
                        return Convert.ToBase64String(buf, 0, buf.Length);
                    }
                }
            }
            /// <summary>
            /// AES对称解密byte类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要解密的数组</param>
            /// <param name="key">对称密码,</param>
            /// <returns>解密后的内容</returns>
            public static  string AESDecryptByteToString(byte[] context, byte[] key)
            {
                if (context == null)
                    throw new ArgumentNullException("context is invalid !");
                if (key == null)
                    throw new ArgumentNullException("key is invalid !");
                using (var aes = new AesCryptoServiceProvider() { Key = key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
                {
                    var iv = new byte[16];
                    Array.Copy(context, 0, iv, 0, iv.Length);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Write))
                        {
                            using (BinaryWriter writer = new BinaryWriter(cryptoStream))
                            {
                                writer.Write(context, iv.Length, context.Length - iv.Length);
                            }
                        }
                        var buf = ms.ToArray();
                        return Convert.ToBase64String(buf, 0, buf.Length);
                    }
                }
            }
            /// <summary>
            /// AES对称加密byte类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要解密的数组</param>
            /// <param name="key">对称密码</param>
            /// <returns>加密后的内容</returns>
            public static byte[] AESEncryptByteToByte(byte[] context, byte[] key)
            {
                if (context == null)
                    throw new ArgumentNullException("context is invalid !");
                if (key == null)
                    throw new ArgumentNullException("key is invalid !");
                using (var aes = new AesCryptoServiceProvider() { Key = key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
                {
                    aes.GenerateIV();
                    var iv = aes.IV;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(iv, 0, iv.Length);
                        using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, iv), CryptoStreamMode.Write))
                        using (var writer = new BinaryWriter(cryptoStream))
                        {
                            writer.Write(context);
                            cryptoStream.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
            }
            /// <summary>
            /// AES对称解密byte类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要解密的数组</param>
            /// <param name="key">对称密码,</param>
            /// <returns>解密后的内容</returns>
            public static byte[] AESDecryptByteToByte(byte[] context, byte[] key)
            {
                if (context == null)
                    throw new ArgumentNullException("context is invalid !");
                if (key == null)
                    throw new ArgumentNullException("key is invalid !");
                using (var aes = new AesCryptoServiceProvider() { Key = key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
                {
                    var iv = new byte[16];
                    Array.Copy(context, 0, iv, 0, iv.Length);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Write))
                        {
                            using (BinaryWriter writer = new BinaryWriter(cryptoStream))
                            {
                                writer.Write(context, iv.Length, context.Length - iv.Length);
                            }
                        }
                        return ms.ToArray();
                    }
                }
            }
            /// <summary>
            /// AES对称加密string类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要加密的内容</param>
            /// <param name="key">密钥</param>
            /// <returns>加密后的内容</returns>
            public static string AESEncryptStringToString(string context, byte[] key)
            {
                if (string.IsNullOrEmpty(context))
                    throw new ArgumentNullException("context is invalid ! ");
                if (key == null)
                    throw new ArgumentNullException("key is invalid ! ");
                using (var aes = new AesCryptoServiceProvider())
                {
                    var iv = aes.IV;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(iv, 0, iv.Length);
                        using (var cryptStream = new CryptoStream(ms, aes.CreateEncryptor(key, aes.IV), CryptoStreamMode.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(cryptStream))
                            {
                                writer.Write(context);
                            }
                        }
                        var buf = ms.ToArray();
                        return Convert.ToBase64String(buf, 0, buf.Length);
                    }
                }
            }
            /// <summary>
            /// AES对称解密string类型内容；
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要解密的内容</param>
            /// <param name="key">密钥</param>
            /// <returns>解密后的内容</returns>
            public static string AESDecryptStringToString(string context, byte[] key)
            {
                if (string.IsNullOrEmpty(context))
                    throw new ArgumentNullException("context is invalid ! ");
                if (key == null)
                    throw new ArgumentNullException("key is invalid ! ");
                var bytes = Convert.FromBase64String(context);
                using (var aes = new AesCryptoServiceProvider())
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        var iv = new byte[16];
                        ms.Read(iv, 0, 16);
                        using (var cryptStream = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cryptStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// AES对称加密string类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要加密的内容</param>
            /// <param name="key">密钥</param>
            /// <returns>加密后的内容</returns>
            public static byte[] AESEncryptStringToByte(string context, byte[] key)
            {
                if (string.IsNullOrEmpty(context))
                    throw new ArgumentNullException("context is invalid ! ");
                if (key == null)
                    throw new ArgumentNullException("key is invalid ! ");
                using (var aes = new AesCryptoServiceProvider())
                {
                    var iv = aes.IV;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(iv, 0, iv.Length);
                        using (var cryptStream = new CryptoStream(ms, aes.CreateEncryptor(key, aes.IV), CryptoStreamMode.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(cryptStream))
                            {
                                writer.Write(context);
                            }
                        }
                        return ms.ToArray();
                    }
                }
            }
            /// <summary>
            /// AES对称解密string类型内容;
            /// 密钥的byte长度必须是16, 24, 32；
            /// </summary>
            /// <param name="context">需要加密的内容</param>
            /// <param name="key">密钥</param>
            /// <returns>解密后的内容</returns>
            public static byte[] AESDecryptStringToByte(string context, byte[] key)
            {
                if (string.IsNullOrEmpty(context))
                    throw new ArgumentNullException("context is invalid ! ");
                if (key == null)
                    throw new ArgumentNullException("key is invalid ! ");
                var bytes = Convert.FromBase64String(context);
                using (var aes = new AesCryptoServiceProvider())
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        var iv = new byte[16];
                        ms.Read(iv, 0, 16);
                        using (var cryptStream = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cryptStream))
                            {
                                var data = reader.ReadToEnd();
                                return Convert.FromBase64String(data);
                            }
                        }
                    }
                }
            }

            public static string GetFileMD5(string filePath)
            {
                try
                {
                    FileInfo fileTmp = new FileInfo(filePath);
                    if (fileTmp.Exists)
                    {
                        FileStream fs = new FileStream(filePath, FileMode.Open);
                        int len = (int)fs.Length;
                        byte[] data = new byte[len];
                        fs.Close();

                        return GetMD5(data);
                    }
                    return "";
                }
                catch (FileNotFoundException e)
                {
                    return "";
                }
            }

            public static string GetMD5(byte[] data)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = md5.ComputeHash(data);
                string fileMD5 = "";
                foreach (byte b in result)
                {
                    fileMD5 += Convert.ToString(b, 16);
                }
                if (!string.IsNullOrEmpty(fileMD5))
                {
                    return fileMD5;
                }
                return "";
            }
        }
    }
}