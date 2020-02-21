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
        //public void Test(DateTime time)
        //{
        //    Console.WriteLine($"receive {time}");
        //}
        [Subscribe("testQueueTime2")]
        public void Test2(int a)
        {
            Console.WriteLine($"receive2 {a}");
        }
        [Subscribe("testQueueTime")]
        public void Test(List<DateTime> time)
        {
            Console.WriteLine($"receive {time.Count}");
        }
        //[Subscribe("testQueueTime")]
        //public async Task Test2(DateTime time)
        //{
        //    Console.WriteLine($"receive {time}");
        //    await Task.Delay(100);
        //}
    }
}
