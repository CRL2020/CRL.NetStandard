#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CRL.Core.ApiProxy
{
    public static class Extensions
    {
        static ConcurrentDictionary<string, ApiClientConnect> ClientConnects = new ConcurrentDictionary<string, ApiClientConnect>();
#if NETSTANDARD
        /// <summary>
        /// 注册API
        /// </summary>
        /// <param name="host"></param>
        public static void RegisterApiHost(this IServiceCollection services, HostInfo host, string consulUrl = "")
        {
            RegisterApiHost(host, consulUrl);
        }


        static void RegisterApiClient<T>(this IServiceCollection services, Func<string, Dictionary<string, object>> headAction = null) where T : class
        {
            services.AddTransient(s =>
            {
                return GetClient<T>(headAction);
            });
        }

        public static void RegisterApiClient(this IServiceCollection services, Func<string, Dictionary<string, object>> headAction, params Assembly[] assemblies)
        {
            foreach (var assembyle in assemblies)
            {
                var types = assembyle.GetTypes();
                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<ServiceAttribute>();
                    if (attr == null)
                    {
                        continue;
                    }
                    var method = typeof(Extensions).GetMethod(nameof(Extensions.RegisterApiClient), BindingFlags.NonPublic | BindingFlags.Static);
                    var result = method.MakeGenericMethod(new Type[] { type }).Invoke(null, new object[] { services, headAction });
                }
            }
        }

#endif
        public static ApiClientConnect RegisterApiHost(HostInfo host, string consulUrl = "")
        {
            var hostName = host.Name;
            var a = ClientConnects.TryGetValue(hostName, out var clientConnect);
            if (!a)
            {
                clientConnect = new ApiClientConnect(host.Url);
                if (host.UseConsul)
                {
                    clientConnect.UseConsulDiscover(consulUrl, host.Name);
                }
                if (host.UseConsoleLog)
                {
                    clientConnect.UseAfterRequest((url, content) =>
                    {
                        Console.WriteLine($"{url} response is {content}");
                    });
                }
                clientConnect.OnError = (e, c) =>
                {
                    Console.WriteLine(e);
                };
                ClientConnects.TryAdd(hostName, clientConnect);
            }
            return clientConnect;
        }
        public static T GetClient<T>(Func<string, Dictionary<string, object>> headAction = null) where T : class
        {
            var attr = typeof(T).GetCustomAttribute<ServiceAttribute>();
            var hostUrl = attr?.HostUrl;
            var hostName = attr?.HostName;
            if (string.IsNullOrEmpty(hostName) && string.IsNullOrEmpty(hostUrl))
            {
                throw new Exception("未使用ServiceAttribute指定HostName或hostUrl");
            }
            ApiClientConnect clientConnect;
            if (!string.IsNullOrEmpty(hostUrl))
            {
                var host = new HostInfo(hostUrl, hostUrl);
                clientConnect = RegisterApiHost(host);
            }
            else
            {
                var a = ClientConnects.TryGetValue(hostName, out clientConnect);
                if (!a)
                {
                    throw new Exception($"未找到对应的ApiClientConnect {hostName}");
                }
            }
            return clientConnect.GetClient<T>(headAction?.Invoke(hostName));
        }
    }
    #region obj
    public class HostInfo
    {
        public HostInfo(string name, string url)
        {
            Name = name;
            Url = url;
        }
        public string Name;
        public string Url;
        public bool UseConsul;
        public bool UseConsoleLog;
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    #endregion
}
