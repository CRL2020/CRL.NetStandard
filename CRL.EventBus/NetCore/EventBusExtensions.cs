#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endif
namespace CRL.EventBus.NetCore
{
#if NETSTANDARD
    public static class EventBusExtensions
    {
        public static void AddEventBus(this IServiceCollection services, Action<QueueConfig> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<IPublisher, Publisher>();
        }
        public static void StartEventBusSubscribe(this IServiceCollection services)
        {
            services.AddSingleton<SubscribeService>();
            services.AddHostedService<SubscribeBackgroundService>();
        }
    }

    class SubscribeBackgroundService : BackgroundService
    {
        SubscribeService subscribeService;
        QueueConfig queueConfig;
        IServiceProvider serviceProvider;
        public SubscribeBackgroundService(Microsoft.Extensions.Options.IOptions<QueueConfig> options,SubscribeService _subscribeService, IServiceProvider _serviceProvider)
        {
            queueConfig = options.Value;
            subscribeService = _subscribeService;
            serviceProvider = _serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            subscribeService.Register(queueConfig.SubscribeAssemblies);
            stoppingToken.Register(() =>
            {
                subscribeService.StopSubscribe();
            });

            subscribeService.StartSubscribe(type =>
            {
                return serviceProvider.GetService(type);
            });
            return Task.CompletedTask;
        }
    }
#endif
}
