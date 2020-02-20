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
            SubscribeService.StartSubscribe(typeof(SubscribeTest));
            var client = new Publisher();
        label1:
            client.Publish("testQueueTime", DateTime.Now);

            Console.WriteLine("send ok");
            Console.ReadLine();
            goto label1;
        }
    }
}
