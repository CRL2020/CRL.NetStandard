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
        public void  GetInitData()
        {
            var list = new List<MongoDBModel2>();
            list.Add(new MongoDBModel2() { name = "test1", Numbrer = 1, OrderId="11" });
            list.Add(new MongoDBModel2() { name = "test2", Numbrer = 2, OrderId = "12" });
            list.Add(new MongoDBModel2() { name = "test3", Numbrer = 3, OrderId = "13" });
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
            //GetInitData();
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
    }
}
