using CRL.Core.Remoting;
using CRL.Core.Request;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ApiProxy
{
    public class ApiClientConnect: AbsClientConnect
    {
        string host;

        internal Action<ImitateWebRequest, Dictionary<string, object>,string> OnBeforRequest;
        internal string Apiprefix = "api";
        internal Encoding Encoding = Encoding.UTF8;
        /// <summary>
        /// 发送前处理
        /// </summary>
        public ApiClientConnect UseBeforRequest(Action<ImitateWebRequest, Dictionary<string, object>,string> action)
        {
            OnBeforRequest = action;
            return this;
        }

        public ApiClientConnect(string _host)
        {
            host = _host;
        }
        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public ApiClientConnect SetEncoding(Encoding encoding)
        {
            Encoding = encoding;
            return this;
        }
        /// <summary>
        /// 直接使用网关时,传入服务调用前辍
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gatewayPrefix"></param>
        /// <returns></returns>
        public T GetClient<T>(string gatewayPrefix) where T : class
        {
            var type = typeof(T);
            var serviceName = type.Name;
            var key = string.Format("{0}_{1}", host, serviceName);
            var a = _services.TryGetValue(key, out object instance);
            if (a)
            {
                return instance as T;
            }
            var info = serviceInfo.GetServiceInfo(type);
            var client = new ApiClient(this)
            {
                HostAddress = new HostAddress() { address = host, serviceNamePrefix = gatewayPrefix },
                serviceInfo = info,
            };
            //创建代理
            instance = client.ActLike<T>();
            _services[key] = instance;
            return instance as T;
        }
        public override T GetClient<T>()
        {
            return GetClient<T>("");
        }
    }
}
