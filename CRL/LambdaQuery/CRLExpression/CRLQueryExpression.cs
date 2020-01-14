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
using System.Text;
using System.Threading.Tasks;

namespace CRL.LambdaQuery.CRLExpression
{
    public class CRLQueryExpression
    {
        /// <summary>
        /// 对象类型,FullName
        /// </summary>
        public string Type
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public CRLExpression Exp
        {
            get;
            set;
        }
        /// <summary>
        /// 分页用,每页大小
        /// </summary>
        public int Size
        {
            get;
            set;
        }
        /// <summary>
        /// 分页用,页索引
        /// </summary>
        public int Page
        {
            get;
            set;
        }
        public string ToJson()
        {
            return StringHelper.SerializerToJson(this);
        }
        /// <summary>
        /// CRLQueryExpression json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static CRLQueryExpression FromJson(string json)
        {
            var result = SerializeHelper.DeserializeFromJson<CRLQueryExpression>(json);
            return result;
        }
    }
    
}
