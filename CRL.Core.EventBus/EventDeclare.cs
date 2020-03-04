using CRL.Core.EventBus.Queue;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CRL.Core.EventBus
{
    public class EventDeclare
    {
        internal AbsQueue IQueue;
        internal string Name;
        internal Type EventDataType;
        internal MethodInfo Method;
        internal Func<object, object[], object> MethodInvoke;
        internal bool IsAsync;
        internal bool IsArray;
        internal Type ServiceInstanceType;
        internal Func<Type, object> ServiceInstanceCtor = null;
        internal Func<object> ServiceInstanceCtor2 = null;
        internal int ListTake;
        internal Queue<object> CacheData = new Queue<object>();
        DateTime CacheDataTime = DateTime.Now;
        internal string QueueName;
        internal bool IsCopy;

        internal double ThreadSleepSecond = 1;

        internal object Clone()
        {
            return MemberwiseClone();
        }
        internal object CreateServiceInstance()
        {
            if (ServiceInstanceCtor != null)
            {
                return ServiceInstanceCtor(ServiceInstanceType);
            }
            else
            {
                return ServiceInstanceCtor2();
            }
        }
        internal string GetArrayName()
        {
            return $"{Name}_Array";
        }
        internal void setCache(object obj)
        {
            CacheData.Enqueue(obj);
            CacheDataTime = DateTime.Now;
        }
        internal void rePublish()
        {
            int i = 0;
            var list = new List<object>();
            while (i <= ListTake && CacheData.Count > 0)
            {
                var obj = CacheData.Dequeue();
                list.Add(obj);
                i += 1;
            }
            CacheDataTime = DateTime.Now;
            var key = GetArrayName();
            if (list.Count > 0)
            {
                IQueue.Publish(key, list);
            }
        }
        internal void StartPublishThread()
        {
            if (!IsCopy)
            {
                return;
            }
            var threadWork = new ThreadWork();
            threadWork.Start("OnSubscribeIEnumerable<T>", () =>
            {
                var ts = DateTime.Now - CacheDataTime;
                if (ts.TotalSeconds > 1)
                {
                    rePublish();
                }
                return true;
            }, 0.5);
        }
    }
}
