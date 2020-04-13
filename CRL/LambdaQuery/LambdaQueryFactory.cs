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
        public static LambdaQuery<T> CreateLambdaQuery<T>(DbContext _dbContext) where T : IModel, new()
        {
            var configBuilder = SettingConfigBuilder.current;
            var _DBType = _dbContext.DBHelper.CurrentDBType;
            if (_DBType != DBType.MongoDB)
            {
                return new RelationLambdaQuery<T>(_dbContext);
            }
            var type = typeof(T);
            var a = mongoQueryCreaters.TryGetValue(type, out object creater);
            if (!a)
            {
                a = configBuilder.LambdaQueryTypeRegister.TryGetValue(DBType.MongoDB, out Type type2);
                if (!a)
                {
                    throw new CRLException("未引用CRL.MongoDB");
                }
                var genericType = type2.MakeGenericType(typeof(T));
                creater = Core.DynamicMethodHelper.CreateCtorFunc<Func<DbContext, LambdaQuery<T>>>(genericType, new Type[] { typeof(DbContext) });
                mongoQueryCreaters.TryAdd(type, creater);
            }
            var func = (Func<DbContext, LambdaQuery<T>>)creater;
            return func(_dbContext);
        }
    }
}
