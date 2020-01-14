using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.Reflection;


namespace CRL.Core
{
    /// <summary>
    /// 字符串加密类
    /// </summary>
    public partial class StringHelper
    {
        /// <summary>
        /// 前补0
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="size">总长度</param>
        /// <returns></returns>
        public static string FillZero(string value, int size)
        {
            string tmp = "";
            for (int i = 0; i < size - value.Length; i++)
            {
                tmp += "0";
            }

            return tmp + value;
        }




        public static string SerializerToJson(object obj)
        {
            return SerializeHelper.SerializerToJson(obj);
        }


        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="number">截取数量</param>
        /// <returns></returns>
        public static string InterceptString(string str, int number)
        {
            if (str == null || str.Length == 0 || number <= 0) return "";
            int iCount = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(str);
            if (iCount > number)
            {
                int iLength = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    int iCharLength = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(new char[] { str[i] });
                    iLength += iCharLength;
                    if (iLength == number)
                    {
                        str = str.Substring(0, i + 1);
                        break;
                    }
                    else if (iLength > number)
                    {
                        str = str.Substring(0, i);
                        break;
                    }
                }
            }
            return str;

        }
        /// <summary>
        /// 截取字符串，以“.”结束
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="number">截取数量</param>
        /// <returns></returns>
        public static string InterceptStringEndDot(string str, int number)
        {
            if (str == null || str.Length == 0 || number <= 0) return "";
            int iCount = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(str);
            if (iCount > number)
            {
                int iLength = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    int iCharLength = System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(new char[] { str[i] });
                    iLength += iCharLength;
                    if (iLength == number)
                    {
                        str = str.Substring(0, i + 1) + "…";
                        break;
                    }
                    else if (iLength > number)
                    {
                        str = str.Substring(0, i) + "…";
                        break;
                    }
                }
            }
            return str;

        }

        

        /// <summary>
        /// 判断字符串是否在几个字符之中
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <param name="strs">几个字符串，就是范围</param>
        /// <returns>如果在返回true，否则返回false</returns>
        public static bool IsIn(string str, params string[] strs)
        {
            foreach (string s in strs)
            {
                if (s == str)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 得到随机数
        /// </summary>
        /// <param name="count">个数</param>
        /// <returns></returns>
        public static string GetCheckCode(int count)
        {
            char[] character = { '1', '2', '3', '4', '5', '6', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            Random rnd = new Random();
            //生成验证码字符串
            string chkCode = string.Empty;
            for (int i = 0; i < count; i++)
            {
                chkCode += character[rnd.Next(character.Length)];
            }
            return chkCode;
        }



        /// <summary>
        /// 得到0-10的汉字显示
        /// </summary>
        /// <param name="number"></param>
        /// <param name="isTraditional"></param>
        /// <returns></returns>
        public static string GetChineseNumber(int number, bool isTraditional)
        {
            var str = number.ToString();
            string str2 = "";
            foreach(var s in str)
            {
                str2 += GetChineseNumber(s.ToString(), isTraditional);
            }
            return str2;
        }
        /// <summary>
        /// 得到0-10的汉字显示
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="isTraditional">是否是繁体</param>
        /// <returns></returns>
        public static string GetChineseNumber(string number,bool isTraditional)
        {
            switch (number)
            { 
                case "1":
                    return isTraditional ? "壹" : "一";
                case "2":
                    return isTraditional ? "贰" : "二";
                case "3":
                    return isTraditional ? "叁" : "三";
                case "4":
                    return isTraditional ? "肆" : "四";
                case "5":
                    return isTraditional ? "伍" : "五";
                case "6":
                    return isTraditional ? "陆" : "六";
                case "7":
                    return isTraditional ? "柒" : "七";
                case "8":
                    return isTraditional ? "捌" : "八";
                case "9":
                    return isTraditional ? "玖" : "九";
                case "10":
                    return isTraditional ? "拾" : "十";
                case "0":
                    return isTraditional ? "零" : "〇";
                default:
                    return string.Empty;
            }
        }
    }
}
