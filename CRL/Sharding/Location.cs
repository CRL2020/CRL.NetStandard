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

namespace CRL.Sharding
{
    /// <summary>
    /// 数据定位
    /// </summary>
    public class Location
    {
        public Location(string dbName,string tablePartName)
        {
            DataBaseName = dbName;
            TablePartName = tablePartName;
        }
        /// <summary>
        /// 库名
        /// </summary>
        public string DataBaseName;
        /// <summary>
        /// 主表名
        /// </summary>
        public string TableName;
        /// <summary>
        /// 当前分表名
        /// </summary>
        public string TablePartName;
        /// <summary>
        /// 所有分表
        /// </summary>
        public List<string> AllTablePartName = new List<string>();
        public override string ToString()
        {
            return string.Format("在库[{0}],表[{1}]", DataBaseName, TablePartName);
        }
    }
}
