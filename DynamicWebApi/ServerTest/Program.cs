using CRL.Core.Remoting;
using CRL.DynamicWebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ServerCreater().CreatetApi();
            server.CheckSign();
            //server.UseJWTUseAuthorization(check);
            server.SetSessionManage(new SessionManage());
            //server.Register<ITestService, TestService>();
            server.RegisterAll(typeof(TestService));
            var listener = new ServerListener();
            listener.Start("http://localhost:809/");
            Console.ReadLine();
        }
      
    }
}
