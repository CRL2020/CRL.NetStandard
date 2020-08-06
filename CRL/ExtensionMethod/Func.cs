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
        /// 表示函数格式化
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="format">like cast({0} as datetime)</param>
        /// <returns></returns>
        public static TResult FuncFormat<TResult>(this DateTime field,string format)
        {
            return default(TResult);
        }
        public static TResult FuncFormat<TResult>(this string field, string format)
        {
            return default(TResult);
        }
        public static TResult FuncFormat<TResult>(this int field, string format)
        {
            return default(TResult);
        }
        public static TResult FuncFormat<TResult>(this double field, string format)
        {
            return default(TResult);
        }
        public static TResult FuncFormat<TResult>(this decimal field, string format)
        {
            return default(TResult);
        }
        public static TResult FuncFormat<TResult>(this float field, string format)
        {
            return default(TResult);
        }
        public static TResult FuncFormat<TResult>(this Guid field, string format)
        {
            return default(TResult);
        }
        /// <summary>
        /// 分割字符串取第一组
        /// </summary>
        /// <param name="field"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public static string SplitFirst(this string field, string par)
        {
            return field;
        }
    }
}
