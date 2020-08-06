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
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
using CRL.LambdaQuery;
namespace CRL
{
    public static partial class ExtensionMethod
    {
        /// <summary>
        /// 表示MIN此字段
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static string MIN(this string origin)
        {
            return origin;
        }
        /// <summary>
        /// 表示MIN此字段
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static DateTime MIN(this DateTime origin)
        {
            return origin;
        }
        /// <summary>
        /// 表示MIN此字段
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static DateTime MIN(this DateTime? origin)
        {
            return origin.Value;
        }
        /// <summary>
        /// 表示MIN此字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static T MIN<T>(this T origin) where T : struct
        {
            return origin;
        }
        /// <summary>
        /// 表示MIN此字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static T MIN<T>(this T? origin) where T : struct
        {
            return origin.Value;
        }
        /// <summary>
        /// 表示MIN一个属性二元运算 如 MIN(b=>b.Num*b.Price)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="origin"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public static TResult MIN<T, TResult>(this T origin, Expression<Func<T, TResult>> resultSelector) where T : IModel
        {
            return default(TResult);
        }
    }
}
