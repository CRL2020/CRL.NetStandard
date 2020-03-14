using CRL;
using CRL.Set;
using System;
using System.Collections.Generic;
using System.Text;

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
            //所有
            var product = orderContext.Products.ToList();

            //返回关联过的查询,使用完整查询满足更多需求
            var product2 = orderContext.Products.GetQuery();

            var p = new ProductData() { BarCode = "33333" };
            //添加一项
            orderContext.Products.Add(p);

            orderContext.Products.Remove(p);//删除一项
            //返回完整的BaseProvider
            var provider = orderContext.Products.GetProvider();

            orderContext.SaveChanges();//保存所有更改

            //自动关联
            var order = new Order();
            var products = order.Products.ToList();
        }
    }
}
