/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.LambdaQuery
{
    public class LambdaQueryFactory
    {
        static System.Collections.Concurrent.ConcurrentDictionary<Type, object> mongoQueryCreaters = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();
        public static LambdaQuery<T> CreateLambdaQuery<T>(DbContextInner _dbContext) where T : class
        {
            //var configBuilder = DBConfigRegister.current;
            var _DBType = _dbContext.DBHelper.CurrentDBType;
            if (_DBType != DBType.MongoDB)
            {
                return new RelationLambdaQuery<T>(_dbContext);
            }
            var type = typeof(T);
            var a = mongoQueryCreaters.TryGetValue(type, out object creater);
            if (!a)
            {
                var typeMongo = DBConfigRegister.GetLambdaQueryType(DBType.MongoDB);
                var genericType = typeMongo.MakeGenericType(typeof(T));
                creater = Core.DynamicMethodHelper.CreateCtorFunc<Func<DbContextInner, LambdaQuery<T>>>(genericType, new Type[] { typeof(DbContextInner) });
                mongoQueryCreaters.TryAdd(type, creater);
            }
            var func = (Func<DbContextInner, LambdaQuery<T>>)creater;
            return func(_dbContext);
        }
    }
}
