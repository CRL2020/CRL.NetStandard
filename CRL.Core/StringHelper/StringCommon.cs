using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core
{
    internal class StringCommon
    {
        /// <summary>
        /// 邮箱正则表达式
        /// </summary>
        public static readonly string RegEmail = @"^[-_A-Za-z0-9\.]+@([_A-Za-z0-9]+\.)+[A-Za-z0-9]{2,3}$";
        public static readonly string RegEmailNoEnds = @"[-_A-Za-z0-9]+@([_A-Za-z0-9]+\.)+[A-Za-z0-9]{2,3}";

        /// <summary>
        /// 固话号正则表达式
        /// </summary>
        public static readonly string RegTelephone = "^(0[0-9]{2,3}-)?([2-9][0-9]{6,7})+(-[0-9]{1,4})?$";
        public static readonly string RegTelephoneNoEnds = "(0[0-9]{2,3}-)?([2-9][0-9]{6,7})+(-[0-9]{1,4})?";

        /// <summary>
        /// 手机号正则表达式
        /// </summary>
        public static readonly string RegCellphone = @"^(13[0-9]|15[0-9]|18[0-9]|147)\d{8}$";
        public static readonly string RegCellphoneNoEnds = @"(13[0-9]|15[0-9]|18[0-9]|147)\d{8}";

       
        /// <summary>
        /// ip地址表达式
        /// </summary>
        public static readonly string RegIp=@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";
    }
}
