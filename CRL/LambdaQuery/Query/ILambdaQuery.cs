/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;

using System.Collections.Generic;

using System.Linq.Expressions;


namespace CRL.LambdaQuery
{
    public interface ILambdaQuery<T> where T : IModel, new()
    {
        double AnalyticalTime { get; }
        double ExecuteTime { get; }
        double MapingTime { get; }
        int RowCount { get; }

        LambdaQuery<T> CompileToSp(bool compileSp);

        LambdaQuery<T2> CreateQuery<T2>() where T2 : IModel, new();

        LambdaQuery<T> Equal<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new();

        LambdaQuery<T> Equal<TResult>(LambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        LambdaQuery<T> Exists<TInner>(Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new();

        LambdaQuery<T> Exists<TResult>(LambdaQueryResultSelect<TResult> query);

        LambdaQuery<T> Expire(int expireMinute);

        //string GetOrderBy();

        GroupQuery<T> GroupBy<TResult>(Expression<Func<T, TResult>> resultSelector);

        LambdaQuery<T> In<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new();

        LambdaQuery<T> In<TResult>(LambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        LambdaQueryJoin<T, T2> Join<T2>(Expression<Func<T, T2, bool>> expression, JoinType joinType = JoinType.Inner);

        LambdaQueryViewJoin<T, TJoinResult> Join<TJoinResult>(LambdaQueryResultSelect<TJoinResult> resultSelect, Expression<Func<T, TJoinResult, bool>> expression, JoinType joinType = JoinType.Inner);

        LambdaQuery<T> NotEqual<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new();

        LambdaQuery<T> NotEqual<TResult>(LambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        LambdaQuery<T> NotExists<TInner>(Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new();

        LambdaQuery<T> NotExists<TResult>(LambdaQueryResultSelect<TResult> query);

        LambdaQuery<T> NotIn<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner : IModel, new();

        LambdaQuery<T> NotIn<TResult>(LambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        LambdaQuery<T> Or(Expression<Func<T, bool>> expression);

        LambdaQuery<T> OrderBy(string orderBy);

        LambdaQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression, bool desc = true);

        LambdaQuery<T> OrderByPrimaryKey(bool desc);

        LambdaQuery<T> Page(int pageSize = 15, int pageIndex = 1);

        string PrintQuery(bool uselog = false);

        //LambdaQuery<T> Select(Expression resultSelectorBody);

        LambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, TResult>> resultSelector = null);

        LambdaQuery<T> ShardingUnion(UnionType unionType);

        LambdaQuery<T> Take(int top);

        Dictionary<TKey, TValue> ToDictionary<TKey, TValue>();

        List<dynamic> ToDynamic();

        List<T> ToList();

        List<TResult> ToList<TResult>() where TResult : class, new();

        LambdaQuery<T> Top(int top);

        dynamic ToScalar();

        TResult ToScalar<TResult>();

        T ToSingle();

        TResult ToSingle<TResult>() where TResult : class, new();

        dynamic ToSingleDynamic();

        //string ToString();

        LambdaQuery<T> Where(Expression<Func<T, bool>> expression);

        LambdaQuery<T> Where(string expression);

        LambdaQuery<T> WhereIf(Expression<Func<T, bool>> expression, bool boolEx);

        LambdaQuery<T> WhereNotNull(Expression<Func<T, bool>> expression);

        LambdaQuery<T> WithNoLock(bool _nolock = true);

        LambdaQuery<T> WithTrackingModel(bool trackingModel = true);

    }
}