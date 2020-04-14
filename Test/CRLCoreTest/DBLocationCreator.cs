using System;
using System.Collections.Generic;
using System.Text;
using CRL;
using CRL.DBAccess;
using CRL.NetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace CRLCoreTest
{
    public class DBLocationCreator : IDBLocationCreator
    {
        ISettingConfigBuilder _settingConfigBuilder;
        public DBLocationCreator(ISettingConfigBuilder settingConfigBuilder)
        {
            _settingConfigBuilder = settingConfigBuilder;
        }

        public void Init()
        {
            //自定义定位
            _settingConfigBuilder.RegisterLocation<Code.Sharding.MemberSharding>((t, a) =>
            {
                var tableName = t.TableName;
                var dbName = a.Code == "02" ? "testdb2" : "testdb";
                var dataBase = $"Data Source=.;Initial Catalog={dbName};User ID=sa;Password=123";
                //返回定位库和表名
                return new CRL.Sharding.Location(dataBase, tableName);
            });
            _settingConfigBuilder.RegisterDBAccessBuild(dbLocation =>
            {
                var connectionString = "Data Source=.;Initial Catalog=testdb;User ID=sa;Password=123";
                if (dbLocation.ShardingLocation != null)
                {
                    connectionString = dbLocation.ShardingLocation.DataBaseSource;
                }
                return new CRL.DBAccessBuild(DBType.MSSQL, connectionString);
            });
        }
    }
}
