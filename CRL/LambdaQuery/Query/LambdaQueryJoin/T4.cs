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
    /// 关联查询分支
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public sealed class LambdaQueryJoin<T, T2, T3, T4> : ILambdaQueryJoin<T, T2, T3, T4>
    {
        LambdaQueryBase BaseQuery;
        internal LambdaQueryJoin(LambdaQueryBase query)
        {
            BaseQuery = query;
        }

        /// <summary>
        /// 在关联结果后增加条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> expression)
        {
            BaseQuery.__Where(expression.Body);
            return this;
        }
        /// <summary>
        /// 在Join后增加条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4> JoinAfter(Expression<Func<T4, bool>> expression)
        {
            BaseQuery.__JoinAfter<T4>(expression.Body);
            return this;
        }
        /// <summary>
        /// 按关联对象选择查询字段
        /// 可多次调用,不要重复
        /// </summary>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4> SelectField<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector)
        {
            //在关联两次以上,可调用以下方法指定关联对象获取对应的字段
            BaseQuery.__SelectField(resultSelector.Parameters, resultSelector.Body);
            return this;
        }
        /// <summary>
        /// 返回强类型结果选择
        /// 兼容老写法
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public ILambdaQueryResultSelect<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector)
        {
            SelectField(resultSelector);
            return new LambdaQueryResultSelect<TResult>(BaseQuery, resultSelector.Body);
        }

        /// <summary>
        /// 选择TJoin关联值到对象内部索引
        /// 可调用多次,不要重复
        /// </summary>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4> SelectAppendValue<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector)
        {
            BaseQuery.__SelectAppendValue(resultSelector.Parameters, resultSelector.Body);
            return this;
        }
        /// <summary>
        /// 按关联对象设置GROUP字段
        /// 可多次调用,不要重复
        /// </summary>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> resultSelector)
        {
            //在关联两次以上,可调用以下方法指定关联对象获取对应的字段
            BaseQuery.__GroupBy(resultSelector.Parameters, resultSelector.Body);
            return this;
        }
        /// <summary>
        /// 按TJoin排序
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> expression, bool desc = true)
        {
            BaseQuery.__OrderBy(expression.Parameters, expression.Body, desc);
            return this;
        }
        /// <summary>
        /// 在当前关联基础上再创建关联
        /// </summary>
        /// <typeparam name="T5"></typeparam>
        /// <param name="expression"></param>
        /// <param name="joinType"></param>
        /// <returns></returns>
        public ILambdaQueryJoin<T, T2, T3, T4, T5> Join<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression, JoinType joinType = JoinType.Inner)
        {
            var query2 = new LambdaQueryJoin<T, T2, T3, T4, T5>(BaseQuery);
            BaseQuery.__Join<T5>(expression.Body, joinType);
            return query2;
        }
    }
}
