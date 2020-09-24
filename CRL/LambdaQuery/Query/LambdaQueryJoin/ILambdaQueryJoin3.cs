/**
* EFCore.QueryExtensions
*/
using System;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface ILambdaQueryJoin<T, T2, T3>
    {
        ILambdaQueryJoin<T, T2, T3> GroupBy<TResult>(Expression<Func<T, T2, T3, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4> Join<T4>(Expression<Func<T, T2, T3, T4, bool>> expression, JoinType joinType = JoinType.Inner);
        ILambdaQueryJoin<T, T2, T3> JoinAfter(Expression<Func<T3, bool>> expression);
        ILambdaQueryJoin<T, T2, T3> OrderBy<TResult>(Expression<Func<T, T2, T3, TResult>> expression, bool desc = true);
        ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, T2, T3, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3> SelectAppendValue<TResult>(Expression<Func<T, T2, T3, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3> SelectField<TResult>(Expression<Func<T, T2, T3, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> expression);
    }
}