﻿
using CRL.Core.Remoting;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.RPC
{
    class RPCClient : AbsClient
    {
        //public int Port;
        static Bootstrap bootstrap;
        IChannel channel = null;

        static ResponseWaits allWaits = new ResponseWaits();
        public RPCClient(AbsClientConnect _clientConnect) : base(_clientConnect)
        {
            bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    //数据包最大长度
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast(new ClientHandler(allWaits));
                }));
        }
 

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                if (channel == null || !channel.Open)
                {
                    channel = Core.AsyncInvoke.RunSync(() => bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(HostAddress.address), HostAddress.port)));
                }
            }
            catch(Exception ero)
            {
                ThrowError("连接服务端失败:" + ero, "500");
            }
            var id = Guid.NewGuid().ToString();
            var method = ServiceType.GetMethod(binder.Name);
            allWaits.Add(id);
            var request = new RequestMessage
            {
                MsgId = id,
                Service = ServiceName,
                Method = binder.Name,
                //Token = clientConnect.Token.Token
            };
            var dic = new List<byte[]>();
            var allArgs = method.GetParameters();
 
            for (int i = 0; i < allArgs.Length; i++)
            {
                var p = allArgs[i];
                dic.Add(Core.BinaryFormat.FieldFormat.Pack(p.ParameterType, args[i]));
            }
            request.Args = dic;
            var token = request.Token;
            request.Token = CreateAccessToken(allArgs, args.ToList(), clientConnect.TokenInfo);
            var pollyAttr = serviceInfo.GetAttribute<PollyAttribute>();
            ResponseMessage response = null;

            var pollyData = PollyExtension.Invoke(pollyAttr, () =>
            {
                channel.WriteAndFlushAsync(request.ToBuffer());
                //等待返回
                var res = allWaits.Wait(id).Response;
                return new PollyExtension.PollyData<ResponseMessage>() { Data = res };
            }, $"{ServiceName}.{method.Name}");
            response = pollyData.Data;
            if (!string.IsNullOrEmpty(pollyData.Error))
            {
                ThrowError(pollyData.Error, "500");
            }

            if (response == null)
            {
                ThrowError("请求超时未响应", "500");
            }
            if (!response.Success)
            {
                ThrowError($"服务端处理错误：{response.Msg}", response.GetData(typeof(string)) + "");
            }
            var returnType = method.ReturnType;
            if (response.Outs != null && response.Outs.Count > 0)
            {
                foreach (var kv in response.Outs)
                {
                    var index = kv.Key;
                    var type = allArgs[index];
                    //args[(int)find] = kv.Value;
                    int offSet = 0;
                    args[index] = Core.BinaryFormat.FieldFormat.UnPack(type.ParameterType, kv.Value, ref offSet);
                }
            }
            if (!string.IsNullOrEmpty(response.Token))
            {
                clientConnect.TokenInfo.Token = response.Token;
            }
            if (returnType == typeof(void))
            {
                result = null;
                return true;
            }
            var generType = returnType;
            bool isTask = false;
            if (returnType.Name.StartsWith("Task`1"))
            {
                generType = returnType.GenericTypeArguments[0];
                isTask = true;
            }
            result = response.GetData(generType);
            if (isTask)
            {
                //返回Task类型
                var method2 = typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static);
                var result2 = method2.MakeGenericMethod(new Type[] { generType }).Invoke(null, new object[] { result });
                result = result2;
            }
            return true;
        }
        public override void Dispose()
        {
            if (channel != null)
            {
                channel.CloseAsync();
            }
        }
    }
}
