using CRL.Core.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqTest
{
    [Subscribe]
    public class SubscribeTest
    {
        //[Subscribe("testQueueTime")]
        public void Test(DateTime time)
        {
            Console.WriteLine($"receive {time}");
        }
        [Subscribe("testQueueTime")]
        public void Test(List<DateTime> time)
        {
            Console.WriteLine($"receive {time.Count}");
        }
    }
}
