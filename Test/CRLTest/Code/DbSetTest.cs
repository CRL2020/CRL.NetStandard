using CRL;
using CRL.Set;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CRL.Core.Extension;
namespace CRLTest.Code
{

    public class OrderContext: DbSetContext
    {
        public override string ManageName => "OrderContext";
        public DbSet<Order> Orders
        {
            get
            {
                return GetDbSet<Order>();
            }
        }
        public DbSet<ProductData> Products
        {
            get
            {
                return GetDbSet<ProductData>();
            }
        }
    }
    public class ContextTest
    {
        public static void Test()
        {
            var orderContext = new OrderContext();
            //orderContext.Orders.Add(new Order() { OrderId="123", ProductId = 2, UserId = 2 });
            //orderContext.SaveChanges();
            var firstOrder = orderContext.Orders.Find(b => b.OrderId == "123");
            //var products = firstOrder.Products.ToList();
            Console.WriteLine($"products {firstOrder.Products.Count()}");
            var i = 101;
            firstOrder.Products.Add(new ProductData() { InterFaceUser = "2222", ProductName = "product" + i, BarCode = "code" + i, UserId = 1, Number = i, OrderId = "123" });
            orderContext.SaveChanges();
            Console.WriteLine($"products {firstOrder.Products.Count()}");

            var m = firstOrder.Member;
            Console.WriteLine(m.GetValue().ToJson());
        }
    }
}
