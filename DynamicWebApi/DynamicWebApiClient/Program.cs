using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace DynamicWebApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientConnect = new CRL.DynamicWebApi.ApiClientConnect("http://localhost:809");
            //clientConnect.SetJwtToken(jwtToken);
            clientConnect.UseSign();
            var service = clientConnect.GetClient<ITestService>();

        label1:
            TestFactory.RunTest(service);
            Console.ReadLine();
            goto label1;
        }
    }
}
