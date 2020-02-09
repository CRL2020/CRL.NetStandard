using CRL.Core.Remoting;
using CRL.DynamicWebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRL.Ocelot
{
    public static class DynamicApiExtensions
    {
        static ServerCreater server;
        static DynamicApiExtensions()
        {
            server = new ServerCreater().CreatetApi();
        }
        public static void AddDynamicApi(this IServiceCollection services,params Type[] currentTypes)
        {
            foreach (var currentType in currentTypes)
            {
                var assembyle = System.Reflection.Assembly.GetAssembly(currentType);
                var types = assembyle.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(AbsService)))
                    {
                        var theFilter = new System.Reflection.TypeFilter(MyInterfaceFilter);
                        var implementedInterfaces = type.FindInterfaces(theFilter, type.BaseType).FirstOrDefault();
                        if (implementedInterfaces == null)
                        {
                            continue;
                        }
                        //实现注册
                        server.Register(implementedInterfaces, type);
                        services.AddTransient(implementedInterfaces, type);
                    }
                }
            }
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

        public static void UseDynamicApi(this IApplicationBuilder app)
        {
            app.UseMiddleware<DynamicApiMiddleware>();
        }
    }
}
