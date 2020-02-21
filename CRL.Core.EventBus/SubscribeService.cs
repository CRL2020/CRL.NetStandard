using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace CRL.Core.EventBus
{
    public class SubscribeService
    {
        public static void StartSubscribe(params Type[] currentTypes)
        {
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
                    var instance = System.Activator.CreateInstance(type);
                    foreach (var m in methods)
                    {
                        var atr2 = m.GetCustomAttribute(typeof(SubscribeAttribute));
                        if (atr2 == null)
                        {
                            continue;
                        }
                        Subscribe(instance, atr2 as SubscribeAttribute, m);
                    }
                }
            }
        }
        static void Subscribe(object serviceInstance, SubscribeAttribute attr, MethodInfo method)
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
            var client = QueueFactory.GetQueueClient(key, false);
            var isArry = typeof(System.Collections.IEnumerable).IsAssignableFrom(args1.ParameterType);
            var isAsync = method.ReturnType == typeof(Task);
            if (isAsync)
            {
                client.OnSubscribeAsync(args1.ParameterType, msg =>
                  {
                      var task = (Task)func.Invoke(serviceInstance, new object[] { msg });
                      return task;
                  });
                return;
            }
            if (isArry)
            {
                client.OnSubscribe(args1.ParameterType, attr.Take, msg =>
                  {
                      func.Invoke(serviceInstance, new object[] { msg });
                  });
            }
            else
            {
                client.OnSubscribe(args1.ParameterType, msg =>
                 {
                     func.Invoke(serviceInstance, new object[] { msg });
                 });
            }
        }
    }
}
