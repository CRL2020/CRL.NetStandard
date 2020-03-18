using Grpc.Core;
using Grpc.Net.Client;
using GrpcService1;
using System;
using System.Threading.Tasks;

namespace gRpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var grpcConnect = new CRL.Grpc.Extend.GrpcConnect("http://127.0.0.1:50001");
            //grpcConnect.UseConsulDiscover("http://127.0.0.1:5000", "grpcServer");//使用consul服务发现
            var client = grpcConnect.GetClient<Greeter.GreeterClient>();

            //var channel = GrpcChannel.ForAddress("http://localhost:50001");
            //var client = new Greeter.GreeterClient(channel);
            var headers = new Metadata { { "Authorization", $"Bearer token" } };
        label1:
            var reply = client.SayHello(
                new HelloRequest { Name = "test" }, headers);
            Console.WriteLine("Greeter 服务返回数据: " + reply.Message);

            Console.ReadLine();
            goto label1;
        }
    }
}
