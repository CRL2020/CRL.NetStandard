using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CRL.Core.Encrypt
{
   public class RSA
   { 
       /// <summary>
       /// 生成公钥、私钥
       /// </summary>
       /// <returns>公钥、私钥，公钥键"PUBLIC",私钥键"PRIVATE"</returns>
       public static Dictionary<string, string> createKeyPair()
       {
           Dictionary<string, string> keyPair = new Dictionary<string, string>();
           RSACryptoServiceProvider provider = new RSACryptoServiceProvider(1024);
           keyPair.Add("PUBLIC", provider.ToXmlString(false));
           keyPair.Add("PRIVATE", provider.ToXmlString(true));
           return keyPair;
       }
        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static byte[] RSAEncrypt(string publickey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);
            return cipherbytes;
            //return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSADecrypt(string privatekey, byte[] content)
        {
 
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(privatekey);
            cipherbytes = rsa.Decrypt(content, false);
            return Encoding.UTF8.GetString(cipherbytes);
        }
        /// <summary>
        /// 进行签名
        /// </summary>
        /// <param name="privatekey">私钥</param>
        /// <param name="rgbHash">需签名的数据</param>
        /// <returns></returns>
        public static byte[] SignatureFormatter(string privatekey, byte[] rgbHash)
        {
            RSACryptoServiceProvider key = new RSACryptoServiceProvider();
            key.FromXmlString(privatekey);
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(key);
            formatter.SetHashAlgorithm("SHA1");
            byte[] inArray = formatter.CreateSignature(rgbHash);
            return inArray;
        }

        /// <summary>
        /// 签名验证
        /// </summary>
        /// <param name="publickey">公钥</param>
        /// <param name="rgbHash">Hash描述</param>
        /// <param name="rgbSignature">签名后的结果</param>
        /// <returns></returns>
        public static bool SignatureDeformatter(string publickey, byte[] rgbHash, byte[] rgbSignature)
        {
            
                RSACryptoServiceProvider key = new RSACryptoServiceProvider();
                key.FromXmlString(publickey);
                RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(key);
                deformatter.SetHashAlgorithm("SHA1");
                if (deformatter.VerifySignature(rgbHash, rgbSignature))
                {
                    return true;
                }
                return false;
            
        }
    }
}
