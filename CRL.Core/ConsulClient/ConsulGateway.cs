using CRL.Core.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ConsulClient
{
    public class ConsulGateway
    {
        private static Random rng = new Random();
        public string ConsulHost { get; } = "127.0.0.1";
        //public int ConsulHttpPort { get; } = 8500;
        protected readonly Request.ImitateWebRequest request = new Request.ImitateWebRequest("ConsulGateway");
        protected static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        //protected static readonly MediaTypeHeaderValue MediaJson = new MediaTypeHeaderValue("application/json");

        public ConsulGateway(string host)
        {
            ConsulHost = host;
            request.ContentType = "application/json";
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
            return PutJson("/consul/RegisterService", service);
        }

        public bool DeregisterService(string serviceId)
        {
            var url = $"/consul/DeregisterService/{serviceId}";
            return PutJson(url, null);
        }
        /// <summary>
        /// 获取所有服务
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ServiceInfo> GetAllServices()
        {
            var url = $"{ConsulHost}/consul/GetAllServices";
            try
            {
                var result = request.Get(url);
                var services = result.ToObject<Dictionary<string, ServiceInfo>>();
                return services;
            }
            catch (Exception ero)
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
