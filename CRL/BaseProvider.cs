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
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
//using System.Transactions;
using CRL.LambdaQuery;
using CRL.Core;

namespace CRL
{
    /// <summary>
    /// 业务基类
    /// 请实现调用对象Instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseProvider<T> : ProviderOrigin<T>
        where T : class, new()
    {
        internal override DbContextInner GetDbContext()
        {
            dbLocation.ManageName = ManageName;
            var helper = DBConfigRegister.GetDBHelper(dbLocation);
            var dbContext = new DbContextInner(helper, dbLocation);

            return dbContext;
        }
        #region 属性
        /// <summary>
        /// 是否从远程查询缓存
        /// </summary>
        protected virtual bool QueryCacheFromRemote
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 是否启用缓存并行查询(耗CPU,但速度快),默认false
        /// 当数据量大于10W时才会生效
        /// </summary>
        protected virtual bool CacheQueryAsParallel
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region 创建缓存
        /// <summary>
        /// 按类型清除当前所有缓存
        /// </summary>
        public void ClearCache()
        {
            Type type = typeof(T);
            var key = "";
            var db = DBExtend as AbsDBExtend;
            if (TypeCache.GetModelKeyCache(type, db.DatabaseName, out key))
            {
                CRL.MemoryDataCache.CacheService.RemoveCache(key);
                TypeCache.RemoveModelKeyCache(type, db.DatabaseName);
            }
        }
        /// <summary>
        /// 缓存默认查询
        /// </summary>
        /// <returns></returns>
        protected virtual ILambdaQuery<T> CacheQuery()
        {
            return GetLambdaQuery();
        }
        int allCacheCount = -1;
        int AllCacheCount
        {
            get
            {
                if (allCacheCount == -1)
                {
                    allCacheCount = AllCache.Count();
                }
                return allCacheCount;
            }
        }
        /// <summary>
        /// 获取当前对象缓存,不指定条件
        /// </summary>
        public IEnumerable<T> AllCache
        {
            get
            {
                var query = CacheQuery();
                var all = GetCache(query as LambdaQuery<T>);
                if (all == null)
                {
                    return new List<T>();
                }
                return all.Values;
            }
        }

        /// <summary>
        /// 从对象缓存中进行查询
        /// 如果QueryCacheFromRemote为true,则从远端查询
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<T> QueryFromCache(Expression<Func<T, bool>> expression)
        {
            int total;
            return QueryFromCache(expression, out total, 0, 0);
        }
        /// <summary>
        /// 按主键从对象缓存中进行查询一项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T QueryItemFromCache(object key)
        {
            string id = key.ToString();
            if (QueryCacheFromRemote)
            {
                var expression = Base.GetQueryIdExpression<T>(key);
                return QueryItemFromCache(expression);
            }
            else
            {
                var all = GetCache(CacheQuery() as LambdaQuery<T>);
                T item;
                var a = all.TryGetValue(id, out item);
                if (a)
                {
                    return item;
                }
                return null;
            }
        }
        /// <summary>
        /// 从对象缓存中进行查询
        /// 如果QueryCacheFromRemote为true,则从远端查询
        /// 返回一项
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public T QueryItemFromCache(Expression<Func<T, bool>> expression)
        {
            int total;
            int pageIndex = 0;
            int pageSize = 0;
            if (QueryCacheFromRemote)
            {
                pageIndex = 1;
                pageSize = 1;
            }
            var list = QueryFromCache(expression, out total, pageIndex, pageSize);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        /// <summary>
        /// 从对象缓存中进行查询
        /// 如果QueryCacheFromRemote为true,则从远端查询
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="total"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<T> QueryFromCache(Expression<Func<T, bool>> expression, out int total, int pageIndex = 0, int pageSize = 0)
        {
            total = 0;
            //if (QueryCacheFromRemote)
            //{
            //    return QueryFromCacheServer(expression, out total, pageIndex, pageSize);
            //}
            return QueryFromCacheBase(expression, out total, pageIndex, pageSize);
        }
        T QueryFormCacheById(object id)
        {
            var key = id.ToString();
            var all = GetCache(CacheQuery() as LambdaQuery<T>);
            if (all == null)
            {
                return null;
            }
            T item;
            var a = all.TryGetValue(key, out item);
            return item;
        }
        List<T> QueryFromCacheBase(Expression<Func<T, bool>> expression, out int total, int pageIndex = 0, int pageSize = 0)
        {
            total = 0;
            #region 按KEY查找
            if (expression.Body is BinaryExpression)
            {
                var binary = expression.Body as BinaryExpression;
                if (binary.NodeType == ExpressionType.Equal)
                {
                    if (binary.Left is MemberExpression)
                    {
                        var member = binary.Left as MemberExpression;
                        var primaryKey = TypeCache.GetTable(typeof(T)).PrimaryKey.MemberName;
                        if (member.Member.Name == primaryKey)
                        {
                            var value = ConstantValueVisitor.GetParameExpressionValue(binary.Right);
                            var item = QueryFormCacheById(value);
                            var list = new List<T>();
                            if (item != null)
                            {
                                list.Add(item);
                            }
                            total = list.Count();
                            return list; 
                        }
                    }
                }
            }
            #endregion
            var predicate = expression.Compile();
            IEnumerable<T> data;
            if (CacheQueryAsParallel && AllCacheCount > 100000)
            {
                data = AllCache.AsParallel().Where(predicate);
            }
            else
            {
                data = AllCache.Where(predicate);
            }
            total = data.Count();
            if (pageIndex > 0)
            {
                //var data2 = Base.CutList(data, pageIndex, pageSize);
                var data2 = data.Page(pageIndex, pageSize).ToList();
                return data2;
            }
            return data.ToList();
        }
        /// <summary>
        /// 按类型获取缓存,只能在继承类实现,只能同时有一个类型
        /// 不建议直接调用,请调用AllCache或重写调用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected Dictionary<string, T> GetCache(LambdaQuery<T> query)
        {
            Type type = typeof(T);
            int expMinute = query.__ExpireMinute;
            if (expMinute == 0)
                expMinute = 5;
            query.__ExpireMinute = expMinute;
            string dataCacheKey;
            var list = new Dictionary<string, T>();
            var db = DBExtend as AbsDBExtend;
            var a = TypeCache.GetModelKeyCache(type, db.DatabaseName, out dataCacheKey);
            if (!a)
            {
                var helper = db.dbContext.DBHelper;
                foreach(var p in query.QueryParames)
                {
                    helper.AddParam(p.Item1,p.Item2);
                }
                var sql = query.GetQuery();
                list = MemoryDataCache.CacheService.GetCacheList<T>(sql, query.GetFieldMapping(), expMinute, helper, out dataCacheKey);

                lock (lockObj)
                {
                    string key2;
                    a = TypeCache.GetModelKeyCache(type, db.DatabaseName, out key2);
                    if (!a)
                    {
                        TypeCache.SetModelKeyCache(type, db.DatabaseName, dataCacheKey);
                    }
                }
            }
            else
            {
                list = MemoryDataCache.CacheService.GetCacheItem<T>(dataCacheKey);
            }
            return list;
        }
        #endregion
    }
}
