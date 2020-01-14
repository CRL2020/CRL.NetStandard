using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public class ServerCreater
    {
        AbsServer Server;
        /// <summary>
        /// 验证签名
        /// </summary>
        internal static bool __CheckSign = false;
        /// <summary>
        /// 验证签名
        /// 参数都会以ToString计算,注意类型问题
        /// </summary>
        /// <returns></returns>
        public ServerCreater CheckSign()
        {
            __CheckSign = true;
            return this;
        }
        internal static ISessionManage SessionManage
        {
            get; set;
        } = new SessionManage();
        public void SetServer(AbsServer server)
        {
            Server = server;
        }
        public AbsServer GetServer()
        {
            return Server;
        }
        public ServerCreater SetSessionManage(ISessionManage _sessionManage)
        {
            SessionManage = _sessionManage;
            return this;
        }
        /// <summary>
        /// 指定类型注册
        /// </summary>
        /// <typeparam name="IService"></typeparam>
        /// <typeparam name="Service"></typeparam>
        /// <returns></returns>
        public ServerCreater Register<IService, Service>() where Service : AbsService, IService, new() where IService : class
        {
            Server.Register<IService, Service>();
            return this;
        }
        /// <summary>
        /// 按类型所在程序集注册所有
        /// </summary>
        /// <param name="currentTypes"></param>
        /// <returns></returns>
        public ServerCreater RegisterAll(params Type[] currentTypes)
        {
            foreach (var currentType in currentTypes)
            {
                var assembyle = System.Reflection.Assembly.GetAssembly(currentType);
                var types = assembyle.GetTypes();
                foreach(var type in types)
                {
                    if(type.IsSubclassOf(typeof(AbsService)))
                    {
                        var theFilter = new System.Reflection.TypeFilter(MyInterfaceFilter);
                        var implementedInterfaces = type.FindInterfaces(theFilter, type.BaseType).FirstOrDefault() ;
                        if (implementedInterfaces == null)
                        {
                            continue;
                        }
                        //实现注册
                        var mainType = this.GetType();
                        var method = mainType.GetMethod(nameof(Register), BindingFlags.Public | BindingFlags.Instance);
                        method.MakeGenericMethod(new Type[] { implementedInterfaces, type }).Invoke(this, new object[] { });
                    }
                }
            }
            return this;
        }
        static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            // 1. "typeObj" is a Type object of an interface supported by class B.
            // 2. "criteriaObj" will be a Type object of the base class of B : 
            // i.e. the Type object of class A.
            Type baseClassType = (Type)criteriaObj;
            // Obtain an array of the interfaces supported by the base class A.
            Type[] interfaces_array = baseClassType.GetInterfaces();
            for (int i = 0; i < interfaces_array.Length; i++)
            {
                // If typeObj is an interface supported by the base class, skip it.
                if (typeObj.ToString() == interfaces_array[i].ToString())
                    return false;
            }

            return true;
        }
        public void Start()
        {
            Server.Start();
        }
        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
