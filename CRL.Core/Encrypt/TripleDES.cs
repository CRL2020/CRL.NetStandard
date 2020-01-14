using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace CRL.Core.Encrypt
{
    /// <summary>
    /// 3DES双倍长
    /// </summary>
    public class TripleDES
    {
        /// <summary>
        /// 3des加密
        /// </summary>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (key.Length != 16)
            {
                throw new Exception("key长度应为16字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            MemoryStream mStream = new MemoryStream();

            TripleDESCryptoServiceProvider tdsp = new TripleDESCryptoServiceProvider();
            tdsp.Mode = CipherMode.ECB;
            tdsp.Padding = PaddingMode.Zeros;
            // Create a CryptoStream using the MemoryStream 
            // and the passed key and initialization vector (IV).
            CryptoStream cStream = new CryptoStream(mStream,
                tdsp.CreateEncryptor(key, iv),
                CryptoStreamMode.Write);

            // Write the byte array to the crypto stream and flush it.
            cStream.Write(data, 0, data.Length);
            cStream.FlushFinalBlock();

            // Get an array of bytes from the 
            // MemoryStream that holds the 
            // encrypted data.
            byte[] ret = mStream.ToArray();

            // Close the streams.
            cStream.Close();
            mStream.Close();

            // Return the encrypted buffer.
            return ret;
        }
        /// <summary>
        /// 3des解密
        /// </summary>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            if (key.Length != 16)
            {
                throw new Exception("key长度应为16字节");
            }
            if (iv.Length != 8)
            {
                throw new Exception("iv长度应为8字节");
            }
            MemoryStream msDecrypt = new MemoryStream(data);

            TripleDESCryptoServiceProvider tdsp = new TripleDESCryptoServiceProvider();
            tdsp.Mode = CipherMode.ECB;
            tdsp.Padding = PaddingMode.Zeros;

            // Create a CryptoStream using the MemoryStream 
            // and the passed key and initialization vector (IV).
            CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                tdsp.CreateDecryptor(key, iv),
                CryptoStreamMode.Read);

            // Create buffer to hold the decrypted data.
            byte[] fromEncrypt = new byte[data.Length];

            // Read the decrypted data out of the crypto stream
            // and place it into the temporary buffer.
            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

            //Convert the buffer into a string and return it.
            return fromEncrypt;
        }
    }
}
