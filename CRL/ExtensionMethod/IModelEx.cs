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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRL
{
    public static partial class ExtensionMethod
    {
        /// <summary>
        /// 获取对象克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static T GetClone<T>(this T origin) where T : IModel
        {
            return origin.Clone() as T;
        }
        /// <summary>
        /// 获取集合克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetClone<T>(this IEnumerable<T> origin) where T : IModel
        {
            return origin.Select(b => (T)b.Clone());
        }
        /// <summary>
        /// 返回分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static PageData<T> GetPageData<T>(this LambdaQuery.ILambdaQuery<T> query) where T : IModel, new()
        {
            var query2 = query as LambdaQuery.LambdaQuery<T>;
            var obj = new PageData<T>() { CurrentPage = query2.SkipPage, Rows = query.ToList<T>(), RowCount = query.RowCount };
            return obj;
        }

    }
    #region PageData
    public class PageData<T>
    {
        public int CurrentPage
        {
            get; set;
        }
        public int RowCount
        {
            get; set;
        }
        public List<T> Rows
        {
            get; set;
        }
    }
    #endregion
}
