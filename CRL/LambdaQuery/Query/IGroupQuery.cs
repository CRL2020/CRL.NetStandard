/**
* EFCore.QueryExtensions
*/
using System;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface IGroupQuery<T>
    {
        IGroupQuery<T> GroupHaving(Expression<Func<T, bool>> expression);
        IGroupQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression, bool desc = true);
        ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, TResult>> resultSelector);
    }
}