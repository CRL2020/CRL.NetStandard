using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CRL.Core.Encrypt
{
    public class ECB
    {
        /// <summary>
        /// ECB解密
        /// </summary>
        /// <param name="encryptedDataBytes"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encryptedDataBytes, byte[] key, Byte[] iv)
        {
            if (key.Length != 8)
            {
                throw new Exception("key长度应为8字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            //Byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            //Byte[] keys = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, key.Length));

            //Byte[] encryptedDataBytes = System.Convert.FromBase64String(sourceData);
            MemoryStream tempStream = new MemoryStream(encryptedDataBytes, 0, encryptedDataBytes.Length);
            DESCryptoServiceProvider decryptor = new DESCryptoServiceProvider();
            decryptor.Mode = CipherMode.ECB;
            decryptor.Padding = PaddingMode.None;
            CryptoStream decryptionStream = new CryptoStream(tempStream, decryptor.CreateDecryptor(key, iv), CryptoStreamMode.Read);
            //StreamReader allDataReader = new StreamReader(decryptionStream);
            byte[] data = new byte[encryptedDataBytes.Length];
            decryptionStream.Read(data, 0, data.Length);
            decryptionStream.Close();
            tempStream.Close();
            return data;

        }
        public static byte[] DecryptECBAnyKey(byte[] data, byte[] key, byte[] iv)
        {
            if (key.Length != 8)
            {
                throw new Exception("key长度应为8字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Padding = PaddingMode.None;

            Type t = Type.GetType("System.Security.Cryptography.CryptoAPITransformMode");
            object obj = t.GetField("Decrypt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(t);

            MethodInfo mi = des.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
            ICryptoTransform desCrypt = (ICryptoTransform)mi.Invoke(des, new object[] { key, CipherMode.ECB, iv, 0, obj });

            byte[] result = desCrypt.TransformFinalBlock(data, 0, data.Length);
            return result;
        }

        public static byte[] EncryptECBAnyKey(byte[] data, byte[] key, byte[] iv)
        {
            if (key.Length != 8)
            {
                throw new Exception("key长度应为8字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Padding = PaddingMode.None;

            Type t = Type.GetType("System.Security.Cryptography.CryptoAPITransformMode");
            object obj = t.GetField("Encrypt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(t);

            MethodInfo mi = des.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
            ICryptoTransform desCrypt = (ICryptoTransform)mi.Invoke(des, new object[] { key, CipherMode.ECB, iv, 0, obj });

            byte[] result = desCrypt.TransformFinalBlock(data, 0, data.Length);
            return result;
        }

        /// <summary>
        /// ECB加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (key.Length != 8)
            {
                throw new Exception("key长度应为8字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            //Byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            MemoryStream tempStream = new MemoryStream();
            //get encryptor and encryption stream
            DESCryptoServiceProvider encryptor = new DESCryptoServiceProvider();
            encryptor.Mode = CipherMode.ECB;
            encryptor.Padding = PaddingMode.None;
            CryptoStream encryptionStream = new CryptoStream(tempStream, encryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            encryptionStream.Write(data, 0, data.Length);
            encryptionStream.FlushFinalBlock();
            encryptionStream.Close();
            byte[] encryptedDataBytes = tempStream.ToArray();
            tempStream.Close();
            return encryptedDataBytes;
        }
    }
}
