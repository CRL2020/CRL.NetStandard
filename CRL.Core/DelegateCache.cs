using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core
{
    public delegate T GetDataHandler<T>();
    class CacheObj
    {
        public object data;
        public DateTime expTime;
    }
    /// <summary>
    /// 自定义委托和过期时间,实现缓存
    /// </summary>
    public class DelegateCache
    {
        static object lockObj = new object();
        static System.Collections.Concurrent.ConcurrentDictionary<string, CacheObj> caches = new System.Collections.Concurrent.ConcurrentDictionary<string, CacheObj>();
        /// <summary>
        /// 初始缓存信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="minute">过期时间,单位分</param>
        /// <param name="handler">委托</param>
        public static T Init<T>(string key, double minute, GetDataHandler<T> handler)
        {
            var a = caches.TryGetValue(key,out CacheObj data);
            var doHandler = false;
            if(!a)
            {
                doHandler = true;
            }
            if (a && data.expTime < DateTime.Now)
            {
                doHandler = true;
            }
            if(!doHandler)
            {
                return (T)data.data;
            }
            var obj = handler();
            data = new CacheObj() { data=obj, expTime=DateTime.Now.AddMinutes(minute) };
            caches.TryRemove(key,out CacheObj obj2);
            caches.TryAdd(key, data);
            return obj;
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            caches.TryRemove(key, out CacheObj obj2);
        }
    }
}
