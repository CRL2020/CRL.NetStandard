using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRL.Core.ConsulClient
{
    public class Client:IDisposable
    {
        private static Random rng = new Random();
        public string ConsulHost { get; } = "127.0.0.1";
        public IConfiguration Configuration { get; }
        protected static readonly MediaTypeHeaderValue MediaJson = new MediaTypeHeaderValue("application/json");
        protected readonly HttpClient httpClient = new HttpClient();
        public Client(IConfiguration configuration)
        {
            Configuration = configuration;
            var host = $"http://{configuration.GetSection("GlobalConfiguration:ServiceDiscoveryProvider:Host")?.Value}:{configuration.GetSection("GlobalConfiguration:ServiceDiscoveryProvider:Port")?.Value}";
            ConsulHost = host;
            httpClient.BaseAddress = new Uri(host);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        private async Task<bool> HandleResponse(Task<HttpResponseMessage> responseTask)
        {
            var response = await responseTask;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            var responseText = await response.Content.ReadAsStringAsync();
            //Log.Warn($"{response.RequestMessage.RequestUri} => {response.RequestMessage.Method} failed : ({(int)response.StatusCode}) {responseText}");
            return false;
        }

        private async Task<bool> PutJson(string path, object obj)
        {
            var content = new StringContent(JsonSerializer.Serialize(obj));
            content.Headers.ContentType = MediaJson;
            return await HandleResponse(httpClient.PutAsync(path, content));

        }
        public async Task<bool> RegisterService(ServiceRegistrationInfo service)
        {
            if (service.Address.Contains("http"))
            {
                service.Address = service.Address.Replace("http://", "");
            }
            return await PutJson("/v1/agent/service/register", service);
        }

        public async Task<bool> DeregisterService(string serviceId)
        {
            var url = $"/v1/agent/service/deregister/{serviceId}";
            return await PutJson(url, null);
        }
        /// <summary>
        /// 获取所有服务
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ServiceInfo> GetAllServices()
        {
            var url = $"{ConsulHost}/v1/agent/services";
            try
            {
                var result = httpClient.GetStringAsync(url).Result;
                var services =JsonSerializer.Deserialize<Dictionary<string, ServiceInfo>>(result);
                return services;
            }
            catch (Exception ero)
            {
                throw new Exception($"无法获取consul服务注册,{ero}");
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
