/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRL.LambdaQuery.CRLExpression
{
    /// <summary>
    /// CRLExpression节点
    /// </summary>
    public class CRLExpression
    {
        public CRLExpression()
        {
        }
        public override string ToString()
        {
            return StringHelper.SerializerToJson(this);
        }
        /// <summary>
        /// 左节点
        /// </summary>
        public CRLExpression Left
        {
            get;
            set;
        }
        public Type MemberType
        {
            get;
            set;
        }
        /// <summary>
        /// 右节点
        /// </summary>
        public CRLExpression Right
        {
            get;
            set;
        }
        /// <summary>
        /// 节点类型
        /// </summary>
        public CRLExpressionType Type
        {
            get;
            set;
        }
        /// <summary>
        /// 数据
        /// </summary>
        public object Data
        {
            get;
            set;
        }
        public string Data_
        {
            get;
            set;
        }
        internal string DataParamed;

        /// <summary>
        /// 左右操作类型
        /// </summary>
        public ExpressionType ExpType
        {
            get;
            set;
        }


        internal string SqlOut;

        internal bool IsConstantValue;
    }
}
