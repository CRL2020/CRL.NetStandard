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
    public class MemberManage : CRL.BaseProvider<Member>
    {
        public override string ManageName
        {
            get
            {
                return "test";
            }
        }
        /// <summary>
        /// 实例访问入口
        /// </summary>
        public static MemberManage Instance
        {
            get { return new MemberManage(); }
        }
        public void UpdateList()
        {
            var list = GetLambdaQuery().Take(3).ToList();
            list.ForEach(b =>
            {
                b.Name = DateTime.Now.ToString();
            });
            Update(list);
        }
    }
}
