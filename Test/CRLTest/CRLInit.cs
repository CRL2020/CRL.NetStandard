using CRL.DBAccess;
using CRL.Mongo;
using System;
using System.Collections.Generic;
using System.Text;
using CRL.RedisProvider;
namespace CRLTest
{
    class CRLInit
    {
        public static void Init()
        {
            var builder = CRL.DBConfigRegister.CreateInstance();
            builder.UseMongoDB();

            var configBuilder = new CRL.Core.ConfigBuilder();
            configBuilder.UseRedis(t =>
            {
                return "Server_204@127.0.0.1:6389";
            })
                .UseRedisSession();
            //自定义定位
            builder.RegisterLocation<Code.Sharding.MemberSharding>((t, a) =>
            {
                var tableName = t.TableName;
                if (a.Name == "hubro")
                {
                    tableName = "MemberSharding1";
                    return new CRL.Sharding.Location("testdb2", tableName);
                }
                //返回定位库和表名
                return new CRL.Sharding.Location("testdb", tableName);
            });
            builder.RegisterDBAccessBuild(dbLocation =>
            {
                if (dbLocation.ManageName == "mongo")
                {
                    return new CRL.DBAccessBuild(DBType.MongoDB, "mongodb://127.0.0.1:27017/admin");
                }
                return null;
            });
            builder.RegisterDBAccessBuild(dbLocation =>
            {
                //定位库
                if (dbLocation.ShardingLocation != null)
                {
                    return new CRL.DBAccessBuild(DBType.MSSQL, "Data Source=.;Initial Catalog=" + dbLocation.ShardingLocation.DataBaseSource + ";User ID=sa;Password=123");
                }
                return new CRL.DBAccessBuild(DBType.MSSQL, "server=.;database=testDb; uid=sa;pwd=123;");
            });
        }
    }
}
