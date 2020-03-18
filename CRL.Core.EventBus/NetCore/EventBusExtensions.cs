#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#endif
namespace CRL.Core.EventBus.NetCore
{
#if NETSTANDARD
    public static class EventBusExtensions
    {
        public static QueueConfig AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddTransient<IPublisher, Publisher>();

            SubscribeService.Register(assemblies);
            return QueueConfig.Instance;
        }
    }
#endif
}
