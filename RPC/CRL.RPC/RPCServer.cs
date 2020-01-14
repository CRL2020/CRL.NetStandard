using CRL.Core.Remoting;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CRL.RPC
{
    /// <summary>
    /// RPC服务端
    /// </summary>
    public class RPCServer: AbsServer
    {
        int port;
        
        ServerBootstrap serverBootstrap;
        IChannel serverChannel { get; set; }
        public RPCServer(int _port)
        {
            port = _port;

            serverBootstrap = new ServerBootstrap()
                .Group(new MultithreadEventLoopGroup(), new MultithreadEventLoopGroup())
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    //数据包最大长度
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast(new ServerHandler(this));
                }));
        }


        public override object InvokeResult(object rq)
        {
            var request = rq as RequestMessage;
            var response = new ResponseMessage();

            try
            {
                var a = serviceHandle.TryGetValue(request.Service, out serviceInfo serviceInfo);
                if (!a)
                {
                    return ResponseMessage.CreateError("未找到该服务", "404");
                }

                var methodInfo = serviceInfo.GetMethod(request.Method);
                if (methodInfo == null)
                {
                    return ResponseMessage.CreateError("未找到该方法" + request.Method, "404");
                }
                var method = methodInfo.MethodInfo;

                var paramters = request.Args;

                var methodParamters = methodInfo.Parameters;

                var args = new object[methodParamters.Length];

                for (int i = 0; i < methodParamters.Length; i++)
                {
                    var p = methodParamters[i];
                    var value = paramters[i];
                    int offSet = 0;
                    args[i] = Core.BinaryFormat.FieldFormat.UnPack(p.ParameterType, value, ref offSet);
                }

                var msgBase = new Core.Remoting.MessageBase() { Args = args.ToList(), Method = request.Method, Service = request.Service, Token = request.Token };
                var errorInfo = InvokeMessage(msgBase, out object result, out Dictionary<int, object> outs2, out string token);
                if (errorInfo != null)
                {
                    return ResponseMessage.CreateError(errorInfo.msg, errorInfo.code);
                }
                response.SetData(method.ReturnType, result);
                response.Success = true;

                var outs = new Dictionary<int, byte[]>();
                foreach (var kv in outs2)
                {
                    var type = methodParamters[kv.Key];
                    var value = kv.Value;
                    outs[kv.Key] = Core.BinaryFormat.FieldFormat.Pack(type.ParameterType, value);
                }

                response.Outs = outs;
                if (!string.IsNullOrEmpty(token))//登录方法后返回新TOKEN
                {
                    response.Token = token;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Msg = ex.InnerException?.Message;
                Console.WriteLine(ex.ToString());
                return ResponseMessage.CreateError(ex.InnerException?.Message + $" 在{request.Service}/{request.Method}", "500");
            }

            return response;
        }

        public override void Start()
        {
            serverChannel = serverBootstrap.BindAsync(port).Result;
            Console.WriteLine("RPCServer start at "+ port);
        }
        public override void Dispose()
        {
            serverChannel.CloseAsync();
        }
    }
}
