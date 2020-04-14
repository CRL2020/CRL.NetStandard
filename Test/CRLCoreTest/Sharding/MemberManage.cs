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

namespace CRLCoreTest.Code.Sharding
{
    public class MemberSharding : CRL.IModel
    {
        [CRL.Attribute.Field(KeepIdentity=true)]//保持插入主键
        public int Id
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string Code;
    }
    public class MemberManage : CRL.Sharding.BaseProvider<MemberSharding>
    {

    }

}
