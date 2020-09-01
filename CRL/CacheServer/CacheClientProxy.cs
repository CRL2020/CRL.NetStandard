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

namespace CRL.CacheServer
{
    /// <summary>
    /// 客户端代理
    /// </summary>
    internal abstract class CacheClientProxy
    {
        /// <summary>
        /// 服务器接口地址
        /// </summary>
        public abstract string Host { get; }
        /// <summary>
        /// 查询方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="total"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        internal List<T> Query<T>(Expression<Func<T, bool>> expression,out int total, int pageIndex = 0, int pageSize = 0) where T : class, new()
        {
            var query = new CRL.LambdaQuery.CRLExpression.CRLExpressionVisitor<T>();
            var json = query.Where(expression, pageIndex, pageSize);
            var command = new Command() { CommandType = CommandType.查询, Data = json, ObjectType = typeof(T).FullName };
            json = StringHelper.SerializerToJson(command);
            var result = SendQuery(json);
            if (result.StartsWith("error"))
            {
                throw new Exception(result);
            }
            var resultData = SerializeHelper.DeserializeFromJson<ResultData>(result);
            total = resultData.Total;
            var result2 = SerializeHelper.DeserializeFromJson<List<T>>(resultData.JsonData);
            return result2;
        }
        /// <summary>
        /// 调用接口方法
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public abstract string SendQuery(string query);
        public abstract void Dispose();
        /// <summary>
        /// 更新缓存方法,服务端需实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        internal void Update<T>(T obj)
        {
            var json = StringHelper.SerializerToJson(obj);
            var command = new Command() { CommandType = CommandType.更新, Data = json, ObjectType = typeof(T).FullName };
            json = StringHelper.SerializerToJson(command);
            try
            {
                var result = SendQuery(json);
                if (result.StartsWith("error"))
                {
                    throw new Exception(result);
                }
            }
            catch(Exception ero)
            {
                EventLog.Log("CacheClientProxy", ero.Message);
            }
        }

        static object lockObj = new object();
        internal void GetServerTypeSetting()
        {
            var command = new Command() { CommandType = CommandType.获取配置};
            var json = StringHelper.SerializerToJson(command);
            try
            {
                var result = SendQuery(json);
                if (result.StartsWith("error"))
                {
                    throw new Exception(result);
                }
                var setting = SerializeHelper.DeserializeFromJson<List<string>>(result);
                lock (lockObj)
                {
                    foreach (var s in setting)
                    {
                        if (!CacheServerSetting.ServerTypeSettings.ContainsKey(s))
                        {
                            CacheServerSetting.ServerTypeSettings.Add(s, this);
                        }
                    }
                }
            }
            catch (Exception ero)
            {
                throw new Exception(string.Format("分布式缓存:获取服务器{0}设置时发生错误:{1}", Host, ero.Message));
            }
        }
    }
}
