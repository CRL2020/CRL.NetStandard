/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRL.Core
{
    /// <summary>
    /// 自定义CallContext
    /// </summary>
    public class CallContext
    {
#if NETSTANDARD
        static ConcurrentDictionary<string, AsyncLocal<object>> localDatas = new ConcurrentDictionary<string, AsyncLocal<object>>();
        public static T GetData<T>(string contextName)
        {
            var a = localDatas.TryGetValue(contextName, out AsyncLocal<object> v);
            if (a)
            {
                if (v.Value == null)
                {
                    return default(T);
                }
                return (T)v.Value;
            }
            return default(T);
        }
        public static void SetData(string contextName, object data)
        {
            localDatas.TryRemove(contextName, out AsyncLocal<object> v);
            localDatas.TryAdd(contextName, new AsyncLocal<object>() { Value = data });
        }
#else

        public static T GetData<T>(string contextName)
        {
            //return default(T);
            var dbContextObj = System.Runtime.Remoting.Messaging.CallContext.GetData(contextName);
            if (dbContextObj == null)
                return default(T);
            return (T)dbContextObj;
        }
        public static void SetData(string contextName, object data)
        {
            //return;
            System.Runtime.Remoting.Messaging.CallContext.SetData(contextName, data);
        }
#endif
    }
}
