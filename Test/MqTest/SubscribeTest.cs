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
        //[Subscribe("timeTest")]
        //public void Test(DateTime time)
        //{
        //    Console.WriteLine($"receive {time}");
        //}
        [Subscribe("intTest")]
        public void Test2(List<int> a)
        {
            Console.WriteLine($"List<int> receive {a.First()}");
        }
        //[Subscribe("timeTest")]
        //public void Test(List<DateTime> time)
        //{
        //    Console.WriteLine($"List<DateTime> receive {time.First()}");
        //}
        //[Subscribe("timeTest")]
        //public async Task Test2(DateTime time)
        //{
        //    Console.WriteLine($"DateTime receive {time}");
        //    await Task.Delay(100);
        //}
        //[Subscribe("intTest")]
        //public async Task Test2(int a)
        //{
        //    Console.WriteLine($"int receive {a}");
        //    await Task.Delay(100);
        //}
        [Subscribe("timeTest")]
        public async Task Test2(List<DateTime> time)
        {
            Console.WriteLine($"async List<DateTime> receive {time.First()}");
            await Task.Delay(100);
        }
        //[Subscribe("intTest")]
        //public async Task Test2(List<int> a)
        //{
        //    Console.WriteLine($"async List<int> receive {a.First()}");
        //    await Task.Delay(100);
        //}
    }
}
