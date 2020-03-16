/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.Set;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRLTest.Code
{
    /// <summary>
    /// 订单
    /// </summary>
    [CRL.Attribute.Table(TableName = "OrderProduct")]//重新指定对应的表名
    public class Order : CRL.IModelBase
    {
        protected override System.Collections.IList GetInitData()
        {
            var list = new List<Order>();
            list.Add(new Order() { UserId = 1, OrderId = "123", ProductId = 1 });
            list.Add(new Order() { UserId = 2, OrderId = "456", ProductId = 1 });
            return list;
        }
        #region 关联
        public DbEntities<ProductData> Products
        {
            get
            {
                return IncludeMany<Order, ProductData>((a, b) => a.OrderId == b.OrderId);
            }
        }
        public DbEntity<Member> Member
        {
            get
            {
                return IncludeOne<Order, Member>((a, b) => a.UserId == b.Id);
            }
        }
        #endregion

        public int Status
        {
            get;
            set;
        }
        public int ProductId
        {
            get;set;
        }
        public string OrderId
        {
            get;
            set;
        }
        public string Remark
        {
            get;
            set;
        }
        public int UserId
        {
            get;
            set;
        }
        //新增Channel属性
        public string Channel
        {
            get;
            set;
        }
        
    }
}
