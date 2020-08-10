using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRL;
using MongoDB.Bson;

namespace CRLTest.Code
{
    public class SearchHistory:CRL.IModel
    {
        //[CRL.Attribute.Field(IsPrimaryKey = true, KeepIdentity = true)]
        //public ObjectId _id
        //{
        //    get; set;
        //}
        public string OrgId { get; set; }
        public string CustomerId { get; set; }
        public string UserId { get; set; }
        public string KeyWord { get; set; }
        public DateTime Time { get; set; }
        public string IndexKey { get; set; }
        public decimal Hit
        {
            get;set;
        }
        public string GetIndexKey()
        {
            var key = string.Format("{0}_{1}_{2}", CustomerId, KeyWord, Time.Date);
            return key;
        }
    }

    public class MongoUpdateTest: CRL.BaseProvider<SearchHistory>
    {
        public override string ManageName => "mongo";
        public void testUpdate(int take = 50)
        {
            //take = 10000;
            var code = "267fb0be096240608adf284303bcc90d";
            var list = GetLambdaQuery().Where(b => b.OrgId == code).Take(take).ToList();
            Console.WriteLine($"行数为{list.Count}");
            foreach (var item in list)
            {
                var state = item.KeyWord;
                //item._id = Guid.NewGuid().ToString();
                item.CustomerId = Guid.NewGuid().ToString();
                var n = Update(b => b.CustomerId == item.CustomerId, new Dictionary<string, object>() { { "KeyWord", state } });
            }
            if(list.Count==0)
            {
                list.Add(new SearchHistory() { CustomerId = Guid.NewGuid().ToString(), KeyWord = "key2", OrgId = code, Time = DateTime.Now });
            }
            //BatchInsert(list);
        }
        public void testUpdate2(int take = 50)
        {
            //return;
            //take = 1000;
            var code = "267fb0be096240608adf284303bcc90d";
            var list = GetLambdaQuery().Where(b => b.OrgId == code).Take(take).ToList();
            var ids = list.Select(b => b.CustomerId).ToArray();

            //var d = Delete(b =>b.CustomerId.In(ids));
            var d = Delete(b => ids.Contains(b.CustomerId));
            Console.WriteLine($"删除行数为{d}");
            foreach (var item in list)
            {
                var state = item.KeyWord;
                //item._id = Guid.NewGuid().ToString();
                item.CustomerId = Guid.NewGuid().ToString();
                
            }
            BatchInsert(list);
        }
        public void TestInsert()
        {
            var list = new List<SearchHistory>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(new SearchHistory() { CustomerId = Guid.NewGuid().ToString(), KeyWord = "key2", OrgId = i.ToString(), Time = DateTime.Now });
            }
            //int total = list.Count;
            //int index = 0;
            //while (index < total)
            //{
            //    var list2 = list.Skip(index).Take(12);
            //    index += list2.Count();
            //    Console.WriteLine(index);
            //}
            Console.WriteLine("ok");
            BatchInsert(list, true);
        }
        public void testCount()
        {
            var n=Count(b=>b.CustomerId=="sss");
        }
    }
}
