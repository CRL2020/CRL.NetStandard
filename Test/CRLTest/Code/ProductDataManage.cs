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
using CRL;
namespace CRLTest.Code
{
    /// <summary>
    /// ProductData业务处理类
    /// 这里实现处理逻辑
    /// </summary>
    public class ProductDataManage : CRL.BaseProvider<ProductData>
    {
        public override string ManageName
        {
            get
            {
                return "test2";
            }
        }
        protected override CRL.LambdaQuery.ILambdaQuery<ProductData> CacheQuery()
        {
            return GetLambdaQuery().Where(b => b.Id < 1000).Expire(5);
        }
        /// <summary>
        /// 对象被更新时,是否通知缓存服务器
        /// </summary>
        protected override bool OnUpdateNotifyCacheServer
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 是否从远程查询缓存
        /// </summary>
        protected override bool QueryCacheFromRemote
        {
            get
            {
                return false;
            }
        }

       
        /// <summary>
        /// 实例访问入口
        /// </summary>
        public static ProductDataManage Instance
        {
            get { return  new ProductDataManage(); }
        }
        public void TransactionTest5()
        {
            var db = GetDBExtend();
            var product = new ProductData();
            product.BarCode = "code" + DateTime.Now.Millisecond;
            product.Number = 10;
            var order = new Order() { OrderId="222"  };
            db.BeginTran();
            db.InsertFromObj(order);
            db.BatchInsert(new List<ProductData>() { product, product });
            db.CommitTran();
        }

        public void TestBatchInsert()
        {
            Delete(b => b.Id > 0);
            var p = new ProductData() { Id = 1999, BarCode="sss", Number=12 };
            var list = new List<ProductData>() { p };
            BatchInsert(list, true);
        }

    }
}
