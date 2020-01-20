using CRL.Core.Extension;
using CRL.Core.Remoting;
using CRL.Core.Request;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CRL.WebSocket
{
    class WebSocketClient : AbsClient
    {
        public WebSocketClient(AbsClientConnect _clientConnect) : base(_clientConnect)
        {

        }


        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var id = Guid.NewGuid().ToString();
            var method = ServiceType.GetMethod(binder.Name);
            var returnType = method.ReturnType;
            var request = new RequestJsonMessage
            {
                Service = ServiceName,
                Method = binder.Name,
                //Token = clientConnect.Token
            };
            var allArgs = method.GetParameters();
            var token = request.Token;
            request.Token = CreateAccessToken(allArgs, args.ToList(), clientConnect.TokenInfo);
            request.Args = args.ToList();
            ResponseJsonMessage response = null;
            try
            {
                response = ((WebSocketClientConnect)clientConnect).SendRequest(request);
            }
            catch (Exception ero)
            {
                ThrowError(ero.Message, "500");
            }
            if (response == null)
            {
                ThrowError("请求超时未响应", "500");
            }
            if (!response.Success)
            {
                ThrowError($"服务端处理错误：{response.Msg}", response.Data);
            }
            if (response.Outs != null && response.Outs.Count > 0)
            {
                foreach (var kv in response.Outs)
                {
                    var p = allArgs[kv.Key];
                    var value = kv.Value;
                    if (p.Name.EndsWith("&"))
                    {
                        var name = p.Name.Replace("&", "");
                        var type2 = Type.GetType(name);
                        value = value.ToString().ToObject(type2);
                    }
                    args[kv.Key] = value;
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
            result = response.GetData(returnType);

            return true;

        }
    }
}
