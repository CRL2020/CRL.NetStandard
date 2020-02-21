using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using CRL.Core.EventBus.Queue;

namespace CRL.Core.EventBus
{
    public class EventDeclare : ICloneable
    {
        IQueue client;
        public EventDeclare(IQueue _client)
        {
            client = _client;
        }
        internal string Name;
        internal Type DataType;
        internal MethodInfo Method;
        internal Func<object, object[], object> MethodInvoke;
        internal bool IsAsync;
        internal bool IsArry;
        internal Func<object> InstanceInvoke;
        internal int ListTake;
        internal Queue<object> CacheData = new Queue<object>();
        internal DateTime CacheDataTime = DateTime.Now;

        public object Clone()
        {
            return MemberwiseClone();
        }

        internal void rePublish(string key)
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
            if (list.Count > 0)
            {
                client.Publish(key, list);
            }
        }
        internal void startPublishThread()
        {
            var threadWork = new ThreadWork();
            threadWork.Start("OnSubscribeIEnumerable<T>", () =>
            {
                var ts = DateTime.Now - CacheDataTime;
                if (ts.TotalSeconds > 1)
                {
                    rePublish(Name + "_L");
                }
                return true;
            }, 0.5);
        }
    }
    public class SubscribeService
    {
        static Dictionary<string, EventDeclare> eventRegister = new Dictionary<string, EventDeclare>();
        internal static EventDeclare GetEventDeclare(string name)
        {
            eventRegister.TryGetValue(name,out EventDeclare ed);
            return ed;
        }
        public static void StartSubscribe(params Type[] currentTypes)
        {
            var client = QueueFactory.GetQueueClient();
            foreach (var currentType in currentTypes)
            {
                var assembyle = System.Reflection.Assembly.GetAssembly(currentType);
                var types = assembyle.GetTypes();
                foreach (var type in types)
                {
                    var atr = type.GetCustomAttribute(typeof(SubscribeAttribute));
                    if (atr == null)
                    {
                        continue;
                    }
                    var methods = type.GetMethods();
                    //var instance = System.Activator.CreateInstance(type);
                    foreach (var m in methods)
                    {
                        var atr2 = m.GetCustomAttribute(typeof(SubscribeAttribute));
                        if (atr2 == null)
                        {
                            continue;
                        }
                        var ed = Register(client,atr2 as SubscribeAttribute, m);
                        ed.InstanceInvoke = () =>
                        {
                            return System.Activator.CreateInstance(type);
                        };
                    }
                }
            }
        
            foreach(var kv in new Dictionary<string,EventDeclare>(eventRegister))
            {
                var ed = kv.Value;
                client.Subscribe(ed);
                if (ed.IsArry)
                {
                    var clone = ed.Clone() as EventDeclare;
                    clone.Name = $"{ed.Name}_L";
                    clone.IsArry = false;
                    client.Subscribe(clone);
                    eventRegister.Add(clone.Name, clone);
                    ed.startPublishThread();
                }
            }
        }
        static EventDeclare Register(IQueue client, SubscribeAttribute attr, MethodInfo method)
        {
            var key = attr?.Name;
            var func = Core.DynamicMethodHelper.CreateMethodInvoker(method);
            var args1 = method.GetParameters().FirstOrDefault();
            if (args1 == null)
            {
                throw new Exception("至少一个参数");
            }
            if (string.IsNullOrEmpty(key))
            {
                key = args1.ParameterType.Name;
            }
            var isArry = typeof(System.Collections.IEnumerable).IsAssignableFrom(args1.ParameterType);
            var isAsync = method.ReturnType == typeof(Task);

            var ed = new EventDeclare(client)
            {
                DataType = args1.ParameterType,
                Name = key,
                Method = method,
                MethodInvoke = func,
                IsAsync = isAsync,
                IsArry = isArry,
                ListTake = attr.ListTake
            };
            eventRegister.Add(key, ed);
            return ed;
            //var client = QueueFactory.GetQueueClient();

            //if (isAsync)
            //{
            //    client.OnSubscribeAsync(key,args1.ParameterType, msg =>
            //      {
            //          var task = (Task)func.Invoke(serviceInstance, new object[] { msg });
            //          return task;
            //      });
            //    return;
            //}
            //if (isArry)
            //{
            //    client.OnSubscribe(key,args1.ParameterType, attr.Take, msg =>
            //      {
            //          func.Invoke(serviceInstance, new object[] { msg });
            //      });
            //}
            //else
            //{
            //    client.OnSubscribe(key, args1.ParameterType, msg =>
            //      {
            //          func.Invoke(serviceInstance, new object[] { msg });
            //      });
            //}
        }
    }
}
