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
    /// 会员
    /// 重新定义了表名和字段名
    /// </summary>
    [CRL.Attribute.Table(TableName = "MM")]
    public class Member : CRL.IModel
    {
        [CRL.Attribute.Field(IsPrimaryKey = true, MapingName = "A")]
        public int Id
        {
            get; set;
        }
        [CRL.Attribute.Field(MapingName = "B")]
        public string Name
        {
            get; set;
        }
        [CRL.Attribute.Field(MapingName = "C")]
        public string Mobile { get; set; }
        [CRL.Attribute.Field(MapingName = "D")]
        public string AccountNo { get; set; }

        protected override System.Collections.IList GetInitData()
        {
            var list = new List<Member>();
            list.Add(new Member() { Name = "hubro" });
            list.Add(new Member() { Name = "hubro2" });
            return list;
        }
    }
}
