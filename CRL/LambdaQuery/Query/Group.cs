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
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using CRL.LambdaQuery;
using System.Text.RegularExpressions;

namespace CRL.LambdaQuery
{
    public class GroupQuery<T> 
    {
        LambdaQueryBase BaseQuery;
        internal GroupQuery(LambdaQueryBase query)
        {
            BaseQuery = query;
        }
        public LambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            BaseQuery.__SelectField(resultSelector.Parameters, resultSelector.Body);
            return new LambdaQueryResultSelect<TResult>(BaseQuery, resultSelector.Body);
        }
        /// <summary>
        /// 设置group having条件
        /// like b => b.Number.SUM() > 1
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public GroupQuery<T> GroupHaving(Expression<Func<T, bool>> expression)
        {
            BaseQuery.__GroupHaving(expression.Body);
            return this;
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public GroupQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression, bool desc = true)
        {
            BaseQuery.__OrderBy(expression.Parameters, expression.Body, desc);
            return this;
        }
    }
    public abstract partial class LambdaQuery<T> : LambdaQueryBase where T : IModel, new()
    {
        /// <summary>
        /// 设置GROUP
        /// </summary>
        /// <param name="resultSelector">like b=>new{b.Name,b.Id}</param>
        /// <returns></returns>
        public GroupQuery<T> GroupBy<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            var parameters = resultSelector.Parameters.Select(b => b.Type).ToArray();
            var fields = GetSelectField(false, resultSelector.Body, false, parameters).mapping;
            __GroupFields = fields;
            return new GroupQuery<T>(this);
        }
    }
}
