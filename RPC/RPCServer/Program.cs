using CRL.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Remoting;
namespace RPCServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ServerCreater().CreatetRPC(805);
            server.CheckSign();
            server.SetSessionManage(new SessionManage());
            //server.Register<ITestService, TestService>();
            server.RegisterAll(typeof(TestService));
            server.Start();
            Console.ReadLine();
        }
    }
}
