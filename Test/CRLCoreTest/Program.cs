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
            var data = new Code.Sharding.MemberSharding();

            data.Code = "01";
            instance.SetLocation(data);
            var find1 = instance.QueryItem(b => b.Id > 0)?.Name;
            Console.WriteLine($"定位数据输入{data.Code},查询值为{find1}");

            data.Code = "02";
            instance.SetLocation(data);
            var find2 = instance.QueryItem(b => b.Id > 0)?.Name;
            Console.WriteLine($"定位数据输入{data.Code},查询值为{find2}");
            Console.ReadLine();
            goto label1;
        }
    }
}
