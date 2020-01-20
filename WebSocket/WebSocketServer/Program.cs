using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.WebSocket;
namespace WebSocketServer
{
    class Program
    {
        class socketMsg
        {
            public string name
            {
                get; set;
            }
        }
        static void Main(string[] args)
        {
            var server = new ServerCreater().CreatetWebSocket(8015);
            server.CheckSign();
            server.SetSessionManage(new SessionManage());
            //server.Register<ITestService, TestService>();
            server.RegisterAll(typeof(TestService));
            server.Start();
            new CRL.Core.ThreadWork().Start("send", () =>
            {
                var socket = server.GetServer() as CRL.WebSocket.WebSocketServer;
                socket.SendMessage("hubro", new socketMsg() { name = DateTime.Now.ToString() }, out string error);
                Console.WriteLine("send msg");
                return true;
            }, 10);
            Console.ReadLine();
        }
    }
}
