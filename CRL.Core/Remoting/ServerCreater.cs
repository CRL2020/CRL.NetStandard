﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public class ServerCreater
    {
        public static ServerCreater Instance;
        public ServerCreater()
        {
            Instance = this;
        }
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
        /// 设置jwt认证方法,和简单登录认证排斥
        /// </summary>
        /// <param name="jwtTokenCheck"></param>
        /// <returns></returns>
        public ServerCreater UseJWTUseAuthorization(JwtTokenCheckHandler jwtTokenCheck)
        {
            Server._jwtTokenCheck = jwtTokenCheck;
            return this;
        }
        /// <summary>
        /// 指定类型注册
        /// </summary>
        /// <typeparam name="IService"></typeparam>
        /// <typeparam name="Service"></typeparam>
        /// <returns></returns>
        public ServerCreater Register<IService, Service>() where Service : AbsService, IService where IService : class
        {
            Server.Register<IService, Service>();
            return this;
        }
        public ServerCreater Register(Type interfaceType, Type serviceType)
        {
            Server.Register(interfaceType, serviceType);
            return this;
        }
        /// <summary>
        /// 按类型所在程序集注册所有
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public ServerCreater RegisterAll(params Assembly[] assemblies)
        {
            foreach (var assembyle in assemblies)
            {
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
                        Server.Register(implementedInterfaces, type);
                        //var mainType = this.GetType();
                        //var method = mainType.GetMethod(nameof(Register), BindingFlags.Public | BindingFlags.Instance);
                        //method.MakeGenericMethod(new Type[] { implementedInterfaces, type }).Invoke(this, new object[] { });
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

    public delegate bool JwtTokenCheckHandler(MessageBase req, out string user, out string error);
}
