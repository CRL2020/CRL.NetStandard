/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRLTest.Code
{

    /// <summary>
    /// OrderManage
    /// </summary>
    public class OrderManage : CRL.BaseProvider<Order>
    {
        public static OrderManage Instance
        {
            get { return new OrderManage(); }
        }
        public void TestRelationUpdate()
        {
            var query = GetLambdaQuery();
            query.Join<ProductData>((a, b) => a.Id == b.Id && b.Number > 10);
            var c = new CRL.ParameCollection();
            c["UserId"] = "$UserId";//order.userid=product.userid
            c["Remark"] = "2222";//order.remark=2222
            Update(query, c);
        }
        public bool TransactionTest2(out string error)
        {
            
            //简化了事务写法,自动提交回滚
            return PackageTrans((out string ex) =>
            {
                ex = "";
                var product = new ProductData();
                product.BarCode = "code" + DateTime.Now.Millisecond;
                product.Number = 10;
                var list = new List<ProductData>();
                list.Add(product);
                ProductDataManage.Instance.Add(list);
                return false; //会回滚
            }, out error);
        }
        public bool TransactionTest3(out string error)
        {

            //简化了事务写法,自动提交回滚
            return PackageTrans((out string ex) =>
            {
                ex = "";
                var product = new ProductData();
                product.BarCode = "code" + DateTime.Now.Millisecond;
                product.Number = 10;
                ProductDataManage.Instance.Add(product);
                var a = TransactionTest2(out ex);//嵌套事务
                return a; //会回滚
            }, out error);
        }
        public bool TransactionTest(out string error)
        {
            //简化了事务写法,自动提交回滚
            return PackageTrans((out string ex) =>
            {
                ex = "";
                var item = QueryItem(1);
                var product = new ProductData();
                product.BarCode = "code" + DateTime.Now.Millisecond;
                product.Number = 10;
                ProductDataManage.Instance.Add(product);
                return false; //会回滚
            }, out error);
        }

        public bool TransactionTest4(out string error)
        {
            //简化了事务写法,自动提交回滚
            return PackageTrans((out string ex) =>
            {
                ex = "";
                var item = QueryItem(1);
                var product = new ProductData();
                product.BarCode = "code" + DateTime.Now.Millisecond;
                product.Number = 10;
                ProductDataManage.Instance.Add(product);
                Add(item);
                return true; //不会回滚
            }, out error);
        }

    }
}
