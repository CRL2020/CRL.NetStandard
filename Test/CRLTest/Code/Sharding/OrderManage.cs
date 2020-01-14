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
using CRL.Sharding;

namespace CRLTest.Code.Sharding
{
    public class OrderSharding : CRL.IModel
    {
        public int MemberId
        {
            get;
            set;
        }
        public string Remark
        {
            get;
            set;
        }

    }
    public class OrderManage : CRL.Sharding.BaseProvider<OrderSharding>
    {
        public static OrderManage Instance
        {
            get { return new OrderManage(); }
        }
    }
}
