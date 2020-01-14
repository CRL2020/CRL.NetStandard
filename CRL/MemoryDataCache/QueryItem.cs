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

namespace CRL.MemoryDataCache
{
    /// <summary>
    /// 查询的项
    /// </summary>
    public class QueryItem
    {
        public string TableName
        {
            get;
            set;
        }
        public Type DataType
        {
            get;
            set;
        }
        public string Key
        {
            get;
            set;
        }
        public string Params
        {
            get;
            set;
        }
        public int TimeOut
        {
            get;
            set;
        }
        public DateTime UpdateTime
        {
            get;
            set;
        }
        public int RowCount
        {
            get;
            set;
        }
        public string DatabaseName
        {
            get;
            set;
        }
    }
}
