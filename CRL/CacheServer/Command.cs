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

namespace CRL.CacheServer
{
    public enum CommandType
    {
        查询,
        更新,
        获取配置
    }
    public class Command
    {
        /// <summary>
        /// 对象类型,FullName
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }
        public CommandType CommandType
        {
            get;
            set;
        }
        /// <summary>
        /// json data
        /// </summary>
        public string Data
        {
            get;
            set;
        }
        public static Command FromJson(string json)
        {
            var result = SerializeHelper.DeserializeFromJson<Command>(json);
            return result;
        }
    }
}
