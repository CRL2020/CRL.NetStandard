/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRL.LambdaQuery
{
    public abstract partial class LambdaQueryBase
    {
        /// <summary>
        /// 增加条件
        /// </summary>
        /// <param name="expressionBody"></param>
        internal void __Where(Expression expressionBody)
        {
            string condition = FormatExpression(expressionBody).SqlOut;
            if (Condition.Length > 0)
            {
                condition = " and " + condition;
            }
            Condition.Append(condition);
        }
        /// <summary>
        /// or
        /// </summary>
        /// <param name="expressionBody"></param>
        internal void __Or(Expression expressionBody)
        {
            string condition1 = FormatExpression(expressionBody).SqlOut;
            Condition.AppendFormat(" or {0}", condition1);
        }
        /// <summary>
        /// 在Join后增加条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionBody"></param>
        internal void __JoinAfter<T>(Expression expressionBody)
        {
            string condition = FormatExpression(expressionBody).SqlOut;
            condition = " and " + condition;
            var innerType = typeof(T);
            var typeQuery = new TypeQuery(innerType);
            __Relations[typeQuery].condition += condition;
        }
        /// <summary>
        /// 按关联对象选择查询字段
        /// 可多次调用,不要重复
        /// </summary>
        /// <returns></returns>
        internal void __SelectField(ReadOnlyCollection<ParameterExpression> Parameters, Expression expressionBody)
        {
            //在关联两次以上,可调用以下方法指定关联对象获取对应的字段
            var parameters = Parameters.Select(b => b.Type).ToArray();
            var selectFieldItem = GetSelectField(true, expressionBody, false, parameters);
            SetSelectFiled(selectFieldItem);
        }
        /// <summary>
        /// 选择字段到对象内部索引
        /// 可调用多次,不要重复
        /// </summary>
        /// <returns></returns>
        internal void __SelectAppendValue(ReadOnlyCollection<ParameterExpression> Parameters, Expression expressionBody)
        {
            if (_CurrentSelectFieldCache == null)
            {
                SelectAll(false);
            }
            var parameters = Parameters.Select(b => b.Type).ToArray();
            var selectField = GetSelectField(true, expressionBody, true, parameters);
            SetSelectFiled(selectField);
        }
        /// <summary>
        /// 设置GROUP字段
        /// 可多次调用,不要重复
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="expressionBody"></param>
        internal void __GroupBy(ReadOnlyCollection<ParameterExpression> Parameters, Expression expressionBody)
        {
            //在关联两次以上,可调用以下方法指定关联对象获取对应的字段
            //var innerType = typeof(TJoin);
            var parameters = Parameters.Select(b => b.Type).ToArray();
            var resultFields = GetSelectField(false, expressionBody, false, parameters).mapping;
            if (__GroupFields == null)
            {
                __GroupFields = new List<Attribute.FieldMapping>();
            }
            __GroupFields.AddRange(resultFields);
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="expressionBody"></param>
        /// <param name="desc"></param>
        internal void __OrderBy(ReadOnlyCollection<ParameterExpression> Parameters, Expression expressionBody, bool desc = true)
        {
            var parameters = Parameters.Select(b => b.Type).ToArray();
            //var innerType = typeof(TJoin);
            var fields = GetSelectField(false, expressionBody, false, parameters).mapping;
            SetOrder(fields.First(), desc);
        }
        /// <summary>
        /// 创建关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionBody"></param>
        /// <param name="joinType"></param>
        internal void __Join<T>(Expression expressionBody, JoinType joinType = JoinType.Inner)
        {
            var innerType = typeof(T);
            GetPrefix(innerType);
            string condition = FormatJoinExpression(expressionBody);
            AddInnerRelation(new TypeQuery(innerType), joinType, condition);
        }

        internal void __GroupHaving(Expression expressionBody)
        {
            string condition = FormatExpression(expressionBody).SqlOut;
            Having += string.IsNullOrEmpty(Having) ? condition : " and " + condition;
        }

    }
}
