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
        [Subscribe("timeTest")]
        public void Test(DateTime time)
        {
            //throw new Exception("error");
            Console.WriteLine($"receive {time}");
        }
        //[Subscribe("timeTest")]
        //public void Test(List<DateTime> time)
        //{
        //    Console.WriteLine($"List<DateTime> receive {time.First()}");
        //}
        //[Subscribe("timeTest")]
        //public async Task Test2(DateTime time)
        //{
        //    Console.WriteLine($"async DateTime receive {time}");
        //    await Task.Delay(100);
        //}
        //[Subscribe("timeTest")]
        //public async Task Test2(List<DateTime> time)
        //{
        //    Console.WriteLine($"async List<DateTime> receive {time.First()}");
        //    await Task.Delay(100);
        //}

        [Subscribe("intTest", ThreadSleepSecond = 1)]
        public void Test2(List<int> a)
        {
            Console.WriteLine($"List<int> receive {a.First()}");
        }

        //[Subscribe("intTest", QueueName ="queue1")]
        //public async Task Test2(int a)
        //{
        //    //throw new Exception("3333");
        //    Console.WriteLine($"int receive {a}");
        //    await Task.Delay(100);
        //}

    }
}
