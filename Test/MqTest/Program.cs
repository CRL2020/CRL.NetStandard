using CRL.Core.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //QueueConfig.UseRabbitMQ("127.0.0.1", "test", "test");
            //QueueConfig.UseRedis("Server_204@127.0.0.1:6389");
            QueueConfig.Instance.UseMongoDb("mongodb://test:test@127.0.0.1:27017/test");
            var client = new Publisher();
            SubscribeService.Register(System.Reflection.Assembly.GetAssembly(typeof(SubscribeTest)));
            SubscribeService.StartSubscribe();
        label1:
            client.Publish("timeTest", DateTime.Now);
            client.Publish("intTest", DateTime.Now.Second);
            Console.WriteLine("send ok");
            Console.ReadLine();
            goto label1;
        }
    }
}
