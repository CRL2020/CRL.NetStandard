using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace CRL.Core.ConsulClient
{
    public interface IConsulService
    {
        bool RegisterService(ServiceRegistrationInfo service);
        bool DeregisterService(string serviceId);
        Dictionary<string, ServiceInfo> GetAllServices();
        bool Login(string name,string pass);
    }
    public class Consul
    {
        private static Random rng = new Random();
        public string ConsulHost = "127.0.0.1";
        //public int ConsulHttpPort { get; } = 8500;
        protected readonly Request.ImitateWebRequest request = new Request.ImitateWebRequest("ConsulClient");
        protected static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        //protected static readonly MediaTypeHeaderValue MediaJson = new MediaTypeHeaderValue("application/json");
        bool _ocelotGateway;
        public Consul(string host)
        {
            ConsulHost = host;
            request.ContentType = "application/json";
        }
        /// <summary>
        /// 使用ocelot服务发现
        /// </summary>
        /// <param name="gatewayHost"></param>
        public void UseOcelotGatewayDiscover(string gatewayHost)
        {
            ConsulHost = gatewayHost;
            _ocelotGateway = true;
        }

        private bool PutJson(string path, object obj)
        {
            var url = $"{ConsulHost}{path}";
            var json = obj.ToJson();
            var result = request.Put(url, json);
            return true;
        }
        public bool RegisterService(ServiceRegistrationInfo service)
        {
            if (service.Address.Contains("http"))
            {
                service.Address = service.Address.Replace("http://", "");
            }
            var path = _ocelotGateway ? "/consul/RegisterService" : "/v1/agent/service/register";
            return PutJson(path, service);
        }

        public bool DeregisterService(string serviceId)
        {
            var path = _ocelotGateway ? $"/consul/DeregisterService?serviceId={serviceId}" : $"/v1/agent/service/deregister/{serviceId}";
            var url = $"/v1/agent/service/deregister/{serviceId}";
            return PutJson(path, null);
        }
        /// <summary>
        /// 获取所有服务
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ServiceInfo> GetAllServices()
        {
            var url = _ocelotGateway ? $"{ConsulHost}/consul/GetAllServices" : $"{ConsulHost}/v1/agent/services";

            //var url = $"{ConsulHost}/v1/agent/services";
            try
            {
                var result = request.Get(url);
                var services = result.ToObject<Dictionary<string, ServiceInfo>>();
                return services;
            }
            catch(Exception ero)
            {
                throw new Exception($"无法获取consul服务注册,{ero}");
            }
        }
        /// <summary>
        /// 按服务名称,随机返回一个服务地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        public ServiceInfo GetServiceInfo(string serviceName, double minute = 0)
        {
            Dictionary<string, ServiceInfo> all;
            if (minute > 0)
            {
                all = DelegateCache.Init("consulServiceCache", minute, () =>
                 {
                     return GetAllServices();
                 });
            }
            else
            {
                all = GetAllServices();
            }
            var services = all.Values.Where(b => b.Service == serviceName).ToList();
            if (services.Count == 0)
            {
                throw new Exception($"找不到可用的服务:{serviceName}");
            }
            int k = rng.Next(services.Count);
            return services[k];
        }
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
