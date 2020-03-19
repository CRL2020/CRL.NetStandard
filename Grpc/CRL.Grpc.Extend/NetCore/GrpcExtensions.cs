#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#endif

namespace CRL.Grpc.Extend.NetCore
{
#if NETSTANDARD
    public static class GrpcExtensions
    {
        public static void AddGrpcExtend(this IServiceCollection services, Action<GrpcClientOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddSingleton<IGrpcConnect, GrpcConnect>();
        }
    }
#endif
}
