using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace CRL.Core.Remoting
{
    public class TestObj
    {
        public string Name { get; set; }
        public bool b { get; set; }
    }
    public interface ITestService
    {
        string Login();
        bool Test1(int a,int? b,out string error);
        TestObj Test2(TestObj obj);
    }
    public class TestService : AbsService, ITestService
    {
        [LoginPoint]
        public string Login()
        {
            SaveSession("hubro", "7777777777", "test");
            return "登录成功";
        }

        public bool Test1(int a, int? b, out string error)
        {
            //throw new Exception("ss");
            var user = CurrentUserName;
            var tag = CurrentUserTag;

            error = "out error";
            Console.WriteLine(a);
            Console.WriteLine(b);
            return true;
        }

        public TestObj Test2(TestObj obj)
        {
            Console.WriteLine(obj.ToJson());
            return obj;
        }
    }
    public class TestFactory
    {
        public static void RunTest(ITestService service)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            service.Login();
            Console.WriteLine("loginOk");
            int? a = 1;
            string error;
            service.Test1(1, a, out error);
            Console.WriteLine("error:" + error);
            var obj2 = service.Test2(new TestObj() { Name = "test" });
            Console.WriteLine("obj2:" + obj2.ToJson());
            sw.Stop();
            var el = sw.ElapsedMilliseconds;
            Console.WriteLine("el:" + el);
        }
    }
}
