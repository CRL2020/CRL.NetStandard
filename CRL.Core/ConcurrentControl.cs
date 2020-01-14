using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace CRL.Core
{
    /// <summary>
    /// 内存锁控制并发
    /// 可以控制30秒内不会重复
    /// </summary>
    public class ConcurrentControl
    {
        static object lockObj = new object();
        static System.Timers.Timer timer;
        static ConcurrentDictionary<string, DateTime> locks = new ConcurrentDictionary<string, DateTime>();
        public static void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }
        /// <summary>
        /// 检测当前键在阻止队列中,是否可用
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Check(string key)
        {
            return Check(key,30);
        }
        /// <summary>
        /// 检测当前键在阻止队列中,是否可用
        /// false则在阻止中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="second">秒</param>
        /// <returns></returns>
        public static bool Check(string key,int second)
        {
            try
            {
                if (timer == null)
                {
                    //thread = new Thread(new ThreadStart(RemoveExpired));
                    //thread.Start();
                    timer = new System.Timers.Timer(10000);
                    timer.Elapsed += (a, b) =>
                    {
                        RemoveExpired();
                    };
                    timer.Start();
                }
                lock (lockObj)
                {
                    
                    if (!locks.ContainsKey(key))
                    {
                        locks.TryAdd(key, DateTime.Now);
                        return true;
                    }
                    TimeSpan ts = DateTime.Now - locks[key];
                    if (ts.TotalSeconds > second)
                    {
                        locks[key] = DateTime.Now;
                        return true;
                    }
                    return false;
                }
            }
            catch { }
            return true;
        }
        /// <summary>
        /// 移除锁记录
        /// 在确定不需要锁时移除
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            DateTime d;
            locks.TryRemove(key, out d);
        }
        static void RemoveExpired()
        {
            try
            {
                var dic = new Dictionary<string, DateTime>(locks);
                foreach (var item in dic)
                {
                    var ts = DateTime.Now - item.Value;
                    if (ts.TotalMinutes > 1)
                    {
                        Remove(item.Key);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
