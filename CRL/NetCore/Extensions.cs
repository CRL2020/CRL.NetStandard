#if NETSTANDARD
using CRL.Sharding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#endif

namespace CRL.NetCore
{
#if NETSTANDARD
    public static class Extensions
    {
        public static void AddCRL<T>(this IServiceCollection services) where T : class, IDBLocationCreator
        {
            services.AddSingleton<IDbConfigRegister, DBConfigRegister>();
            services.AddSingleton<IDBLocationCreator, T>();
        }
        public static void UseCRL(this IServiceProvider provider)
        {
            var dBLocationCreator = provider.GetService<IDBLocationCreator>();
            dBLocationCreator.Init();
        }
    }
    public interface IDBLocationCreator
    {
        void Init();
        
    }
#endif
}
