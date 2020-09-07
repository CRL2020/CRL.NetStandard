using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    #region obj
    public class serviceInfo
    {
        public static System.Collections.Concurrent.ConcurrentDictionary<string, Type> apiPrefixCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Type>();
        static ConcurrentDictionary<Type, serviceInfo> serviceInfoCache = new ConcurrentDictionary<Type, serviceInfo>();
        public static serviceInfo GetServiceInfo(Type type, bool initObjCtor = false)
        {
            var a = serviceInfoCache.TryGetValue(type,out var info);
            if(a)
            {
                return info;
            }
            info = new serviceInfo()
            {
                ServiceType = type,
                Attributes = type.GetCustomAttributes().ToList(),
               
            };
            if (initObjCtor)
            {
                info.InstaceCtor = DynamicMethodHelper.CreateCtorFunc<Func<object>>(type, Type.EmptyTypes);
            }
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var methodInfoList = new List<methodInfo>();
            foreach (var m in methods)
            {
                var mInfo = new methodInfo()
                {
                    Attributes = m.GetCustomAttributes().ToList(),
                    MethodInfo = m,
                    Parameters = m.GetParameters(),
                    MethodInvoker = DynamicMethodHelper.CreateMethodInvoker(m)
                };
                mInfo.IsAsync = m.ReturnType.Name.StartsWith("Task`1");
                if ( mInfo.IsAsync)//总是返回同步结果
                {
                    mInfo.TaskInvoker = DynamicMethodHelper.TaskResultInvoker<object>(m.ReturnType);
                    var taskType = typeof(AsyncResult<>).MakeGenericType(m.ReturnType.GetGenericArguments()[0]);
                    mInfo.TaskCreater = DynamicMethodHelper.CreateCtorFunc<Func<AsyncResult>>(taskType, new Type[0]);
                }
                methodInfoList.Add(mInfo);
            }
            info.Methods = methodInfoList;
            info.ServiceAttribute = type.GetCustomAttribute<ServiceAttribute>() ?? new ServiceAttribute();
            apiPrefixCache.TryAdd(info.ServiceAttribute.ApiPrefix, type);
            serviceInfoCache.TryAdd(type, info);
            return info;
        }
        internal Type ServiceType;
        internal Func<object> InstaceCtor;
        internal Type InterfaceType;
        internal List<methodInfo> Methods = new List<methodInfo>();

        public ServiceAttribute ServiceAttribute { get; private set; }

        internal List<Attribute> Attributes = new List<Attribute>();
        public T GetAttribute<T>() where T : Attribute
        {
            foreach (var item in Attributes)
            {
                if (item is T)
                {
                    return item as T;
                }
            }
            return null;
        }
        public methodInfo GetMethod(string name)
        {
            return Methods.Find(b => b.MethodInfo.Name == name);
        }
    }
    public class methodInfo
    {
        /// <summary>
        /// 方法调用
        /// </summary>
        public Func<object, object[], object> MethodInvoker;
        /// <summary>
        /// Task访问
        /// </summary>
        public Func<object, object> TaskInvoker;

        /// <summary>
        /// Task创建
        /// </summary>
        public Func<AsyncResult> TaskCreater;

        /// <summary>
        /// 是否为异步方法
        /// </summary>
        public bool IsAsync;
        public MethodInfo MethodInfo;
        public List<Attribute> Attributes = new List<Attribute>();
        public ParameterInfo[] Parameters;
        public T GetAttribute<T>() where T : Attribute
        {
            foreach (var item in Attributes)
            {
                if (item is T)
                {
                    return item as T;
                }
            }
            return null;
        }
    }
    #endregion
}
