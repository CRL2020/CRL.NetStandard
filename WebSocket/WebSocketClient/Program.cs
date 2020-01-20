using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using CRL.Core.Remoting;

namespace WebSocketClient
{
    class Program
    {
        class socketMsg
        {
            public string name
            {
                get;set;
            }
        }
        static void showId()
        {
            var id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(id);
        }
        static void Main(string[] args)
        {

            var clientConnect = new CRL.WebSocket.WebSocketClientConnect("127.0.0.1", 8015);
            clientConnect.UseSign();
            clientConnect.SubscribeMessage<socketMsg>((obj) =>
            {
                Console.WriteLine("OnMessage:" + obj.ToJson());
            });
            clientConnect.StartPing();
            var service = clientConnect.GetClient<ITestService>();
        label1:
            TestFactory.RunTest(service);
            Console.ReadLine();
            goto label1;
        }
    }
}
