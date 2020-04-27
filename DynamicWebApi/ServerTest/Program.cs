using CRL.Core.Remoting;
using CRL.DynamicWebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ServerCreater().CreateApi();
            server.CheckSign();
            server.UseJWTUseAuthorization(tokenCheck);
            server.SetSessionManage(new SessionManage());
            //server.Register<ITestService, TestService>();
            server.RegisterAll(System.Reflection.Assembly.GetAssembly(typeof(TestService)));
            var listener = new ServerListener();
            listener.Start("http://localhost:8019/");
            label1:
            //PollyTest().Wait();
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
        static async Task PollyTest()
        {
            var atr = new CRL.Core.Remoting.PollyAttribute();
            //atr.TimeOutTime = TimeSpan.FromMilliseconds(100);
            atr.CircuitBreakerCount = 2;
            atr.RetryCount = 1;
            var pollyCtx = new Polly.Context();
            var response = await PollyExtension.InvokeAsync<string>(atr, async () =>
            {
                //await Task.Delay(200);
                throw new Exception("has error");
                return await Task.FromResult(new CRL.Core.Remoting.PollyExtension.PollyData<string>() {
                Data="ok"});
            }, "");
            if (pollyCtx.ContainsKey("msg"))
            {
                Console.WriteLine(pollyCtx["msg"]);
            }

            //var str = PollyExtension.Invoke(atr, () =>
            // {
            //    throw new Exception("has error");
            //    //System.Threading.Thread.Sleep(200);
            //     return new PollyExtension.PollyData<string>() { Error = "ok" };
            // }, "test");
            Console.WriteLine(response.ToJson());
        }
    }
}
