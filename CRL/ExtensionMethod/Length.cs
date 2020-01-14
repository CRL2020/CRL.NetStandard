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
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
using CRL.LambdaQuery;
namespace CRL
{
    public static partial class ExtensionMethod
    {
        /// <summary>
        /// 表示len此字段
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static int Len(this object origin)
        {
            if (origin == null)
            {
                return 0;
            }
            return origin.ToString().Length;
        }
    }
}
