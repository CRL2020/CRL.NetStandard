#if NETSTANDARD
using CRL.Core.Remoting;
using CRL.DynamicWebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#endif
namespace CRL.DynamicWebApi.NetCore
{
#if NETSTANDARD
    public static class DynamicApiExtensions
    {
        static Action<ServerCreater> _setupAction;
        static Assembly[] _assemblies;
        public static void AddDynamicApi(this IServiceCollection services, Action<ServerCreater> setupAction, params Assembly[] assemblies)
        {
            _setupAction = setupAction;
            _assemblies = assemblies;
            services.AddSingleton<ServerCreater>();

            foreach (var assembyle in assemblies)
            {
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
                        //注册AbsService
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
            var serverCreater = app.ApplicationServices.GetService<ServerCreater>();
            serverCreater.CreateApi().UseCoreInjection();
            _setupAction(serverCreater);
            serverCreater.RegisterAll(_assemblies);
            app.UseMiddleware<DynamicApiMiddleware>();
        }
    }
#endif
}
