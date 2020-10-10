using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqTest
{
    class DirectRabbitMQTest
    {
        public static void test()
        {
            var queueName = "testqueue1";
            var client = new CRL.RabbitMQ.SimpleRabbitMQ("127.0.0.1", "guest", "guest", queueName);
            //client.Publish(DateTime.Now.ToString());
            client.BeginReceive<string>((msg,key) =>
              {
                  Console.WriteLine(msg);
              });
        label1:
            client.Publish(DateTime.Now.ToString());
            Console.ReadLine();
            goto label1;
        }
    }
}
