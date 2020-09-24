/**
* EFCore.QueryExtensions
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRL.LambdaQuery
{
    /// <summary>
    /// 返回强类型选择结果查询
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class LambdaQueryResultSelect<TResult> : ILambdaQueryResultSelect<TResult>
    {
        Expression resultSelectorBody;
        public Type InnerType
        {
            get
            {
                return BaseQuery.__MainType;
            }
        }
        internal LambdaQueryResultSelect(LambdaQueryBase query, Expression _resultSelectorBody)
        {
            resultSelectorBody = _resultSelectorBody;
            BaseQuery = query;

        }
        public LambdaQueryBase BaseQuery { get; set; }
        /// <summary>
        /// 联合查询
        /// 会清除父查询的排序
        /// </summary>
        /// <typeparam name="TResult2"></typeparam>
        /// <param name="resultSelect"></param>
        /// <param name="unionType"></param>
        /// <returns></returns>
        public ILambdaQueryResultSelect<TResult> Union<TResult2>(ILambdaQueryResultSelect<TResult2> resultSelect, UnionType unionType = UnionType.UnionAll)
        {
            BaseQuery.CleanOrder();//清除OrderBy
            BaseQuery.AddUnion(resultSelect.BaseQuery, unionType);
            return this;
        }
        //string __QueryOrderBy = "";
        /// <summary>
        /// 设置排序
        /// 会重置原排序
        /// </summary>
        /// <typeparam name="TResult2"></typeparam>
        /// <param name="expression"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public ILambdaQueryResultSelect<TResult> OrderBy<TResult2>(Expression<Func<TResult, TResult2>> expression, bool desc = true)
        {
            var parameters = expression.Parameters.Select(b => b.Type).ToArray();
            var fields = BaseQuery.GetSelectField(false, expression.Body, false, parameters).mapping;
            BaseQuery.SetOrder(fields.First(), desc);

            return this;
        }
        /// <summary>
        /// 返回自定义类型
        /// </summary>
        /// <typeparam name="TResult2"></typeparam>
        /// <returns></returns>
        public List<TResult2> ToList<TResult2>()
        {
            var db = DBExtendFactory.CreateDBExtend(BaseQuery.__DbContext);
            return db.QueryResult<TResult2>(BaseQuery);
        }
        /// <summary>
        /// 返回筛选类型
        /// </summary>
        /// <returns></returns>
        public List<TResult> ToList()
        {

            if (resultSelectorBody is MemberInitExpression)
            {
                var memberInitExp = (resultSelectorBody as MemberInitExpression);
                resultSelectorBody = memberInitExp.NewExpression;
            }
            var db = DBExtendFactory.CreateDBExtend(BaseQuery.__DbContext);

            if (resultSelectorBody is NewExpression)
            {
                var newExpression = resultSelectorBody as NewExpression;
                return db.QueryResult<TResult>(BaseQuery, newExpression);
            }
            else if (resultSelectorBody is MemberExpression)
            {
                throw new Exception("请返回匿名类型" + resultSelectorBody);
            }
            else if (resultSelectorBody is ParameterExpression)
            {
                return db.QueryResult<TResult>(BaseQuery);
            }

            throw new Exception("ToList不支持此表达式 " + resultSelectorBody);
        }
        /// <summary>
        /// 返回动态类型
        /// </summary>
        /// <returns></returns>
        public List<dynamic> ToDynamic()
        {
            var db = DBExtendFactory.CreateDBExtend(BaseQuery.__DbContext);
            return db.QueryDynamic(BaseQuery);
        }
        /// <summary>
        /// mongodb专用
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ILambdaQueryResultSelect<TResult> HavingCount(Expression<Func<TResult, bool>> expression)
        {
            BaseQuery.GetPrefix(typeof(TResult));
            var CRLExpression = BaseQuery.__Visitor.RouteExpressionHandler(expression.Body);
            BaseQuery.mongoHavingCount = CRLExpression;
            return this;
        }
    }
}
