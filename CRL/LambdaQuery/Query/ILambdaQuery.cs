/**
* EFCore.QueryExtensions
*/
using System;

using System.Collections.Generic;

using System.Linq.Expressions;


namespace CRL.LambdaQuery
{
    public interface ILambdaQuery<T> where T :class
    {
        double AnalyticalTime { get; }
        double ExecuteTime { get; }
        double MapingTime { get; }
        int RowCount { get; }

        ILambdaQuery<T> CompileToSp(bool compileSp);

        ILambdaQuery<T2> CreateQuery<T2>() where T2 :class;

        ILambdaQuery<T> Equal<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner :class;

        ILambdaQuery<T> Equal<TResult>(ILambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        ILambdaQuery<T> Exists<TInner>(Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner :class;

        ILambdaQuery<T> Exists<TResult>(ILambdaQueryResultSelect<TResult> query);

        ILambdaQuery<T> Expire(int expireMinute);

        string GetOrderBy();

        IGroupQuery<T> GroupBy<TResult>(Expression<Func<T, TResult>> resultSelector);

        ILambdaQuery<T> In<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner :class;

        ILambdaQuery<T> In<TResult>(ILambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        ILambdaQueryJoin<T, T2> Join<T2>(Expression<Func<T, T2, bool>> expression, JoinType joinType = JoinType.Inner);

        ILambdaQueryViewJoin<T, TJoinResult> Join<TJoinResult>(ILambdaQueryResultSelect<TJoinResult> resultSelect, Expression<Func<T, TJoinResult, bool>> expression, JoinType joinType = JoinType.Inner);

        ILambdaQuery<T> NotEqual<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner :class;

        ILambdaQuery<T> NotEqual<TResult>(ILambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        ILambdaQuery<T> NotExists<TInner>(Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner :class;

        ILambdaQuery<T> NotExists<TResult>(ILambdaQueryResultSelect<TResult> query);

        ILambdaQuery<T> NotIn<TInner>(Expression<Func<T, object>> outField, Expression<Func<TInner, object>> innerField, Expression<Func<T, TInner, bool>> expression) where TInner :class;

        ILambdaQuery<T> NotIn<TResult>(ILambdaQueryResultSelect<TResult> query, Expression<Func<T, TResult>> outField);

        ILambdaQuery<T> Or(Expression<Func<T, bool>> expression);

        ILambdaQuery<T> OrderBy(string orderBy);

        ILambdaQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression, bool desc = true);

        ILambdaQuery<T> OrderByPrimaryKey(bool desc);

        ILambdaQuery<T> Page(int pageSize = 15, int pageIndex = 1);

        string PrintQuery(bool uselog = false);

        ILambdaQuery<T> Select(Expression resultSelectorBody);

        ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, TResult>> resultSelector = null);

        ILambdaQuery<T> ShardingUnion(UnionType unionType);

        ILambdaQuery<T> Take(int top);

        Dictionary<TKey, TValue> ToDictionary<TKey, TValue>();

        List<dynamic> ToDynamic();

        List<T> ToList();

        List<TResult> ToList<TResult>() where TResult :class;

        ILambdaQuery<T> Top(int top);

        dynamic ToScalar();

        TResult ToScalar<TResult>();

        T ToSingle();

        TResult ToSingle<TResult>() where TResult :class;

        dynamic ToSingleDynamic();

        string ToString();

        ILambdaQuery<T> Where(Expression<Func<T, bool>> expression);

        ILambdaQuery<T> Where(string expression);

        ILambdaQuery<T> WhereIf(Expression<Func<T, bool>> expression, bool boolEx);

        ILambdaQuery<T> WhereNotNull(Expression<Func<T, bool>> expression);

        ILambdaQuery<T> WithNoLock(bool _nolock = true);

        ILambdaQuery<T> WithTrackingModel(bool trackingModel = true);

    }
}
