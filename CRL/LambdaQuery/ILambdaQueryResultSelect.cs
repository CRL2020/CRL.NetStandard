/**
* EFCore.QueryExtensions
*/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CRL.LambdaQuery
{
    public interface ILambdaQueryResultSelect<TResult>
    {
        LambdaQueryBase BaseQuery { get; set; }
        Type InnerType { get; }
        ILambdaQueryResultSelect<TResult> HavingCount(Expression<Func<TResult, bool>> expression);
        ILambdaQueryResultSelect<TResult> OrderBy<TResult2>(Expression<Func<TResult, TResult2>> expression, bool desc = true);
        List<dynamic> ToDynamic();
        List<TResult> ToList();
        List<TResult2> ToList<TResult2>();
        ILambdaQueryResultSelect<TResult> Union<TResult2>(ILambdaQueryResultSelect<TResult2> resultSelect, UnionType unionType = UnionType.UnionAll);
    }
}