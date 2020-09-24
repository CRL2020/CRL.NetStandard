/**
* EFCore.QueryExtensions
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
    public enum JoinType
    {
        Left,
        Inner,
        Right,
        Full
    }

    public abstract partial class LambdaQuery<T> : LambdaQueryBase where T :class
    {
        /// <summary>
        /// 创建一个JOIN查询分支
        /// </summary>
        /// <typeparam name="T2">关联类型</typeparam>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2> Join<T2>(Expression<Func<T, T2, bool>> expression,JoinType joinType = JoinType.Inner) 
        {
            var query2 = new LambdaQueryJoin<T, T2>(this);
            __Join<T2>(expression.Body, joinType);
            return query2;
        }

        /// <summary>
        /// 创建关联一个强类型查询
        /// </summary>
        /// <typeparam name="TJoinResult"></typeparam>
        /// <param name="resultSelect"></param>
        /// <param name="expression"></param>
        /// <param name="joinType"></param>
        /// <returns></returns>
        public ILambdaQueryViewJoin<T, TJoinResult> Join<TJoinResult>(ILambdaQueryResultSelect<TJoinResult> resultSelect, Expression<Func<T, TJoinResult, bool>> expression, JoinType joinType = JoinType.Inner) 
        {
            if(!resultSelect.BaseQuery.__FromDbContext)
            {
                throw new Exception("关联需要由LambdaQuery.CreateQuery创建");
            }
            var query2 = new LambdaQueryViewJoin<T, TJoinResult>(this, resultSelect);
            //var innerType = typeof(TSource);
            var innerType = resultSelect.InnerType;

            //var prefix1 = GetPrefix(innerType);
            var prefix2 = GetPrefix(typeof(TJoinResult));
            var typeQuery = new TypeQuery(innerType, prefix2);
            var baseQuery = resultSelect.BaseQuery;
            QueryParames.AddRange(baseQuery.QueryParames);

            string innerQuery = baseQuery.GetQuery();
            typeQuery.InnerQuery = innerQuery;
            string condition = FormatJoinExpression(expression.Body);
            AddInnerRelation(typeQuery, joinType, condition);
            return query2;
        }
    }
}
