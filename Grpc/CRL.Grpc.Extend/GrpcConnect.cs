using CRL.Core.Remoting;
using Grpc.Core;
using Grpc.Core.Interceptors;
//using Grpc.Net.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CRL.Grpc.Extend
{
    public interface IGrpcConnect: IDisposable
    {
        T GetClient<T>();
        void SetMetadata(Metadata metadata);
        GrpcClientOptions GetOptions();
        Channel GetChannel();
        Metadata GetMetadata();
    }
    /// <summary>
    /// gRPC扩展,支持consul服务发现,polly策略
    /// </summary>
    public class GrpcConnect : IGrpcConnect
    {
        GrpcClientOptions _options;
#if NETSTANDARD
      
        public GrpcConnect(Microsoft.Extensions.Options.IOptions<GrpcClientOptions> options)
        {
            _options = options.Value;
            InitOptions();
        }
#else

#endif
        public GrpcConnect(GrpcClientOptions options)
        {
            _options = options;
            InitOptions();
        }
        public GrpcClientOptions GetOptions()
        {
            return _options;
        }
        bool firstCheck = true;
        void InitOptions()
        {
            if(_options.UseConsul)
            {
                updateSerivceStatus();
                threadWork = new Core.ThreadWork();
                threadWork.Start("consulServiceCheck", () =>
                {
                    if (firstCheck)
                    {
                        firstCheck = false;
                        return true;
                    }
                    updateSerivceStatus();
                    return true;
                }, 10);
            }
            else
            {
                var address = $"{_options.Host}:{_options.Port}";
                channelCache.TryAdd("1", new ChannelObj() { channel = CreateChannel(address) });
            }
        }

        CRL.Core.ThreadWork threadWork;
        static object lockObj = new object();
        void updateSerivceStatus()
        {
            var consulClient = new Core.ConsulClient.Consul(_options.ConsulUrl);
            //发现consul服务注册,返回服务地址
            var allService = consulClient.GetService(_options.ConsulServiceName,true);
            foreach (var s in allService)
            {
                var a = channelCache.TryGetValue(s.ServiceID, out ChannelObj channel);
                if (!a)
                {
                    var address = $"{s.ServiceAddress}:{s.ServicePort}";
                    channelCache.TryAdd(s.ServiceID, new ChannelObj() { channel = CreateChannel(address), expTime = DateTime.Now.AddMinutes(5) });
                }
                else
                {
                    channel.expTime = DateTime.Now.AddMinutes(5);
                }
            }
            lock (lockObj)
            {
                foreach (var kv in new ConcurrentDictionary<string, ChannelObj>(channelCache))
                {
                    if (kv.Value.expired)
                    {
                        channelCache.TryRemove(kv.Key, out ChannelObj obj);
                    }
                }
            }
        }

        class ChannelObj
        {
            public Channel channel;
            public DateTime expTime;
            public bool expired
            {
                get
                {
                    return DateTime.Now > expTime;
                } 
            }
        }
        private static Random rng = new Random();
        ConcurrentDictionary<string, ChannelObj> channelCache = new ConcurrentDictionary<string, ChannelObj>();

        public Channel GetChannel()
        {
            var list = channelCache.Values.ToArray();
            if (list.Count() == 0)
            {
                throw new Exception($"没有找到可用的注册服务:{_options.ConsulServiceName}");
            }
            var k = rng.Next(list.Count());
            return list[k].channel;
        }
        Channel CreateChannel(string address)
        {
            var channelOptions = new List<ChannelOption>()
                    {
                        new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                        new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue),
                    };
            var channel = new Channel(address, ChannelCredentials.Insecure, channelOptions);
            return channel;
        }
        ConcurrentDictionary<Type, object> instanceCache = new ConcurrentDictionary<Type, object>();
        public T GetClient<T>()
        {
            var a = instanceCache.TryGetValue(typeof(T), out object instance);
            if (!a)
            {
                var grpcCallInvoker = new GRpcCallInvoker(this);
                instance = System.Activator.CreateInstance(typeof(T), grpcCallInvoker);
                instanceCache.TryAdd(typeof(T), instance);
            }
            return (T)instance;
        }
        Metadata metadata;
        public Metadata GetMetadata()
        {
            return metadata;
        }
        public void SetMetadata(Metadata _metadata)
        {
            metadata = _metadata;
        }
        public void Dispose()
        {
            foreach (var kv in channelCache)
            {
                kv.Value.channel.ShutdownAsync().Wait();
            }
            threadWork?.Stop();
        }
    }
}
