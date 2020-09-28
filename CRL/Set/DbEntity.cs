/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.LambdaQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Set
{
    /// <summary>
    /// 对象关联
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbEntity<T> where T : class, new()
    {
        Expression<Func<T, bool>> _relationExp;
        DbContextInner _dbContext;
        internal DbEntity(DbContextInner dbContext, Expression<Func<T, bool>> relationExp)
        {
            _dbContext = dbContext;
            _relationExp = relationExp;
        }
        AbsDBExtend getAbsDBExtend()
        {
            if (_dbContext == null)
            {
                throw new Exception("_dbContext为空");
            }
            var db = DBExtendFactory.CreateDBExtend(_dbContext);
            return db;
        }
        /// <summary>
        /// 获取当前值
        /// </summary>
        /// <returns></returns>
        public T GetValue()
        {
            var db = getAbsDBExtend();
            var item = db.QueryItem(_relationExp);
            return item;
        }
    }
}
