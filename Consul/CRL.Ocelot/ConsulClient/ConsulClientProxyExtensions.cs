using CRL.Core.ConsulClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRL.Ocelot
{
    public static class ConsulClientProxyExtensions
    {
        public static IServiceCollection AddConsulProxy(this IServiceCollection services)
        {
            services.AddTransient<CRL.Core.ConsulClient.Client>();
            return services;
        }
        public static IApplicationBuilder UseConsulProxy(this IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            var Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/consul/RegisterService", async context =>
                {
                    using (var client = serviceProvider.GetService<CRL.Core.ConsulClient.Client>())
                    {
                        var ms = context.Request.Body;
                        var data = new byte[ms.Length];
                        ms.Read(data, 0, data.Length);
                        var args = System.Text.Encoding.UTF8.GetString(data);
                        var obj = JsonSerializer.Deserialize<ServiceRegistrationInfo>(args);
                        var result = client.RegisterService(obj);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                    }
                });
                endpoints.MapGet("/consul/DeregisterService", async context =>
                {
                    using (var client = serviceProvider.GetService<CRL.Core.ConsulClient.Client>())
                    {
                        var name = context.Request.Query["name"];
                        await context.Response.WriteAsync(name);
                    }
                });
            });
            return app;
        }
    }
}
