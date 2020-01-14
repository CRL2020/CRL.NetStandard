using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace CRL.Core
{
    public partial class StringHelper
    {
        /// <summary>
        /// BYTE转16进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ByteToHex(byte[] data)
        {
            string str = BitConverter.ToString(data);
            str = str.Replace("-", "");
            return str;
        }
        /// <summary>
        /// 16进制转BYTE
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexToByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="instr">要加密的字体串</param>
        /// <returns></returns>
        public static string EncryptMD5(string instr, Encoding enc = null)
        {
            string result;

            if (enc == null)
            {
                enc = Encoding.Default;
            }
            byte[] toByte = enc.GetBytes(instr);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            toByte = md5.ComputeHash(toByte);
            result = BitConverter.ToString(toByte).Replace("-", "");

            return result;
        }
        
        #region 加密解密，可以设置密钥
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="sourceDataBytes"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Encrypt(Byte[] sourceDataBytes, byte[] key,byte[] iv)
        {
            return CRL.Core.Encrypt.DES.Encrypt(sourceDataBytes, key, iv);
        }
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] Decrypt(Byte[] data, byte[] key,byte[] iv)
        {
            return CRL.Core.Encrypt.DES.Decrypt(data, key, iv);
        }

        /// <summary>
        /// 加密字符串,返回16进制字符串
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string sourceData, string key)
        {
            Byte[] keys = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, key.Length));
            Byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var data = Encrypt(Encoding.UTF8.GetBytes(sourceData), keys, iv);
            //return Convert.ToBase64String(data);
            return ByteToHex(data);
        }
        /// <summary>
        /// 解密 传入16进制字符串
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string sourceData, string key)
        {
            //var data = Convert.FromBase64String(sourceData);
            var data = HexToByte(sourceData);
            Byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Byte[] keys = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, key.Length));
            var data2 = Decrypt(data, keys, iv);
            return System.Text.Encoding.UTF8.GetString(data2).Trim('\0');
        }
        #endregion

    }
}
