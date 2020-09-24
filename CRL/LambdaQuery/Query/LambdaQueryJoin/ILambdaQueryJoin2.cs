/**
* EFCore.QueryExtensions
*/
using System;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface ILambdaQueryJoin<T, T2>
    {
        ILambdaQueryJoin<T, T2> GroupBy<TResult>(Expression<Func<T, T2, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3> Join<T3>(Expression<Func<T, T2, T3, bool>> expression, JoinType joinType = JoinType.Inner);
        ILambdaQueryJoin<T, T2> JoinAfter(Expression<Func<T2, bool>> expression);
        ILambdaQueryJoin<T, T2> OrderBy<TResult>(Expression<Func<T, T2, TResult>> expression, bool desc = true);
        ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, T2, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2> SelectAppendValue<TResult>(Expression<Func<T, T2, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2> SelectField<TResult>(Expression<Func<T, T2, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2> Where(Expression<Func<T, T2, bool>> expression);
    }
}