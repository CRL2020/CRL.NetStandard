using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CRL.Core
{
    public partial class StringHelper
    {
        #region 判断

        /// <summary>
        /// 判断字符串是否在一个以‘_ , |’隔开的字符串里
        /// </summary>
        /// <param name="str">目标字符串</param>
        /// <param name="strs">要查找的字符串</param>
        /// <returns></returns>
        public static bool IsInStrs(string str, string strs)
        {
            return IsInParams(str, strs.Split('_', ',', '|'));
        }

        /// <summary>
        /// 判断字符串是否在一个数组里
        /// </summary>
        /// <param name="str">目标字符串</param>
        /// <param name="strs">要查找的字符串数组</param>
        /// <returns></returns>
        public static bool IsInParams(string str, params string[] strs)
        {
            foreach (string s in strs)
            {
                if (s == str)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否是null或空字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool IsNullOrEmpty(object obj)
        {
            string str = obj + "";
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 是否不为空字符串也不是null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotNullAndEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 是否是Integer
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInteger(string str)
        {
            int i;
            return int.TryParse(str, out i);
        }
        /// <summary>
        /// 是否是double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDouble(string str)
        {
            double dbl;
            return double.TryParse(str, out dbl);
        }
        /// <summary>
        /// 是否是single
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSingle(string str)
        {
            Single flt;
            return Single.TryParse(str, out flt);
        }

        /// <summary>
        /// 是否是ip地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip,StringCommon.RegIp);

        }
        
        /// <summary>
        /// 是否是手机号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsCellPhone(string str)
        {
            Regex rphone = new Regex(StringCommon.RegCellphone);
            return rphone.IsMatch(str);
        }
        /// <summary>
        /// 是否是固话号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsPhone(string str)
        { 
            Regex rphone = new Regex(StringCommon.RegTelephone);
            return rphone.IsMatch(str);
        }

        /// <summary>
        /// 是否是邮箱地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmail(string str)
        {
            Regex remail = new Regex(StringCommon.RegEmail);
            return remail.IsMatch(str);
        }
        /// <summary>
        /// 是否是中国公民身份证号
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool IsIDCard(string Id)
        {
            if (Id.Length == 18)
            {
                return isIDCard18(Id);
                
            }
            else if (Id.Length == 15)
            {
                return  isIDCard15(Id);
            }
            else
            {
                return false;
            }
        }
        private static bool isIDCard18(string Id)
        {
            long n = 0;
            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }
        private static bool isIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            return true;//符合15位身份证标准
        } 
        public static bool CustomRegex(string inputStr,string express)
        {
            Regex regex = new Regex(express);
            return regex.IsMatch(inputStr);
        }

        #endregion
    }
}
