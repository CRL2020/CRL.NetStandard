using CRL.Core.ConsulClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRL.Ocelot
{
    public class ConsulService : Core.Remoting.AbsService, IConsulService
    {
        CRL.Core.ConsulClient.Consul client;
        public ConsulService(IConfiguration configuration)
        {
            var host = $"http://{configuration.GetSection("GlobalConfiguration:ServiceDiscoveryProvider:Host")?.Value}:{configuration.GetSection("GlobalConfiguration:ServiceDiscoveryProvider:Port")?.Value}";
            client = new Core.ConsulClient.Consul(host);
        }
  
        public bool DeregisterService(string serviceId)
        {
            return client.DeregisterService(serviceId);
        }
        public List<CatalogService> GetService(string serviceName, bool passingOnly)
        {
            return client.GetService(serviceName, passingOnly);
        }
        //public Dictionary<string, ServiceInfo> GetAllServices()
        //{
        //    return client.GetAllServices();
        //}
        [CRL.Core.Remoting.LoginPoint()]
        public bool Login(string name, string pass)
        {
            SaveSession(name, pass);
            return true;
        }

        public bool RegisterService(ServiceRegistrationInfo service)
        {
            return client.RegisterService(service);
        }
    }
}
