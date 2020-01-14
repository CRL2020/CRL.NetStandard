using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public abstract class AbsClientConnect : IDisposable
    {
        public string Token = "";
        public Action<string, string> OnError;

        internal bool __UseSign = false;
        /// <summary>
        /// 使用签名
        /// 参数都会以ToString计算,注意类型问题
        /// </summary>
        public void UseSign()
        {
            __UseSign = true;
        }
        protected Dictionary<string, object> _services = new Dictionary<string, object>();
        public virtual void Dispose()
        {

        }
        public abstract T GetClient<T>() where T : class;
        #region consul
        /// <summary>
        /// 获取服务地址
        /// </summary>
        internal Func<HostAddress> __GetConsulAgent;
        /// <summary>
        /// 网关地址
        /// </summary>
        internal string __GatewayUrl;
        /// <summary>
        /// 是否使用网关
        /// </summary>
        internal bool __UseGateway;
        [Obsolete("使用UseConsulDiscover")]
        public void UseConsulAgent(string consulUrl, string serviceName, double cacheMinute = 0.5)
        {
            UseConsulDiscover(consulUrl, serviceName, cacheMinute);
        }
        /// <summary>
        /// 使用consul服务发现
        /// </summary>
        /// <param name="consulUrl"></param>
        /// <param name="serviceName"></param>
        /// <param name="cacheMinute"></param>
        public void UseConsulDiscover(string consulUrl, string serviceName, double cacheMinute = 0.5)
        {
            if (string.IsNullOrEmpty(consulUrl))
            {
                throw new ArgumentNullException("consulUrl");
            }
            var consulClient = new ConsulClient.Consul(consulUrl);
            //发现consul服务注册,返回服务地址
            __GetConsulAgent = () =>
            {
                var serviceInfo = consulClient.GetServiceInfo(serviceName, cacheMinute);
                return new HostAddress() { address = serviceInfo.Address, port = serviceInfo.Port };
            };
        }
        /// <summary>
        /// 直接使用OcelotAPI网关
        /// </summary>
        /// <param name="gatewayUrl"></param>
        /// <param name="serviceNamePrefix"></param>
        /// <param name="useGateway"></param>
        /// <param name="cacheMinute"></param>
        public void UseOcelotApiGateway(string gatewayUrl)
        {
            if (string.IsNullOrEmpty(gatewayUrl))
            {
                throw new ArgumentNullException("gatewayUrl");
            }
            __GatewayUrl = gatewayUrl;
            __UseGateway = true;
            __GetConsulAgent = () =>
            {//使用网关,直接返回网关地址
                return new HostAddress() { address = __GatewayUrl, port = 0 };
            };
        }
        /// <summary>
        /// 使用OcelotAPI网关服务发现
        /// </summary>
        /// <param name="gatewayUrl"></param>
        /// <param name="serviceName"></param>
        /// <param name="cacheMinute"></param>
        public void UseOcelotApiGatewayDiscover(string gatewayUrl, string serviceName, double cacheMinute = 0.5)
        {
            if (string.IsNullOrEmpty(gatewayUrl))
            {
                throw new ArgumentNullException("gatewayUrl");
            }
            __GatewayUrl = gatewayUrl;
            var gatewayClient = new ConsulClient.ConsulGateway(gatewayUrl);
            __GetConsulAgent = () =>
            {
                //使用真实服务地址
                var serviceInfo = gatewayClient.GetServiceInfo(serviceName, cacheMinute);
                return new HostAddress() { address = serviceInfo.Address, port = serviceInfo.Port };
            };
        }

        #endregion
    }
}
