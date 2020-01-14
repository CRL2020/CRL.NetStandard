using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CRL.Core.Encrypt
{
    public class CBC
    {
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] iv = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return Encrypt(data, key, iv);
        }
        /// <summary>
        /// cbc加密
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
            //byte[] iv = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            DESCryptoServiceProvider MyServiceProvider = new DESCryptoServiceProvider();
            MyServiceProvider.Mode = CipherMode.CBC;
            MyServiceProvider.Padding = PaddingMode.None;
            ICryptoTransform MyTransform = MyServiceProvider.CreateEncryptor(key, iv);
            MemoryStream ms = new MemoryStream();
            CryptoStream MyCryptoStream = new CryptoStream(ms, MyTransform, CryptoStreamMode.Write);
            MyCryptoStream.Write(data, 0, data.Length);
            MyCryptoStream.FlushFinalBlock();
            MyCryptoStream.Close();
            byte[] bTmp = ms.ToArray();
            ms.Close();
            return bTmp;
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (key.Length != 8)
            {
                throw new Exception("key长度应为8字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            MemoryStream tempStream = new MemoryStream(data, 0, data.Length);
            DESCryptoServiceProvider decryptor = new DESCryptoServiceProvider();
            decryptor.Mode = CipherMode.CBC;
            decryptor.Padding = PaddingMode.None;
            CryptoStream decryptionStream = new CryptoStream(tempStream, decryptor.CreateDecryptor(key, iv), CryptoStreamMode.Read);
            byte[] result = new byte[data.Length];
            decryptionStream.Read(data, 0, data.Length);
            decryptionStream.Close();
            tempStream.Close();
            return result;
        }
    }
}
