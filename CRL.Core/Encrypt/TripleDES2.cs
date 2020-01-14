using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core.Encrypt
{
    /// <summary>
    /// 3des2倍长
    /// </summary>
    public class TripleDES2
    {        
        /// <summary>
        /// 3des二倍长加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            byte[] key1 = new byte[8];
            Array.Copy(key, 0, key1, 0, 8);
            byte[] key2 = new byte[8];
            Array.Copy(key, 8, key2, 0, 8);
            byte[] data1 = ECB.Encrypt(data, key1, iv);//加密
            data1 = ECB.Decrypt(data1, key2, iv);//解密
            data1 = ECB.Encrypt(data1, key1, iv);//加密
            return data1;
        }
        /// <summary>
        /// 3des二倍长解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {

            byte[] key1 = new byte[8];
            Array.Copy(key, 0, key1, 0, 8);
            byte[] key2 = new byte[8];
            Array.Copy(key, 8, key2, 0, 8);

            byte[] data1 = ECB.Decrypt(data, key1, iv);
            data1 = ECB.Encrypt(data1, key2, iv);
            data1 = ECB.Decrypt(data1, key1, iv);
            return data1;
        }
    }
}
