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
using System.Threading;
using System.Threading.Tasks;

namespace CRL.Core
{
    ///// <summary>
    ///// 当前调用上下文
    ///// </summary>
    //public class CallContext
    //{
    //    //rem 不能使用LogicalGetData,会造自定义线程只有一个实例
    //    public static T GetData<T>(string contextName)
    //    {
    //        //return default(T);
    //        var dbContextObj = System.Runtime.Remoting.Messaging.CallContext.GetData(contextName);
    //        if (dbContextObj == null)
    //            return default(T);
    //        return (T)dbContextObj;
    //    }
    //    public static void SetData(string contextName, object data)
    //    {
    //        //return;
    //        System.Runtime.Remoting.Messaging.CallContext.SetData(contextName, data);
    //    }
    //}
    /// <summary>
    /// 自定义CallContext
    /// </summary>
    public class CallContext
    {
        static ThreadLocal<Dictionary<string, object>> caches = new ThreadLocal<Dictionary<string, object>>(() => new Dictionary<string, object>());
        public static T GetData<T>(string contextName)
        {
            var a = caches.Value.TryGetValue(contextName, out object v);
            if (a)
            {
                return (T)v;
            }
            return default(T);
        }
        public static void SetData(string contextName, object data)
        {
            caches.Value.Remove(contextName);
            caches.Value.Add(contextName, data);
        }
        class callContextData
        {
            public string key;
            public object data;
            public DateTime time;
        }
    }
}
