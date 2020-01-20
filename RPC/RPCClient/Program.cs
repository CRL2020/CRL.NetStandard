using CRL.Core.Extension;
using CRL.Core.Remoting;
using CRL.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var clientConnect = new RPCClientConnect("127.0.0.1", 805);
            clientConnect.UseSign();
            var service = clientConnect.GetClient<ITestService>();
        label1:
            TestFactory.RunTest(service);
            Console.ReadLine();
            goto label1;
        }
    }

}
