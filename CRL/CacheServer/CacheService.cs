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
    /// <summary>
    /// 查询服务
    /// </summary>
    internal class CacheService
    {
        /// <summary>
        /// 按json格式查询
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string Deal(string command)
        {
            var commandObj = Command.FromJson(command);
            //var obj = LambdaQuery.CRLQueryExpression.FromJson(commandObj.Data);
            return Deal(commandObj);
        }
        /// <summary>
        /// 按CRLExpression 查询
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static string Deal(CacheServer.Command command)
        {
            if (command.CommandType == CommandType.获取配置)
            {
                return StringHelper.SerializerToJson(CacheServerSetting.ServerTypeSetting);
            }
            var type = command.ObjectType;
            if (!CacheServerSetting.CacheServerDealDataRules.ContainsKey(type))
            {
                return "error,服务端未找到SettingConfig.CacheServerDealDataRules对应的处理:" + type;
            }
            var handler = CacheServerSetting.CacheServerDealDataRules[type];
            var data = handler(command);
            return StringHelper.SerializerToJson(data);
        }
    }
}
