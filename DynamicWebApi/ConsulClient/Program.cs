using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsulClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientConnect = new CRL.DynamicWebApi.ApiClientConnect("http://localhost:3400");
            var consulClient = clientConnect.GetClient<CRL.Core.ConsulClient.IConsulService>();
            var hostAddress ="127.0.0.1";
            var info = new CRL.Core.ConsulClient.ServiceRegistrationInfo
            {
                Address = hostAddress,
                Name = "DataSyncApi",
                ID = "DataSyncApi",
                Port = 801,
                Tags = new[] { "v1" },
                Check = new CRL.Core.ConsulClient.CheckRegistrationInfo()
                {
                    HTTP = $"http://{hostAddress}:801",
                    Interval = "10s",
                    DeregisterCriticalServiceAfter = "90m"
                }
            };
            consulClient.Login("test","");
            var a = consulClient.RegisterService(info);
            Console.ReadLine();
        }
    }
}
