
using CRL.Core.Extension;
using CRL.Core.Remoting;
using System;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Common;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using DotNetty.Codecs.Http.WebSockets;

namespace CRL.WebSocket
{
    public class WebSocketServer: AbsServer
    {
        IChannel bootstrapChannel;
        ServerBootstrap bootstrap;
        int port;
        public WebSocketServer(int _port)
        {
            port = _port;
            
        }
        public override void Start()
        {
            Console.WriteLine(
                $"\n{RuntimeInformation.OSArchitecture} {RuntimeInformation.OSDescription}"
                + $"\n{RuntimeInformation.ProcessArchitecture} {RuntimeInformation.FrameworkDescription}"
                + $"\nProcessor Count : {Environment.ProcessorCount}\n");

      
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            Console.WriteLine($"Server garbage collection : {(GCSettings.IsServerGC ? "Enabled" : "Disabled")}");
            Console.WriteLine($"Current latency mode for garbage collection: {GCSettings.LatencyMode}");
            Console.WriteLine("\n");

            IEventLoopGroup bossGroup;
            IEventLoopGroup workGroup;
            bossGroup = new MultithreadEventLoopGroup(1);
            workGroup = new MultithreadEventLoopGroup();

            X509Certificate2 tlsCertificate = null;
            //if (ServerSettings.IsSsl)
            //{
            //    tlsCertificate = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //}
            try
            {
                bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workGroup);

                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536));
                        pipeline.AddLast(new WebSocketServerHandler(this));
                    }));

                bootstrapChannel =  bootstrap.BindAsync(IPAddress.Loopback, port).Result;

                Console.WriteLine("Open your web browser and navigate to "
                    + $"{(false ? "https" : "http")}"
                    + $"://127.0.0.1:{port}/");
                Console.WriteLine("Listening on "
                    + $"{(false ? "wss" : "ws")}"
                    + $"://127.0.0.1:{port}/websocket");
                //Console.ReadLine();

                //bootstrapChannel.CloseAsync().Wait();
            }
            finally
            {
                //workGroup.ShutdownGracefullyAsync().Wait();
                //bossGroup.ShutdownGracefullyAsync().Wait();
            }

        }
        public override void Dispose()
        {
            bootstrapChannel.CloseAsync().Wait();
        }
        public override object InvokeResult(object rq)
        {
            return InvokeResult2(null, rq);
        }
        public object InvokeResult2(IChannelHandlerContext ctx, object rq)
        {
            var request = rq as RequestJsonMessage;
            var response = new ResponseJsonMessage();

            try
            {
                var msgBase = new Core.Remoting.MessageBase() { Args = request.Args, Method = request.Method, Service = request.Service, Token = request.Token };
                var errorInfo = InvokeMessage(msgBase, out object result, out Dictionary<int, object> outs, out string token);
                if (errorInfo != null)
                {
                    return ResponseJsonMessage.CreateError(errorInfo.msg, errorInfo.code);
                }
                response.SetData(result);
                response.Success = true;
                response.Outs = outs;
                if (!string.IsNullOrEmpty(token))//登录方法后返回新TOKEN
                {
                    response.Token = token;
                    var arry = response.Token.Split('@');
                    AddClient(ctx, arry[0]);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Msg = ex.InnerException?.Message;
                Console.WriteLine(ex.ToString());
                CRL.Core.EventLog.Log(ex.ToString(), request.Service);
                return ResponseJsonMessage.CreateError(ex.InnerException?.Message + $" 在{request.Service}/{request.Method}", "500");
            }
 
            return response;
        }

        Dictionary<string, IChannelHandlerContext> clientKeys = new Dictionary<string, IChannelHandlerContext>();
        internal void AddClient(IChannelHandlerContext ctx,string key)
        {
            Console.WriteLine("AddClient:"+ctx.Channel.Id);
            clientKeys.Remove(key);
            clientKeys.Add(key, ctx);
        }
        internal void RemoveClient(IChannelHandlerContext ctx)
        {
            Console.WriteLine("RemoveClient:" + ctx.Channel.Id);
            string key="!!!";
            foreach (var item in clientKeys)
            {
                if (item.Value.Channel.Id == ctx.Channel.Id)
                {
                    key = item.Key;
                    break;
                }
            }
            //clients.Remove(ctx.Channel.Id);
            clientKeys.Remove(key);
        }
        public bool SendMessage<T>(string clientKey, T msg, out string error)
        {
            error = "";
            var a = clientKeys.TryGetValue(clientKey, out IChannelHandlerContext ctx);
            if (!a)
            {
                error = "找不到客户端连接";
                return false;
            }
            var response = new ResponseJsonMessage() { Data = msg.ToJson(), MsgType = typeof(T).Name };
            var frame2 = new TextWebSocketFrame(response.ToJson());
            ctx.WriteAndFlushAsync(frame2);
            return true;
        }
    }
    class clientInfo
    {
        public string key;
        public IChannelHandlerContext ctx;
    }
}
