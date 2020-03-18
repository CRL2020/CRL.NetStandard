using CRL.Core.Remoting;
using Grpc.Core;
using Grpc.Core.Interceptors;
//using Grpc.Net.Client;
using System;
using System.Collections.Generic;

namespace CRL.Grpc.Extend
{
    public class GrpcConnect : IDisposable
    {
        //认证
        //https://www.cnblogs.com/stulzq/p/11897628.html
        /// <summary>
        /// gRPC 拦截器
        /// </summary>
        public Interceptor Interceptor;
        static GrpcConnect()
        {
            //设置允许不安全的HTTP2支持
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }
        string host;
        public GrpcConnect(string _host)
        {
            host = _host;
        }
        /// <summary>
        /// 获取服务地址
        /// </summary>
        internal Func<HostAddress> __GetConsulAgent;
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
            var consulClient = new Core.ConsulClient.Consul(consulUrl);
            //发现consul服务注册,返回服务地址
            __GetConsulAgent = () =>
            {
                var serviceInfo = consulClient.GetServiceInfo(serviceName, cacheMinute);
                return new HostAddress() { address = serviceInfo.Address, port = serviceInfo.Port };
            };
            endPointTimeout = cacheMinute;
        }
        double endPointTimeout;
        DateTime endPointCheckTime = DateTime.Now;
        Channel _channel;
        Channel channel
        {
            get
            {
                var ts = DateTime.Now - endPointCheckTime;
                if (endPointTimeout > 0 && ts.TotalMinutes > endPointTimeout)
                {
                    //_channel?.Dispose();
                    _channel = null;
                    endPointCheckTime = DateTime.Now;
                }
                if (_channel == null)
                {
                    var address = host;
                    if (__GetConsulAgent != null)
                    {
                        var ad = __GetConsulAgent.Invoke();
                        address = $"{ad.address}:{ad.port}";
                    }
                    else
                    {
                        var uri = new Uri(host);
                        address = $"{uri.Host}:{uri.Port}";
                    }
                    var channelOptions = new List<ChannelOption>()
                    {
                        new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                        new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue),
                    };

                    _channel = new Channel(address, ChannelCredentials.Insecure, channelOptions);
                }
                return _channel;
            }
        }
        System.Collections.Concurrent.ConcurrentDictionary<Type, object> instanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();
        public T GetClient<T>()
        {
            var a = instanceCache.TryGetValue(typeof(T), out object instance);
            if (!a)
            {
                if(Interceptor!=null)
                {
                    var invoker = channel.Intercept(Interceptor);
                    instance = System.Activator.CreateInstance(typeof(T), invoker);
                }
                else
                {
                    instance = System.Activator.CreateInstance(typeof(T), channel);
                }
   
                instanceCache.TryAdd(typeof(T), instance);
            }
            return (T)instance;
        }

        public void Dispose()
        {
            _channel?.ShutdownAsync().Wait();
        }
    }
}
