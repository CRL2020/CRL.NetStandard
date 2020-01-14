using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CRL.Core.Encrypt
{
    public class DES
    {
        public static byte[] Encrypt(Byte[] data, byte[] key, byte[] iv)
        {
            //Byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            //get target memory stream
            if (key.Length != 8)
            {
                throw new Exception("key长度应为8字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            MemoryStream tempStream = new MemoryStream();
            //get encryptor and encryption stream
            DESCryptoServiceProvider encryptor = new DESCryptoServiceProvider();
            CryptoStream encryptionStream = new CryptoStream(tempStream, encryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write);
            //encryptor.Padding = PaddingMode.PKCS7;
            //encrypt data
            encryptionStream.Write(data, 0, data.Length);
            encryptionStream.FlushFinalBlock();

            //put data into byte array
            Byte[] encryptedDataBytes = tempStream.ToArray();
            tempStream.Close();
            encryptionStream.Close();
            //convert encrypted data into string
            return encryptedDataBytes;
        }
        public static byte[] Decrypt(Byte[] data, byte[] key, byte[] iv)
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
            decryptor.Padding = PaddingMode.PKCS7;
            CryptoStream decryptionStream = new CryptoStream(tempStream, decryptor.CreateDecryptor(key, iv), CryptoStreamMode.Read);
            byte[] result = new byte[data.Length];
            decryptionStream.Read(result, 0, result.Length);
            decryptionStream.Close();
            tempStream.Close();
            return result;
        }
    }
}
