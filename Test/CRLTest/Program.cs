using CRL;
using CRL.Core;
using CRL.DBAccess;
using CRLTest.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using CRL.Core.RedisProvider;
using CRL.Mongo;
namespace CRLTest
{
    #region obj
    class testClass
    {
        public string name
        {
            get; set;
        }
        public DateTime? time
        {
            get; set;
        }
        //public b b
        //{
        //    get; set;
        //}
        public decimal price
        {
            get; set;
        }
        //public Dictionary<string, object> dic
        //{
        //    get; set;
        //}
    }
    public class b
    {
        public string name
        {
            get;set;
        }
        public string name2
        {
            get; set;
        }
    }
    public class MyGenericClass<T>
    {

    }
    #endregion
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new CRL.SettingConfigBuilder();
            builder.UseMongoDB();

            var configBuilder = new CRL.Core.ConfigBuilder();
            configBuilder.UseRedis("Server_204@127.0.0.1:6389")
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
            builder.RegisterDBAccessBuild(dbLocation=>
            {
                if (dbLocation.ManageName == "mongo")
                {
                    var conn = CRL.Core.CustomSetting.GetConfigKey("mongodb");
                    return new CRL.DBAccessBuild(DBType.MongoDB, conn);

                }
                return null;
            });
            builder.RegisterDBAccessBuild(dbLocation =>
            {
                //定位库
                if (dbLocation.ShardingLocation != null)
                {
                    return new CRL.DBAccessBuild(DBType.MSSQL, "Data Source=.;Initial Catalog=" + dbLocation.ShardingLocation.DataBaseName + ";User ID=sa;Password=123");
                }
                return new CRL.DBAccessBuild(DBType.MSSQL, "server=.;database=testDb; uid=sa;pwd=123;");
            });


            //Code.MemberManage.Instance.QueryItem(1);
            //Code.OrderManage.Instance.QueryItem(1);
            //Code.ProductDataManage.Instance.QueryItem(1);
            string str = "111";
            var client = new CRL.Core.RedisProvider.RedisClient(4);

        label1:
            //testFormat();
            //MongoDBTestManage.Instance.GroupTest();
            TestAll();
            //testCallContext("data3");
            Console.ReadLine();
            goto label1;
            Console.ReadLine();
        }
        static void testFormat()
        {

            var obj = new testClass() { };
            //obj.b = new b() { name = "b22424" };
            //obj.dic = new Dictionary<string, object>() { { "tes111111111112t", 1 }, { "t2222222222122est2", 1 }, { "te22222221s3t", 1 } };
            obj.name = "test2ConvertObject";
            obj.time = DateTime.Now;
            obj.price = 1002;
            int count = 1000;
            new CounterWatch().Start("json", () =>
            {
                testJson(obj);
            }, count);
            //var data = CRL.Core.BinaryFormat.ClassFormat.Pack(obj.GetType(), obj);
            new CounterWatch().Start("binary",() =>
            {
                //var obj2 = CRL.Core.BinaryFormat.ClassFormat.UnPack(obj.GetType(), data);
                testBinary(obj);
            }, count);

        }
        static int testJson(testClass obj)
        {
            var json = SerializeHelper.SerializerToJson(obj);
            var obj2 = SerializeHelper.DeserializeFromJson<testClass>(json);
            //var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            return 0;
        }
        static int testBinary(testClass obj)
        {
            var data = CRL.Core.BinaryFormat.ClassFormat.Pack(obj.GetType(), obj);
            var obj2 = CRL.Core.BinaryFormat.ClassFormat.UnPack(obj.GetType(), data);
            return 0;
        }
        static void testSharding()
        {
            var instance = new Code.Sharding.MemberManage();
            instance.SetLocation(new Code.Sharding.MemberSharding() { Name = "hubro" });
            var obj = instance.QueryItem(1);
            Console.WriteLine(obj?.Name);
            instance.SetLocation(new Code.Sharding.MemberSharding() { Name = "hubro2" });
            var obj2 = instance.QueryItem(1);
            Console.WriteLine(obj2?.Name);
        }
        static void TestAll()
        {
            var array = typeof(Code.TestAll).GetMethods(BindingFlags.Static | BindingFlags.Public).OrderBy(b => b.Name.Length);
            var instance = new Code.TestAll();
            foreach (var item in array)
            {
                if (item.Name == "TestUpdate")
                {
                    continue;
                }
                try
                {
                    item.Invoke(instance, null);
                    Console.WriteLine($"{item.Name} ok");
                }
                catch(Exception ero)
                {
                    Console.WriteLine($"{item.Name} error {ero.Message}");
                }
 
            }
        }
        static void MakeGenericTypeTest()
        {
            var type = CRL.Core.Extension.Extension.MakeGenericType("CRL.LambdaQuery.Mapping.QueryInfo", "CRL", typeof(ProductData));
        }

        static void requestTest()
        {
            new CRL.Core.ThreadWork().Start("11", () =>
            {
                try
                {
                    var result = CRL.Core.Request.HttpRequest.HttpGet("http://localhost:8002");
                    Console.WriteLine(result);
                }
                catch (Exception ero)
                {
                    Console.WriteLine(ero.Message);
                }
                return true;
            }, 0.3);
        }
        static void testCallContext(string v)
        {
            CallContext.SetData("testData", v);
            var value = CallContext.GetData<string>("testData");
            Console.WriteLine(value);
        }
    }
}
