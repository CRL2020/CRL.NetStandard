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
using System.Linq.Expressions;

namespace CRL.Attribute
{
    /// <summary>
    /// 表
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        /// <summary>
        /// 表名
        /// MongoDB不支持字段别名
        /// </summary>
        public string TableName
        {
            get;
            set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get;
            set;
        }
        /// <summary>
        /// 默认排序 如Id Desc
        /// </summary>
        public string DefaultSort
        {
            get;
            set;
        }
    }

    public class TableInnerAttribute : TableAttribute
    {
        public override string ToString()
        {
            return TableName;
        }

        /// <summary>
        /// 自增主键
        /// </summary>
        public FieldInnerAttribute PrimaryKey
        {
            get;
            set;
        }
        public FieldInnerAttribute GetPrimaryKey()
        {
            return PrimaryKey;
        }
        /// <summary>
        /// 对象类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 所有字段
        /// </summary>
        public List<FieldInnerAttribute> Fields
        {
            get;
            internal set;
        } = new List<FieldInnerAttribute>();
        /// <summary>
        /// 只存基本数据库字段
        /// </summary>
        public IgnoreCaseDictionary<FieldInnerAttribute> FieldsDic = new IgnoreCaseDictionary<FieldInnerAttribute>();
    }
}
