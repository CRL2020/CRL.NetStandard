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

namespace CRL
{
    public class DBExtendFactory
    {
        public static AbsDBExtend CreateDBExtend(DbContext _dbContext)
        {
            var configBuilder = SettingConfigBuilder.current;
            var dbType = _dbContext.DBHelper.CurrentDBType;
            if (_dbContext.DataBaseArchitecture == DataBaseArchitecture.Relation)
            {
                return new DBExtend.RelationDB.DBExtend(_dbContext);
            }
            var a = configBuilder.AbsDBExtendRegister.TryGetValue(dbType, out Func<DbContext, AbsDBExtend> func);
            if (!a)
            {
                throw new CRLException($"未找到AbsDBExtend {dbType}");
            }
            return func(_dbContext);
        }
    }
}
