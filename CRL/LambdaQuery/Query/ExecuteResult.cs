/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.LambdaQuery
{
    public abstract partial class LambdaQuery<T> where T : IModel, new()
    {
        #region 获取一条记录

        /// <summary>
        /// 获取一条
        /// </summary>
        /// <returns></returns>
        public dynamic ToSingleDynamic()
        {
            return Top(1).ToDynamic().FirstOrDefault();
        }
        /// <summary>
        /// 获取一条
        /// </summary>
        /// <returns></returns>
        public T ToSingle()
        {
            return Top(1).ToList().FirstOrDefault();
        }
        /// <summary>
        /// 获取一条
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToSingle<TResult>()where TResult : class,new()
        {
            return Top(1).ToList<TResult>().FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// 返回动态对象
        /// 会按GROUP和分页判断
        /// </summary>
        /// <returns></returns>
        public List<dynamic> ToDynamic()
        {
            var db = DBExtendFactory.CreateDBExtend(__DbContext);
            if (__DbContext.DBHelper.CurrentDBType== DBAccess.DBType.MongoDB)
            {
                var method = db.GetType().GetMethod("QueryDynamic2", BindingFlags.Public | BindingFlags.Instance);
                var list = method.MakeGenericMethod(new Type[] { typeof(T) }).Invoke(db, new object[] { this });
                return list as List<dynamic>;
            }
            return db.QueryDynamic(this);
        }
        /// <summary>
        /// 返回指定类型
        /// 会按GROUP和分页判断
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public List<TResult> ToList<TResult>()
            where TResult : class,new()
        {
            var db = DBExtendFactory.CreateDBExtend(__DbContext);
            return db.QueryResult<TResult>(this);
        }
        /// <summary>
        /// 返回当前类型
        /// 会按GROUP和分页判断
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            var db = DBExtendFactory.CreateDBExtend(__DbContext);
            return db.QueryList(this);
        }
        /// <summary>
        /// 返回字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>()
        {
            var db = DBExtendFactory.CreateDBExtend(__DbContext);
            return db.ToDictionary<T, TKey, TValue>(this);
        }

        #region 返回首列结果
        /// <summary>
        /// 返回首列结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult ToScalar<TResult>()
        {
            var db = DBExtendFactory.CreateDBExtend(__DbContext);
            var result = db.QueryScalar(this);
            if (result == null || result is DBNull)
            {
                return default(TResult);
            }
            return (TResult)result;
        }
        /// <summary>
        /// 返回首列结果
        /// </summary>
        /// <returns></returns>
        public dynamic ToScalar()
        {
            var db = DBExtendFactory.CreateDBExtend(__DbContext);
            return db.QueryScalar(this);
        }
        #endregion
    }
}
