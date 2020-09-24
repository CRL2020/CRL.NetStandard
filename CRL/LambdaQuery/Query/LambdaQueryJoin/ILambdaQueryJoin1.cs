/**
* EFCore.QueryExtensions
*/
using System;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface ILambdaQueryJoin<T, T2, T3, T4, T5>
    {
        ILambdaQueryJoin<T, T2, T3, T4, T5> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4, T5> JoinAfter(Expression<Func<T5, bool>> expression);
        ILambdaQueryJoin<T, T2, T3, T4, T5> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> expression, bool desc = true);
        ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4, T5> SelectAppendValue<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4, T5> SelectField<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> resultSelector);
        ILambdaQueryJoin<T, T2, T3, T4, T5> Where(Expression<Func<T, T2, T3, T4, T5, bool>> expression);
    }
}