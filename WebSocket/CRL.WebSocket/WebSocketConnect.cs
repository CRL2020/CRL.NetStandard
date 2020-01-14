using CRL.Core.Remoting;
using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using ImpromptuInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CRL.WebSocket
{
    public class WebSocketClientConnect : AbsClientConnect
    {
        string host;
        int port;
        IChannel channel;
        IEventLoopGroup group;
        static ResponseWaits allWaits = new ResponseWaits();

        public WebSocketClientConnect(string _host,int _port)
        {
            host = _host;
            port = _port;
            Start();
        }
        public override T GetClient<T>()
        {
            var type = typeof(T);
            var serviceName = typeof(T).Name;
            var key = string.Format("{0}_{1}", host, serviceName);
            var a = _services.TryGetValue(key, out object instance);
            if (a)
            {
                return instance as T;
            }
            var info = serviceInfo.GetServiceInfo(type);
            var client = new WebSocketClient(this)
            {
                HostAddress = new HostAddress() { address=host},
                serviceInfo = info,
            };
            //创建代理
            instance = client.ActLike<T>();
            _services[key] = instance;
            return instance as T;
        }
        internal void OnMessage(ResponseJsonMessage msg)
        {
            var a = subs.TryGetValue(msg.MsgType, out methodType method);
            if (a)
            {
                var dg = method.action;
                var data = msg.GetData(method.type);
                dg.DynamicInvoke(data);
            }
        }

        CRL.Core.ThreadWork pingWork;
        void Start()
        {
            var builder = new UriBuilder
            {
                Scheme = "ws",
                Host = host,
                Port = port
            };

            string path = "websocket";
            if (!string.IsNullOrEmpty(path))
            {
                builder.Path = path;
            }

            Uri uri = builder.Uri;


            IEventLoopGroup group;
            group = new MultithreadEventLoopGroup();

            X509Certificate2 cert = null;
            string targetHost = null;
            //if (ClientSettings.IsSsl)
            //{
            //    cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //    targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            //}
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Option(ChannelOption.TcpNodelay, true);
            bootstrap.Channel<TcpSocketChannel>();

            // Connect with V13 (RFC 6455 aka HyBi-17). You can change it to V08 or V00.
            // If you change it to V00, ping is not supported and remember to change
            // HttpResponseDecoder to WebSocketHttpResponseDecoder in the pipeline.
            var handler = new WebSocketClientHandler(
                WebSocketClientHandshakerFactory.NewHandshaker(
                        uri, WebSocketVersion.V13, null, true, new DefaultHttpHeaders()), allWaits, this);

            bootstrap.Handler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                //if (cert != null)
                //{
                //    pipeline.AddLast("tls", new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                //}

                pipeline.AddLast(
                    new HttpClientCodec(),
                    new HttpObjectAggregator(8192),
                    WebSocketClientCompressionHandler.Instance,
                    handler);
            }));

            channel = bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port)).Result;
            handler.HandshakeCompletion.Wait();

            Console.WriteLine("WebSocket handshake completed.");

            pingWork = new CRL.Core.ThreadWork();
        }
        /// <summary>
        /// 开启ping保持连接
        /// </summary>
        public void StartPing()
        {
            pingWork.Start("sendPing", () =>
            {
                var frame = new PingWebSocketFrame(Unpooled.WrappedBuffer(new byte[] { 8, 1, 8, 1 }));
                channel.WriteAndFlushAsync(frame);
                //Console.WriteLine("sendPing");
                return true;
            }, 3);
        }
        internal ResponseJsonMessage SendRequest(RequestJsonMessage msg)
        {
            var id = Guid.NewGuid().ToString();
            allWaits.Add(id);
            msg.MsgId = id;
            WebSocketFrame frame = new TextWebSocketFrame(msg.ToBuffer());
            if (!channel.Active)
            {
                //ThrowError("服务端已断开连接", "500");
            }
            channel.WriteAndFlushAsync(frame);

            //等待返回
            var response = allWaits.Wait(id).Response;
            return response;
        }

        public override void Dispose()
        {
            pingWork?.Stop();
            channel.CloseAsync().Wait();
            group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)).Wait();
        }
        Dictionary<string, methodType> subs = new Dictionary<string, methodType>();
        public void SubscribeMessage<T>(Action<T> action) where T : class
        {
            var name = typeof(T).Name;
            subs.Add(name, new methodType() { action = action, type = typeof(T) });
        }
        class methodType
        {
            public Delegate action;
            public Type type;
        }
    }
}
