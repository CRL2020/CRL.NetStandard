#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#endif

namespace CRL.NetCore
{
#if NETSTANDARD
    public static class CRLExtensions
    {
        public static void AddCRL(this IServiceCollection services, Action<ISettingConfigBuilder> setupAction)
        {
            var builder = CRL.SettingConfigBuilder.CreateInstance();
            setupAction(builder);
        }
    }
#endif
}
