using Grpc.Core;

using GrpcService1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using CRL.Grpc.Extend.NetCore;
using CRL.Grpc.Extend;

namespace gRpcClient
{
    class Program
    {
        static IServiceProvider provider;
        static Program()
        {
            var builder = new ConfigurationBuilder();

            var configuration = builder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddOptions();

            services.AddGrpcExtend(op =>
            {
                op.Host = "127.0.0.1";
                op.Port = 50001;
                //op.UseConsulDiscover("http://localhost:8500", "grpcServer");//使用consul服务发现
                op.AddPolicy("Greeter.SayHello", new CRL.Core.Remoting.PollyAttribute() { RetryCount = 3 });//定义方法polly策略
            }, System.Reflection.Assembly.GetExecutingAssembly());

            provider = services.BuildServiceProvider();
        }


        static void Main(string[] args)
        {            
            //设置允许不安全的HTTP2支持
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var grpcConnect = provider.GetService<IGrpcConnect>();
            //认证
            //https://www.cnblogs.com/stulzq/p/11897628.html
            var token = "";
            var headers = new Metadata { { "Authorization", $"Bearer {token}" } };
            grpcConnect.SetMetadata(headers);


        label1:
            var client = provider.GetService<Greeter.GreeterClient>();
            var reply = client.SayHello(
                new HelloRequest { Name = "test" });
            Console.WriteLine("Greeter 服务返回数据: " + reply.Message);

            Console.ReadLine();
            goto label1;
        }
    }
}
