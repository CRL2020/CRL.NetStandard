using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GrpcService1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<HealthCheckService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            //注册服务
            var consulClient = new CRL.Core.ConsulClient.Consul("http://localhost:8500");
            var info = new CRL.Core.ConsulClient.ServiceRegistrationInfo
            {
                Address = "127.0.0.1",
                Name = "grpcServer",
                ID = "grpcServer1",
                Port = 50001,
                Tags = new[] { "v1" },
                Check = new CRL.Core.ConsulClient.CheckRegistrationInfo()
                {
                    GRPC = "127.0.0.1:50001",
                    Interval = "10s",
                    GRPCUseTLS = false,
                    DeregisterCriticalServiceAfter = "90m"
                }
            };
            consulClient.DeregisterService(info.ID);
            var a = consulClient.RegisterService(info);

        }
    }
}
