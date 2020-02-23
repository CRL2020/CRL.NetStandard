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
            var config = new QueueConfig() { Host = "47.105.149.240", Pass = "henanhaiwang", User = "henanhaiwang" };
            QueueConfig.SetConfig(config);

            var client = new Publisher();
            SubscribeService.RegisterAll(typeof(SubscribeTest));
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
