using CRL.Core.Extension;
using CRL.Core.Remoting;
using CRL.Core.Request;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.DynamicWebApi
{
    class ApiClient: AbsClient
    {
        public ApiClient(AbsClientConnect _clientConnect) : base(_clientConnect)
        {
        }
        
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var methodInfo = serviceInfo.GetMethod(binder.Name);
            var id = Guid.NewGuid().ToString();
            var method = ServiceType.GetMethod(binder.Name);
            var methodParamters = method.GetParameters();
            var returnType = method.ReturnType;
            var request = new RequestJsonMessage
            {
                Service = ServiceName,
                Method = binder.Name,
            };
            var allArgs = method.GetParameters();
            request.Args = args.ToList();
            var pollyAttr = serviceInfo.GetAttribute<PollyAttribute>();
            //ResponseJsonMessage response = null;
            #region send
            var hostAddress = HostAddress;
            string url;
            if (!string.IsNullOrEmpty(hostAddress.serviceNamePrefix))
            {     
                //使用网关
                url = hostAddress.GetHttpAddress() + $"/{request.Service}/{request.Method}";
            }
            else
            {
                url = hostAddress.GetHttpAddress() + $"/{serviceInfo.ServiceAttribute.ApiPrefix}/{request.Service}/{request.Method}";
            }

            var httpRequest = new ImitateWebRequest(HostAddress.address, Encoding.UTF8);
            httpRequest.ContentType = "application/json";
            var token = CreateAccessToken(methodParamters, request.Args, clientConnect.TokenInfo);
            httpRequest.SetHead("token", token);

            #endregion
            var json = request.Args.ToJson();
            var asynResult = SendRequestAsync(pollyAttr, httpRequest, url, "POST", json, $"{ServiceName}.{method.Name}", (msg) =>
               {
                   var resMsg = msg.ToObject<ResponseJsonMessage>();
                   if (!resMsg.Success)
                   {
                       ThrowError($"服务端处理错误：{resMsg.Msg}", resMsg.Data);
                   }
                   if (resMsg.Outs != null && resMsg.Outs.Count > 0)
                   {
                       foreach (var kv in resMsg.Outs)
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
                   if (!string.IsNullOrEmpty(resMsg.Token))
                   {
                       clientConnect.TokenInfo.Token = resMsg.Token;
                   }
                   var generType = returnType;
                   if (methodInfo.IsAsync)
                   {
                       generType = returnType.GenericTypeArguments[0];
                   }
                   //转换为实际的数据类型
                   return resMsg.GetData(generType);
               });
            if (returnType == typeof(void))
            {
                result = null;
                return true;
            }
            if (methodInfo.IsAsync)
            {
                var task = methodInfo.TaskCreater();
                task.ResultCreater = async () =>
                {
                    return await asynResult;
                };
                result = task.InvokeAsync();
            }
            else
            {
                result = asynResult.Result;
            }
           
            return true;

        }
    }
}
