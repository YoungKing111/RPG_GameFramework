﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class AES
{
    private static string AESHead = "AESEncrypt";

    /// <summary>
    /// 文件加密，传入文件路径
    /// </summary>
    public static void AESFileEncrypt(string path, string EncryptKey)
    {
        if (!File.Exists(path))
            return;

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fs != null)
                {
                    byte[] headBuff = new byte[10];
                    fs.Read(headBuff, 0, 10);
                    string headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
#if UNITY_EDITOR
                        Debug.Log(path + "已经加密过了！");
#endif
                        return;
                    }
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    byte[] headBuffer = Encoding.UTF8.GetBytes(AESHead);
                    fs.Write(headBuffer, 0, 10);
                    byte[] EncBuffer = AESEncrypt(buffer, EncryptKey);
                    fs.Write(EncBuffer, 0, EncBuffer.Length);
                }
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// 文件解密，传入文件路径  (改动了加密文件，不合适运行时)
    /// </summary>
    public static void AESFileDecrypt(string path, string EncryptKey)
    {
        if (!File.Exists(path))
            return;

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fs != null)
                {
                    byte[] headBuff = new byte[10];
                    fs.Read(headBuff, 0, headBuff.Length);
                    string headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        byte[] buffer = new byte[fs.Length - headBuff.Length];
                        fs.Read(buffer, 0, Convert.ToInt32(fs.Length - headBuff.Length));
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(0);
                        byte[] EncBuffer = AESDecrypt(buffer, EncryptKey);
                        fs.Write(EncBuffer, 0, EncBuffer.Length);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// 文件解密，传入文件路径,返回字节
    /// </summary>
    public static byte[] AESFileByteDecrypt(string path, string EncryptKey)
    {
        if (!File.Exists(path))
            return null;

        byte[] EncBuffer = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs != null)
                {
                    byte[] headBuff = new byte[10];
                    fs.Read(headBuff, 0, headBuff.Length);
                    string headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        byte[] buffer = new byte[fs.Length - headBuff.Length];
                        fs.Read(buffer, 0, Convert.ToInt32(fs.Length - headBuff.Length));
                        EncBuffer = AESDecrypt(buffer, EncryptKey);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
        }

        return EncBuffer;
    }

    /// <summary>
    /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="EncryptString">待加密密文</param>
    /// <param name="EncryptKey">加密密钥</param>
    public static string AESEncrypt(string EncryptString, string EncryptKey)
    {
        return Convert.ToBase64String(AESEncrypt(Encoding.Default.GetBytes(EncryptString), EncryptKey));
    }

    /// <summary>
    /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="EncryptString">待加密密文</param>
    /// <param name="EncryptKey">加密密钥</param>
    public static byte[] AESEncrypt(byte[] EncryptByte, string EncryptKey)
    {
        //if (EncryptByte.Length == 0) { throw (new Exception("明文不得为空")); }
        if (EncryptByte.Length == 0) { return EncryptByte; }
        if (string.IsNullOrEmpty(EncryptKey)) { throw (new Exception("密钥不得为空")); }
        byte[] m_strEncrypt;
        byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
        byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strEncrypt = m_stream.ToArray();
            m_stream.Close(); m_stream.Dispose();
            m_csstream.Close(); m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
        return m_strEncrypt;
    }


    /// <summary>
    /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="DecryptString">待解密密文</param>
    /// <param name="DecryptKey">解密密钥</param>
    public static string AESDecrypt(string DecryptString, string DecryptKey)
    {
        return Convert.ToBase64String(AESDecrypt(Encoding.Default.GetBytes(DecryptString), DecryptKey));
    }

    /// <summary>
    /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="DecryptString">待解密密文</param>
    /// <param name="DecryptKey">解密密钥</param>
    public static byte[] AESDecrypt(byte[] DecryptByte, string DecryptKey)
    {
        //if (DecryptByte.Length == 0) { throw (new Exception("密文不得为空")); }
        if (DecryptByte.Length == 0) { return DecryptByte; }
        if (string.IsNullOrEmpty(DecryptKey)) { throw (new Exception("密钥不得为空")); }
        byte[] m_strDecrypt;
        byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
        byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(DecryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strDecrypt = m_stream.ToArray();
            m_stream.Close(); m_stream.Dispose();
            m_csstream.Close(); m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
        return m_strDecrypt;
    }
}

