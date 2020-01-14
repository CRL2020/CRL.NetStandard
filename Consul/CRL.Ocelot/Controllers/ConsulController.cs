using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using CRL.Core.ConsulClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly Client _client;
        private readonly ILogger<ConsulController> _logger;

        public ConsulController(ILogger<ConsulController> logger, Client client)
        {
            _logger = logger;
            _client = client;
        }
        [HttpPut]
        public async Task<bool> RegisterService([FromBody]ServiceRegistrationInfo service)
        {
            return await _client.RegisterService(service);
        }
        [HttpPut]
        public async Task<bool> DeregisterService([FromQuery]string serviceId)
        {
            return await _client.DeregisterService(serviceId);
        }
        [HttpGet]
        public Dictionary<string, ServiceInfo> GetAllServices()
        {
            return _client.GetAllServices();
        }
    }
    //[Produces("application/json")]
    //[Route("[controller]/[action]")]
    //public class ConsulController : ControllerBase
    //{
    //    private readonly IConsulClient _consul;
    //    public ConsulController(IOptions<FileConfiguration> fileConfiguration, IConsulClientFactory clientFactory)
    //    {
    //        var serviceDiscoveryProvider = fileConfiguration.Value.GlobalConfiguration.ServiceDiscoveryProvider;
    //        var _configurationKey = string.IsNullOrWhiteSpace(serviceDiscoveryProvider.ConfigurationKey) ? "InternalConfiguration" :
    //            serviceDiscoveryProvider.ConfigurationKey;

    //        var config = new ConsulRegistryConfiguration(serviceDiscoveryProvider.Host,
    //            serviceDiscoveryProvider.Port, _configurationKey, serviceDiscoveryProvider.Token);
    //        _consul = clientFactory.Get(config);
    //    }
    //    [HttpPut]
    //    public async Task<WriteResult> RegisterService(AgentServiceRegistration service)
    //    {
    //        return await _consul.Agent.ServiceRegister(service);
    //    }
    //    [HttpPut]
    //    public async Task<WriteResult> DeregisterService(string serviceId)
    //    {
    //        return await _consul.Agent.ServiceDeregister(serviceId);
    //    }
    //    [HttpGet]
    //    public Dictionary<string, AgentService> GetAllServices()
    //    {
    //        return _consul.Agent.Services().Result.Response;
    //    }
    //}
}
