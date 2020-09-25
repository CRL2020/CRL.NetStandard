using CRL.EFCore.Extensions;
using System;
namespace EFTest
{
    class Program
    {
        static void Main(string[] args)
        {
        label1:

            test();
            //testCreateTable();
            Console.ReadLine();
            goto label1;
        }

        static void test()
        {
            using (var context = new Context1())
            {
                var set1 = context.Set1;
                context.CreateTable<TestClass>();
                var query = context.GetLambdaQuery<TestClass>();
                query.Join<TestClass2>((a, b) => a.Id == b.Id);
                query.PrintQuery();

                var db = context.GetDBExtend();
                db.InsertFromObj(new TestClass() { Id = DateTime.Now.Second, Name = "ddddd" });
                var n = db.Delete<TestClass>(b => b.Id > 0);
                Console.WriteLine(n);
            }
        }
    }
}
