using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CRL.NetCore;
using CRL.DBAccess;
using System.Collections.Generic;
using CRL;
using CRL.Attribute;
using CRL.Sharding;

namespace CRLCoreTest
{
    class Program
    {
        static IServiceProvider provider;
        static Program()
        {
            var services = new ServiceCollection();
            services.AddCRL<DBLocationCreator>();
            services.AddScoped<Code.Sharding.MemberManage>();

            provider = services.BuildServiceProvider();
            provider.UseCRL();
        }

        static void Main(string[] args)
        {

        label1:
            var instance = provider.GetService<Code.Sharding.MemberManage>();
            var data = new Code.Sharding.MemberSharding() { Name = "test" };
            instance.SetLocation(data);
            var find1 = instance.QueryItem(b => b.Id > 0)?.Name;
            Console.WriteLine($"定位数据输入{data.Name},查询值为{find1}");

            data.Name = "db2";
            instance.SetLocation(data);
            var find2 = instance.QueryItem(b => b.Id > 0)?.Name;
            Console.WriteLine($"定位数据输入{data.Name},查询值为{find2}");
            Console.ReadLine();
            goto label1;
        }
    }

    public class DBLocationCreator : IDBLocationCreator
    {
        public DBLocationCreator(ISettingConfigBuilder settingConfigBuilder)
        {
            //自定义定位
            settingConfigBuilder.RegisterLocation<Code.Sharding.MemberSharding>((t, a) =>
            {
                var tableName = t.TableName;
                if (a.Name == "db2")
                {
                    //tableName = "MemberSharding1";
                    return new CRL.Sharding.Location("testdb2", tableName);
                }
                //返回定位库和表名
                return new CRL.Sharding.Location("testdb", tableName);
            });
            settingConfigBuilder.RegisterDBAccessBuild(dbLocation =>
            {
                //定位库
                if (dbLocation.ShardingLocation != null)
                {
                    //Console.WriteLine($"定位库为 {dbLocation.ShardingLocation.DataBaseName}");
                    return new CRL.DBAccessBuild(DBType.MSSQL, "Data Source=.;Initial Catalog=" + dbLocation.ShardingLocation.DataBaseName + ";User ID=sa;Password=123");
                }
                return new CRL.DBAccessBuild(DBType.MSSQL, "server=.;database=testDb; uid=sa;pwd=123;");
            });
        }
    }
}
