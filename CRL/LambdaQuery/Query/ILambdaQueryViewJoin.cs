/**
* EFCore.QueryExtensions
*/
using System;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface ILambdaQueryViewJoin<T, TJoinResult> where T : class
    {
        ILambdaQueryViewJoin<T, TJoinResult> OrderBy<TResult>(Expression<Func<TJoinResult, TResult>> expression, bool desc = true);
        ILambdaQueryResultSelect<TJoinResult2> Select<TJoinResult2>(Expression<Func<T, TJoinResult, TJoinResult2>> resultSelector) where TJoinResult2 : class;
        ILambdaQuery<T> SelectAppendValue<TJoinResult2>(Expression<Func<TJoinResult, TJoinResult2>> resultSelector);
    }
}