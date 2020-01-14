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
        public static LambdaQuery<T> CreateLambdaQuery<T>(DbContext _dbContext) where T : IModel, new()
        {
            var _DBType = _dbContext.DBHelper.CurrentDBType;
            if (_DBType != DBType.MongoDB)
            {
                return new RelationLambdaQuery<T>(_dbContext);
            }
            
            var type = CRL.Core.Extension.Extension.MakeGenericType("CRL.Mongo.MongoDBLambdaQuery", "CRL.Mongo", typeof(T));
            var query = System.Activator.CreateInstance(type, _dbContext) as LambdaQuery<T>;
            return query;
            //return new MongoDBLambdaQuery<T>(_dbContext);
        }
    }
}
