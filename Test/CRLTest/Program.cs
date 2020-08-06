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
using CRL.RedisProvider;
using CRL.Mongo;
using ProtoBuf;

namespace CRLTest
{
    #region obj
    [ProtoContract]
    class testClass
    {
        [ProtoMember(1)]
        public string name
        {
            get; set;
        }
        public string name2
        {
            get; set;
        }
        [ProtoMember(2)]
        public DateTime? time
        {
            get; set;
        }
        //public b b
        //{
        //    get; set;
        //}
        [ProtoMember(3)]
        public decimal price
        {
            get; set;
        }
        //public Dictionary<string, object> dic
        //{
        //    get; set;
        //}
        [ProtoMember(4)]
        public decimal price2
        {
            get; set;
        }
        [ProtoMember(5)]
        public decimal price3
        {
            get; set;
        }
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
            var builder = CRL.SettingConfigBuilder.CreateInstance();
            builder.UseMongoDB();

            var configBuilder = new CRL.Core.ConfigBuilder();
            configBuilder.UseRedis(t=>
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
            builder.RegisterDBAccessBuild(dbLocation=>
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


            //Code.MemberManage.Instance.QueryItem(1);
            //Code.OrderManage.Instance.QueryItem(1);
            //Code.ProductDataManage.Instance.QueryItem(1);
            string str = "111";
            var client = new CRL.RedisProvider.RedisClient(1,"test");

        label1:
            new ProductDataManage().TestBatchInsert();
            //var item = new Code.MongoDBTestManage().Sum(b => b.Id > 0, b => b.Numbrer);
            //new MongoUpdateTest().TestInsert();
            //Code.ContextTest.Test();
            //testHttpClient();
            //testFormat();
            Code.TestAll.TestMethod();
            //MongoDBTestManage.Instance.MongoQueryTest();
            //TestAll();
            //testCallContext("data3");
            //client.ContainsKey("sss");
            Console.WriteLine("ok");
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
            //obj.name2 = "sss";
            obj.price2 = 2;
            obj.price3 = 10;
            int count = 1;


            new CounterWatch().Start("testJson", () =>
            {
                testJson(obj);
            }, count);

            new CounterWatch().Start("testBinary", () =>
            {
                testBinary(obj);
            }, count);
            new CounterWatch().Start("testProtobuf", () =>
            {
                testProtobuf(obj);
            }, count);
        }
        static int testJson(testClass obj)
        {
            var json = SerializeHelper.SerializerToJson(obj);
            var len = Encoding.UTF8.GetBytes(json).Length;
            Console.WriteLine(len);
            //var obj2 = SerializeHelper.DeserializeFromJson<testClass>(json);
            return len;
        }
        static int testProtobuf(testClass obj)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, obj);
                //var obj2 = ProtoBuf.Serializer.Deserialize<testClass>(ms);
                Console.WriteLine(ms.Length);
                return (int)ms.Length;
            }
 
        }

        static int testBinary(testClass obj)
        {
            var data = CRL.Core.BinaryFormat.ClassFormat.Pack(obj.GetType(), obj);
            //var obj2 = CRL.Core.BinaryFormat.ClassFormat.UnPack(obj.GetType(), data);
            Console.WriteLine(data.Length);
            return data.Length;
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
        static void testCallContext(string v)
        {
            CallContext.SetData("testData", v);
            var value = CallContext.GetData<string>("testData");
            Console.WriteLine(value);
        }
        static void testHttpClient()
        {

            var urls = new List<string>() { "http://news.163.com", "http://www.baidu.com", "https://www.cnblogs.com" };
            foreach (var item in urls)
            {
                Task.Run(() =>
                {
                    var request = new CRL.Core.Request.ImitateWebRequest(item);
                    request.ContentType = "text/html";
                    var result = request.SendDataAsync(item, "GET", "").Result;
                    Console.WriteLine(result.Length);
                });
            }
        }
    }
}
