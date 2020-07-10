using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL;
namespace CRLTest.Code
{
    [CRL.Attribute.Table(TableName = "MongoDBModel3")]
    public class MongoDBModel2 : CRL.IModelBase
    {
       
        public string OrderId
        {
            get;
            set;
        }
        public int Numbrer
        {
            get;
            set;
        }
        public string name
        {
            get; set;
        }
        public int Status
        {
            get;set;
        }
        public double Price
        {
            get;set;
        }
    }
    public class MongoResult
    {
        public int sum
        {
            get; set;
        }
        public int count
        {
            get; set;
        }
        public string name
        {
            get; set;
        }
        public string orderId
        {
            get; set;
        }
    }
    public class MongoDBTestManage : CRL.BaseProvider<MongoDBModel2>
    {
        public override string ManageName => "mongo";
        public static MongoDBTestManage Instance
        {
            get { return new MongoDBTestManage(); }
        }
        public void GetInitData()
        {

            var list = new List<MongoDBModel2>();
            list.Add(new MongoDBModel2() { name = "test", Numbrer = 1, Price = 1.125, OrderId = "13" });
            for (int i = 0; i < 10; i++)
            {
                var n = i + 0.23 * i;
                list.Add(new MongoDBModel2() { name = "test" + i, Numbrer = i, Price = n, OrderId = "13" });
            }
            BatchInsert(list);
        }
        public void Test()
        {
            var query = GetLambdaQuery();
            query.Where(b => !string.IsNullOrEmpty(b.name));
            query.Select(b => new { b.Numbrer, b.name });
            var result2 = query.ToDynamic();
        }
        public void GroupTest(int page = 1)
        {
            //Delete(b=>b.Numbrer>0);
            GetInitData();
            var query = GetLambdaQuery();
            query.Page(4, page);
            var result = query.GroupBy(b => new { b.name, b.OrderId }).Select(b => new
            {
                num = b.Numbrer.SUM(),
                b.OrderId,
                b.name
            }).HavingCount(b => b.num > 1).ToList();
            foreach (var item in result)
            {
                Console.WriteLine($"{item.OrderId} {item.num} {item.name}");
            }
            //var result = query.ToList<MongoResult>();
        }
        public void GroupTest2(int page = 1)
        {
            //Delete(b=>b.Numbrer>0);
            GetInitData();
            var query = GetLambdaQuery();
            var result = query.GroupBy(b => new { b.name }).Select(b => new
            {
                num = b.Numbrer.SUM(),
                num2 = b.SUM(x => x.Numbrer * x.Price),
                b.name
            }).ToList();
            var sql = query.PrintQuery();//输出bson
            foreach (var item in result)
            {
                Console.WriteLine($"{item.num2} {item.name}");
            }
          
        }
        public void SumTest()
        {
            var count = Count(b => b.Numbrer >= 0);
            if (count == 0)
            {
                GetInitData();
            }
            var query = GetLambdaQuery();
            var result = query.GroupBy(b => new {  b.OrderId }).Select(b => new
            {
                sum = b.Price.SUM(),
                b.OrderId,
            }).ToList();
            foreach (var item in result)
            {
                Console.WriteLine($"{item.OrderId} {item.OrderId} {item.sum}");
            }

        }
    }
}
