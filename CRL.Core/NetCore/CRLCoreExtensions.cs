#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#endif

namespace CRL.Core.NetCore
{
#if NETSTANDARD
    public static class CRLCoreExtensions
    {
        public static void AddCRLCore(this IServiceCollection services, Action<ConfigBuilder> setupAction)
        {
            var configBuilder = new CRL.Core.ConfigBuilder();
            setupAction(configBuilder);
        }
    }
#endif
}
