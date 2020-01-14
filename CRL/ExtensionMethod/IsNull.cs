/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL
{
    public static partial class ExtensionMethod
    {
        /// <summary>
        /// 表示SQL ISNULL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static T IsNull<T>(this T origin, T v) where T : struct
        {
            return default(T);
        }
        /// <summary>
        /// 表示SQL ISNULL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static T IsNull<T>(this T? origin, T v) where T : struct
        {
            return default(T);
        }
        /// <summary>
        /// 表示SQL ISNULL
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string IsNull(this string origin, string v)
        {
            return v;
        }
    }
}
