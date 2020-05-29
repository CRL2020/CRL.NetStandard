#if NETSTANDARD
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#endif

namespace CRL.GrpcExtend.NetCore
{
#if NETSTANDARD
    public static class GrpcExtensions
    {
        public static void AddGrpcExtend(this IServiceCollection services, Action<GrpcClientOptions> setupAction, params Assembly[] assemblies)
        {
            services.Configure(setupAction);
            services.AddSingleton<IGrpcConnect, GrpcConnect>();
            services.AddScoped<CallInvoker, GRpcCallInvoker>();
            foreach (var assembyle in assemblies)
            {
                var types = assembyle.GetTypes();
                foreach (var type in types)
                {
                    if(typeof(ClientBase).IsAssignableFrom(type))
                    {
                        services.AddSingleton(type);
                    }
                }
            }
        }
    }
#endif
}
