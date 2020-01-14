using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace CRL.Core.Encrypt
{
    /// <summary>
    /// 计算MAC
    /// </summary>
    public class MAC
    {
        
        
        /// <summary>
        /// ansi x9.9 MAC计算标准算法?
        /// </summary>
        /// <param name="date"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        //public static string Get_ANSI_X99_MAC(byte[] date, byte[] key, byte[] iv)
        //{
        //    byte[] Data = Add8Bit(date);
        //    byte[] b_BufArr1 = new byte[8];
        //    byte[] b_BufArr2 = new byte[8];
        //    Array.Copy(iv, b_BufArr1, 8);
        //    int iGroup = 0;
        //    if (Data.Length % 8 == 0)
        //        iGroup = Data.Length / 8;
        //    else
        //        iGroup = Data.Length / 8 + 1;
        //    for (int i = 0; i < iGroup; i++)
        //    {
        //        Array.Copy(Data, 8 * i, b_BufArr2, 0, 8);
        //        b_BufArr1 = XOR(b_BufArr1, b_BufArr2);
        //        b_BufArr2 = EncryptCBC(b_BufArr1,key,iv);
        //        Array.Copy(b_BufArr2, b_BufArr1, 8);
        //    }
        //    return ByteToHex(b_BufArr2).Substring(0, 8);
        //}
        /// <summary>
        /// ansi x9.9 MAC计算
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Get_ANSI_X99_MAC(byte[] data, byte[] key, byte[] iv)
        {
            byte[] data2 = Add8Bit(data);
            byte[] data3 = Encrypt.CBC.Encrypt(data2, key, iv);
            string s = MAC.ByteToHex(data3);
            string str = s.Substring(s.Length - 16);
            //str = str.Substring(0, 8);
            return str;
        }


        /// <summary>
        /// ECB_DES_MAC计算
        /// 银联8583协议
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Get_ECB_DES_MAC(byte[] data, byte[] key, byte[] iv)
        {
            #region 算法
            /**
             *POS终端采用ＥＣＢ的加密方式，简述如下： 
            1、将欲发送给POS中心的消息中，从消息类型（MTI）到63域之间的部分构成MAC ELEMEMENT BLOCK 
            （MAB）。 
            2、对MAB，按每8个字节做异或（不管信息中的字符格式），如果最后不满8个字节，则添加“0X00”。 
            示例 ： 
            MAB = M1 M2 M3 M4 
            其中： M1 = MS11 MS12 MS13 MS14 MS15 MS16 MS17 MS18 
            M2 = MS21 MS22 MS23 MS24 MS25 MS26 MS27 MS28 
            M3 = MS31 MS32 MS33 MS34 MS35 MS36 MS37 MS38 
            M4 = MS41 MS42 MS43 MS44 MS45 MS46 MS47 MS48 
            按如下规则进行异或运算： 
            MS11 MS12 MS13 MS14 MS15 MS16 MS17 MS18 
            XOR） MS21 MS22 MS23 MS24 MS25 MS26 MS27 MS28 
            --------------------------------------------------- 
            TEMP BLOCK1=TM11 TM12 TM13 TM14 TM15 TM16 TM17 TM18 
            然后，进行下一步的运算： 
            TM11 TM12 TM13 TM14 TM15 TM16 TM17 TM18 
            XOR） MS31 MS32 MS33 MS34 MS35 MS36 MS37 MS38 
            --------------------------------------------------- 
            TEMP BLOCK2=TM21 TM22 TM23 TM24 TM25 TM26 TM27 TM28 
            再进行下一步的运算： 
            TM21 TM22 TM23 TM24 TM25 TM26 TM27 TM28 
            XOR） MS41 MS42 MS43 MS44 MS45 MS46 MS47 MS48 
            --------------------------------------------------- 
            RESULT BLOCK=TM31 TM32 TM33 TM34 TM35 TM36 TM37 TM38 
            3、将异或运算后的最后8个字节（RESULT BLOCK）转换成16 个HEXDECIMAL： 
            RESULT BLOCK =TM31 TM32 TM33 TM34 TM35 TM36 TM37 TM38 
            =TM311 TM312 TM321 TM322 TM331 TM332 TM341 TM342 || 
            TM351 TM352 TM361 TM362 TM371 TM372 TM381 TM382 
            4、取前8 个字节用MAK加密： 
            ENC BLOCK1 = eMAK（TM311 TM312 TM321 TM322 TM331 TM332 TM341 TM342） 
            = EN11 EN12 EN13 EN14 EN15 EN16 EN17 EN18 
            5、将加密后的结果与后8 个字节异或： 
            EN11 EN12 EN13 EN14 EN15 EN16 EN17 EN18 
            XOR） TM351 TM352 TM361 TM362 TM371 TM372 TM381 TM382 
            ------------------------------------------------------------ 
            TEMP BLOCK=TE11 TE12 TE13 TE14 TE15 TE16 TE17 TE18 
            6、用异或的结果TEMP BLOCK 再进行一次DES运算。 
            ENC BLOCK2 = eMAK（TE11 TE12 TE13 TE14 TE15 TE16 TE17 TE18） 
            = EN21 EN22 EN23 EN24 EN25 EN26 EN27 EN28 
            7、将运算后的结果（ENC BLOCK2）转换成16 个HEXDECIMAL： 
            ENC BLOCK2 = EN21 EN22 EN23 EN24 EN25 EN26 EN27 EN28 
            = EM211 EM212 EM221 EM222 EM231 EM232 EM241 EM242 || 
            EM251 EM252 EM261 EM262 EM271 EM272 EM281 EM282 
            如： 
            ENC RESULT= %H84, %H56, %HB1, %HCD, %H5A, %H3F, %H84, %H84 
            转换成16 个HEXDECIMAL: 8456B1CD5A3F8484 
            8、取前8个字节作为MAC值。 
            取8456B1CD为MAC值．
             * **/
            #endregion
            byte[] Data = Add8Bit(data);//1.不足8的倍数补00 
            byte[] b_BufArr1 = new byte[8];
            byte[] b_BufArr2 = new byte[8];
            int iGroup = Data.Length / 8;
            Array.Copy(Data, 0, b_BufArr1, 0, 8);//先把第一组开始 
            for (int i = 1; i < iGroup; i++)//2.循环每组做异或 
            {
                Array.Copy(Data, 8 * i, b_BufArr2, 0, 8);
                b_BufArr1 = XOR(b_BufArr1, b_BufArr2);
            }
            //将异或运算后的最后8个字节转换成16个HEXDECIMAL 
            string resultBlock = ByteToHex(b_BufArr1);//3.最后的异或16进制字符串 
            byte[] bytStep3 = Encoding.ASCII.GetBytes(resultBlock);//转为BYTE
            //将第一步得到的HEX字串转换成两个byte[8] 
            byte[] bytStep41 = new byte[8];
            Array.Copy(bytStep3, 0, bytStep41, 0, 8);
            byte[] bytStep42 = new byte[8];
            Array.Copy(bytStep3, 8, bytStep42, 0, 8);
            //下面注释的简化
            byte[] r1 = Encrypt.ECB.Encrypt(bytStep41, key, iv);//将第一组进行DES
            byte[] r2 = XOR(r1, bytStep42);//将加密结果与第二组进行异或
            string result = ByteToHex(Encrypt.ECB.Encrypt(r2, key, iv));//异或结果进行DES
            return result;
        }
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

        public static byte[] Add8Bit(byte[] SourMACData)
        {
            byte[] TarMacData = null;
            int iGroup = 0;
            int len_source = SourMACData.Length;
            int YuShu = SourMACData.Length % 8;
            if ((YuShu == 0))
            {
                iGroup = SourMACData.Length / 8;
                return SourMACData;
            }
            else
            {
                iGroup = SourMACData.Length / 8 + 1;
                TarMacData = new byte[iGroup * 8];
                Array.Copy(SourMACData, TarMacData, len_source);
                return FillChar(len_source + 1, TarMacData, 0x00);
            }
        }

        public static byte[] FillChar(int StartFillIndex, byte[] b_data, byte Fill)
        {
            for (int i = StartFillIndex; i < b_data.Length; i++)
            {
                b_data[i] = Fill;
            }
            return b_data;
        }

        public static byte[] XOR(byte[] b1, byte[] b2)
        {
            int len = 0;
            if (b1.Length != b2.Length)
                return null;
            len = b1.Length;
            byte[] b_target = new byte[len];
            for (int i = 0; i < len; i++)
            {
                b_target[i] = (byte)(b1[i] ^ b2[i]);
            }
            return b_target;
        }

        #region pin计算
        /// <summary>
        /// pinBlock计算
        /// strCardNo长度需大于12位
        /// </summary>
        /// <param name="strPassword"></param>
        /// <param name="strCardNo"></param>
        /// <returns></returns>
        public static byte[] pinBlock(String strPassword, String strCardNo)
        {
            if (strPassword.Length > 8)
                strPassword = strPassword.Substring(0, 8);
            //PIN BLOCK - 8位
            byte[] bytesPin = new byte[] { (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF };
            bytesPin[0] = (byte)strPassword.Length;
            byte[] bcdPwd = str2bcd(strPassword);
            System.Array.Copy(bcdPwd, 0, bytesPin, 1, bcdPwd.Length);
            //PAN  - 这里没算了前面的0，是6位
            int nLength = strCardNo.Length;
            //if (nLength < 12)
            //{
            //    strCardNo = "0" + strCardNo;
            //}
            //从卡号右边数第二位开始，向左取12位
            String strCardNo12 = strCardNo.Substring(nLength - 13, 12);
            byte[] bcdPAN = str2bcd(strCardNo12);
            //异或
            byte[] bytesPinBlock = new byte[8];
            bytesPinBlock[0] = bytesPin[0];
            bytesPinBlock[1] = bytesPin[1];
            for (int i = 2; i < 8; i++)
            {
                bytesPinBlock[i] = (byte)(bytesPin[i] ^ bcdPAN[i - 2]);
            }
            return bytesPinBlock;
        }
        /// <summary>
        /// 从PINBLOCK中反转密码
        /// </summary>
        /// <param name="strCardNo"></param>
        /// <param name="bytesPinBlock"></param>
        /// <returns></returns>
        public static string getPassFromPinBlock(string strCardNo, byte[] bytesPinBlock)
        {
            strCardNo = strCardNo.PadLeft(13, '0');
            int nLength = strCardNo.Length;
            String strCardNo12 = strCardNo.Substring(nLength - 13, 12);
            byte[] bcdPAN = str2bcd(strCardNo12);
            byte[] bytesPin = new byte[] { (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)0xFF };
            for (int i = 2; i < 8; i++)
            {
                bytesPin[i] = (byte)(bytesPinBlock[i] ^ bcdPAN[i - 2]);
            }
            bytesPin[0] = bytesPinBlock[0];
            bytesPin[1] = bytesPinBlock[1];
            byte[] bcdPwd = new byte[3];
            System.Array.Copy(bytesPin, 1, bcdPwd, 0, bcdPwd.Length);
            return bcd2Str(bcdPwd);
        }

        /// <summary>
        /// 10进制串转为BCD码
        /// </summary>
        /// <param name="strTemp"></param>
        /// <returns></returns>
        public static Byte[] str2bcd(string strTemp)
        {
            try
            {
                if (Convert.ToBoolean(strTemp.Length & 1))//数字的二进制码最后1位是1则为奇数
                {
                    strTemp = "0" + strTemp;//数位为奇数时前面补0
                }
                Byte[] aryTemp = new Byte[strTemp.Length / 2];
                for (int i = 0; i < (strTemp.Length / 2); i++)
                {
                    aryTemp[i] = (Byte)(((strTemp[i * 2] - '0') << 4) | (strTemp[i * 2 + 1] - '0'));
                }
                return aryTemp;//高位在前
            }
            catch (Exception ero)
            { return null; }
        }

        public static string bcd2Str(byte[] bytes)
        {

            StringBuilder temp = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                temp.Append((byte)((bytes[i] & 0xf0) >> 4));
                temp.Append((byte)(bytes[i] & 0x0f));
            }
            return temp.ToString().Substring(0, 1).Equals("0") ? temp.ToString().Substring(1) : temp.ToString();
        }
        #endregion
        
    }
}
