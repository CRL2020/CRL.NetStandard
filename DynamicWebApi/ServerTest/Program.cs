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
            server.UseJWTUseAuthorization(tokenCheck);
            server.SetSessionManage(new SessionManage());
            //server.Register<ITestService, TestService>();
            server.RegisterAll(typeof(TestService));
            var listener = new ServerListener();
            listener.Start("http://localhost:809/");
            label1:
            PollyTest();
            Console.ReadLine();
            goto label1;
        }
      
        static bool tokenCheck(MessageBase req, out string user, out string error)
        {
            error = "";
            var tuple = JwtHelper.ReadToken(req.Token);
            var a = tuple.Item1.TryGetValue("name", out user);
            return a;
        }
        static void PollyTest()
        {
            var atr = new CRL.Core.Remoting.PollyAttribute();
            //atr.TimeOutTime = TimeSpan.FromMilliseconds(100);
            //atr.CircuitBreakerCount = 2;
            atr.RetryCount = 1;
            var str = PollyExtension.Invoke(atr, () =>
             {
                throw new Exception("has error");
                //System.Threading.Thread.Sleep(200);
                 return new PollyExtension.PollyData<string>() { Error = "ok" };
             }, "test");
            Console.WriteLine(str.Error);
        }
    }
}
