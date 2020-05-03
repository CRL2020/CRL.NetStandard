using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRL.Core.ConsulClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.Configuration.File;
using Ocelot.Provider.Consul;

namespace CRL.Ocelot.Controllers
{
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    //[Authorize()]
    public class ConsulController : ControllerBase
    {
        CRL.Core.ConsulClient.Consul client;
        public ConsulController(IConfiguration configuration)
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

        public bool RegisterService(ServiceRegistrationInfo service)
        {
            return client.RegisterService(service);
        }
    }
}
