using CRL.EventBus;
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
            var config = new QueueConfig();
            //config.UseRabbitMQ("127.0.0.1", "guest", "guest");
            //config.UseRedis("Server_204@127.0.0.1:6389");
            config.UseMongoDb("mongodb://test:test@127.0.0.1:27017/test");
            var client = new Publisher(config);
            client.Publish("timeTest", DateTime.Now);
            var subService = new SubscribeService(config);
            //注册方式订阅
            //subService.Register(System.Reflection.Assembly.GetAssembly(typeof(SubscribeTest)));
            //subService.StartSubscribe();

            //手动指定订阅
            subService.StartSubscribe<List<DateTime>>(new SubscribeAttribute("timeTest"), b =>
             {
                 Console.WriteLine(b);
             });
        label1:
            client.Publish("timeTest", DateTime.Now);
            client.Publish("intTest", DateTime.Now.Second);
            Console.WriteLine("send ok");
            Console.ReadLine();
            goto label1;
        }
    }
}
