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
        ResponseJsonMessage SendRequest(ParameterInfo[] argsName, RequestJsonMessage msg)
        {
            var hostAddress = HostAddress;
            string url;
            if(!string.IsNullOrEmpty(hostAddress.serviceNamePrefix))
            {
                url = hostAddress.GetHttpAddress() + $"/{msg.Service}/{msg.Method}";
            }
            else
            {
                url = hostAddress.GetHttpAddress() + $"/DynamicApi/{msg.Service}/{msg.Method}";
            }
 
            var request = new ImitateWebRequest(HostAddress.address, Encoding.UTF8);
            request.ContentType = "application/json";
            //var token = clientConnect.Token;
            var token = CreateAccessToken(argsName, msg.Args, clientConnect.TokenInfo);

            request.SetHead("token", token);
            var json = msg.Args.ToJson();
            var result = request.Post(url, json);
            return result.ToObject<ResponseJsonMessage>();
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
                //Token = clientConnect.Token.Token
            };
            var allArgs = method.GetParameters();
            request.Args = args.ToList();
            var pollyAttr = serviceInfo.GetAttribute<PollyAttribute>();
            ResponseJsonMessage response = null;
            var pollyData = PollyExtension.Invoke(pollyAttr, () =>
            {
                var res = SendRequest(methodParamters, request);
                return new PollyExtension.PollyData<ResponseJsonMessage>() { Data = res };
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
                //返回Task类型,伪异步
                var task = methodInfo.TaskCreater();
                var result2 = result;
                task.ResultCreater = async () =>
                {
                    return await Task.FromResult(result2);
                };
                result = task.InvokeAsync();
                //var method2 = typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static);
                //var method3 = method2.MakeGenericMethod(new Type[] { generType });
                //var result2 = method3.Invoke(null, new object[] { result });
                //result = result2;
            }
            return true;

        }
    }
}
