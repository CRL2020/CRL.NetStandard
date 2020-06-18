using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using CRL.EventBus.Queue;
using CRL.Core;

namespace CRL.EventBus
{

    public class SubscribeService
    {
        static Dictionary<string, EventDeclare> eventRegister = new Dictionary<string, EventDeclare>();
        QueueConfig queueConfig;
#if NETSTANDARD
        public SubscribeService(Microsoft.Extensions.Options.IOptions<QueueConfig> options)
        {
            queueConfig = options.Value;
        }
#endif
        public SubscribeService(QueueConfig _queueConfig)
        {
            queueConfig = _queueConfig;
        }
        internal static EventDeclare GetEventDeclare(string name)
        {
            eventRegister.TryGetValue(name,out EventDeclare ed);
            return ed;
        }
        public void Register(params Assembly[] assemblies)
        {
            foreach (var assembyle in assemblies)
            {
                var types = assembyle.GetTypes();
                foreach (var type in types)
                {
                    Register(type);
                }
            }  
        }
        public void Register(Type type)
        {
            var atr = type.GetCustomAttribute(typeof(SubscribeAttribute));
            if (atr == null)
            {
                return;
            }
            var methods = type.GetMethods();
            foreach (var m in methods)
            {
                var atr2 = m.GetCustomAttribute(typeof(SubscribeAttribute));
                if (atr2 == null)
                {
                    continue;
                }
                var ed = CreateEventDeclare(atr2 as SubscribeAttribute, m);
                ed.ServiceInstanceType = type;
                ed.ServiceInstanceCtor2 = DynamicMethodHelper.CreateCtorFunc<Func<object>>(type, Type.EmptyTypes);
                if (eventRegister.ContainsKey(ed.Name))
                {
                    throw new Exception($"已注册过相同的事件名 {ed.Name}");
                }
                eventRegister.Add(ed.Name, ed);

                if (ed.IsArray)//集合重新创建一个事件定义
                {
                    var clone = ed.Clone() as EventDeclare;
                    clone.IsCopy = true;
                    eventRegister.Add(clone.GetArrayName(), clone);
                }
            }
        }
        /// <summary>
        /// 启动订阅
        /// </summary>
        /// <param name="serviceInstanceCtor">core 传入服务实例化委托</param>
        public void StartSubscribe(Func<Type, object> serviceInstanceCtor = null)
        {
            //var queueConfig = QueueConfig.GetConfig();
            foreach (var ed in eventRegister.Values)
            {
                var queue = QueueFactory.GetQueueClient(queueConfig, ed);
                ed.IQueue = queue;
                ed.ServiceInstanceCtor = serviceInstanceCtor;//core 传入服务实例化委托
                if (ed.IsCopy)
                {
                    ed.StartPublishThread();
                }
                if (ed.IsAsync)
                {
                    queue.SubscribeAsync(ed);
                }
                else
                {
                    queue.Subscribe(ed);
                }
            }
        }
        /// <summary>
        /// 手动指定订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attr"></param>
        /// <param name="func"></param>
        public void StartSubscribe<T>(SubscribeAttribute attr, Action<T> func)
        {
            var ed = CreateEventDeclare<T>(attr, (i, b) =>
            {
                func((T)b.First());
                return null;
            });
            var queue = QueueFactory.GetQueueClient(queueConfig, ed);
            ed.IQueue = queue;
            if (ed.IsCopy)
            {
                ed.StartPublishThread();
            }
            if (ed.IsAsync)
            {
                queue.SubscribeAsync(ed);
            }
            else
            {
                queue.Subscribe(ed);
            }
        }
        EventDeclare CreateEventDeclare<T>(SubscribeAttribute attr, Func<object, object[], object> func)
        {
            var key = attr?.Name;
            var eventDataType = typeof(T);
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("name不能为空");
            }
            var isArry = typeof(System.Collections.IEnumerable).IsAssignableFrom(eventDataType);

            var ed = new EventDeclare()
            {
                EventDataType = eventDataType,
                Name = key,
                MethodInvoke = func,
                //IsAsync = isAsync,
                IsArray = isArry,
                ListTake = attr.ListTake,
                QueueName = attr.QueueName,
                ThreadSleepSecond = attr.ThreadSleepSecond
            };
            return ed;
        }
        EventDeclare CreateEventDeclare(SubscribeAttribute attr, MethodInfo method)
        {
            var key = attr?.Name;
            var func = Core.DynamicMethodHelper.CreateMethodInvoker(method);
            var args1 = method.GetParameters().FirstOrDefault();
            if (args1 == null)
            {
                throw new Exception("至少一个参数");
            }
            var eventDataType = args1.ParameterType;
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("name不能为空");
            }
            var isArry = typeof(System.Collections.IEnumerable).IsAssignableFrom(eventDataType);
            var isAsync = method.ReturnType == typeof(Task);

            var ed = new EventDeclare()
            {
                EventDataType = eventDataType,
                Name = key,
                MethodInvoke = func,
                IsAsync = isAsync,
                IsArray = isArry,
                ListTake = attr.ListTake,
                QueueName = attr.QueueName,
                ThreadSleepSecond = attr.ThreadSleepSecond
            };
            return ed;
        }

        public void StopSubscribe()
        {
            QueueFactory.DisposeAll();
        }
    }
}
