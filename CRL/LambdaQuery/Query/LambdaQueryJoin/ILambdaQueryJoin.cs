/**
* EFCore.QueryExtensions
*/
using System;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface ILambdaQueryJoin<T, T2, T3, T4>
    {
        ILambdaQueryJoin<T, T2, T3, T4> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4, T5> Join<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression, JoinType joinType = JoinType.Inner);
        ILambdaQueryJoin<T, T2, T3, T4> JoinAfter(Expression<Func<T4, bool>> expression);
        ILambdaQueryJoin<T, T2, T3, T4> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> expression, bool desc = true);
        ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4> SelectAppendValue<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4> SelectField<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> expression);
    }
}