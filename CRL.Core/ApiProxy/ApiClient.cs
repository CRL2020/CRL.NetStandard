using CRL.Core.Extension;
using CRL.Core.Remoting;
using CRL.Core.Request;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ApiProxy
{
    class ApiClient : AbsClient
    {
        public ApiClient(AbsClientConnect _clientConnect) : base(_clientConnect)
        {

        }
        static Dictionary<ContentType, string> ContentTypeDic = new Dictionary<ContentType, string>() { { ContentType.JSON, "application/json" },
            { ContentType.XML, "application/xml" },
            { ContentType.FORM, "application/x-www-form-urlencoded" },
            { ContentType.NONE, "text/plain" },
        };
        object SendRequest(serviceInfo serviceInfo, methodInfo method, object[] args)
        {
            //var method = serviceInfo.GetMethod(methodName);
            var serviceAttribute = serviceInfo.GetAttribute<ServiceAttribute>();
            var methodAttribute = method.GetAttribute<MethodAttribute>();
            var argsName = method.MethodInfo.GetParameters();
            var returnType = method.MethodInfo.ReturnType;
            var contentType = ContentType.JSON;
            var serviceName = serviceInfo.ServiceType.Name;
            var hostAddress = HostAddress;
            if (serviceAttribute != null && serviceAttribute.ContentType != ContentType.NONE)
            {
                contentType = serviceAttribute.ContentType;
                if(!string.IsNullOrEmpty(serviceAttribute.Name))
                {
                    serviceName = serviceAttribute.Name;
                }
                if (!string.IsNullOrEmpty(serviceAttribute.GatewayPrefix))
                {
                    hostAddress.serviceNamePrefix = serviceAttribute.GatewayPrefix;
                }
            }
            var apiClientConnect = clientConnect as ApiClientConnect;
            var httpMethod = HttpMethod.POST;
            var responseContentType = contentType;
            var requestPath = $"/{apiClientConnect.Apiprefix}/{serviceName}/{method.MethodInfo.Name}";
            if (methodAttribute != null)
            {
                httpMethod = methodAttribute.Method;
                if (!string.IsNullOrEmpty(methodAttribute.Path))
                {
                    requestPath = methodAttribute.Path;
                    if (!requestPath.StartsWith("/"))
                    {
                        requestPath = "/" + requestPath;
                    }
                }
                if (methodAttribute.ContentType != ContentType.NONE)
                {
                    contentType = methodAttribute.ContentType;
                    responseContentType = contentType;
                }
                if (methodAttribute.ResponseContentType != ContentType.NONE)
                {
                    responseContentType = methodAttribute.ResponseContentType;
                }
            }

            var url = hostAddress.GetHttpAddress() + requestPath;
            var request = new ImitateWebRequest(ServiceName, apiClientConnect.Encoding);
            request.ContentType = ContentTypeDic[contentType];
            string result;
            var firstArgs = args.FirstOrDefault();
            var members = new Dictionary<string, object>();
            #region 提交前参数回调处理
            if (httpMethod == HttpMethod.POST && args.Count() == 1)//只有一个参数的POST
            {
                var type = firstArgs.GetType();
                var pro = type.GetProperties();
                if (firstArgs is System.Collections.IDictionary)
                {
                    var dic = firstArgs as System.Collections.IDictionary;
                    foreach (string key in dic.Keys)
                    {
                        members.Add(key, dic[key]);
                    }
                }
                else
                {
                    foreach (var p in pro)
                    {
                        members.Add(p.Name, p.GetValue(firstArgs));
                    }
                }
            }
            else
            {
                for (int i = 0; i < argsName.Length; i++)
                {
                    var p = argsName[i];
                    var value = args[i];
                    members.Add(p.Name, value);
                }
            }
            #endregion
            try
            {
                apiClientConnect.OnBeforRequest?.Invoke(request, members, url);
            }
            catch (Exception ero)
            {
                throw new Exception("设置请求头信息时发生错误:" + ero.Message);
            }

            if (httpMethod == HttpMethod.POST || httpMethod == HttpMethod.PUT)
            {
                string data = "";
                if (firstArgs != null)
                {
                    if (contentType == ContentType.JSON)
                    {
                        data = members.ToJson();
                    }
                    else if (contentType == ContentType.XML)
                    {
                        data = Core.SerializeHelper.XmlSerialize(firstArgs, apiClientConnect.Encoding);
                    }
                    else if (contentType == ContentType.FORM)
                    {
                        data = GetFormData(members);
                    }
                    else
                    {
                        data = firstArgs.ToString();
                    }
                }
                result = request.SendData(url, httpMethod.ToString(), data, out string nowUrl);
            }
            else
            {
                var list = new List<string>();
                foreach (var kv in members)
                {
                    list.Add(string.Format("{0}={1}", kv.Key, kv.Value));
                }
                var str = string.Join("&", list);
                result = request.Get($"{url}?{str}");
            }
            var generType = returnType;
            bool isTask = false;
            if (returnType.Name.StartsWith("Task`1"))
            {
                generType = returnType.GenericTypeArguments[0];
                isTask = true;
            }
            object returnObj;
            try
            {
                if (responseContentType == ContentType.JSON)
                {
                    returnObj = SerializeHelper.DeserializeFromJson(result, generType);
                }
                else if (responseContentType == ContentType.XML)
                {
                    returnObj = SerializeHelper.XmlDeserialize(generType, result, apiClientConnect.Encoding);
                }
                else
                {
                    returnObj = result;
                }
            }
            catch (Exception ero)
            {
                var eroMsg = $"反序列化为{generType.Name}时出错:" + ero.Message;
                Core.EventLog.Error(eroMsg + " " + result);
                throw new Exception(eroMsg);
            }
            if (isTask)
            {
                //返回Task类型
                var method2 = typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static);
                var result2 = method2.MakeGenericMethod(new Type[] { generType }).Invoke(null, new object[] { returnObj });
                return result2;
            }
            return returnObj;
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var controlName = ServiceName;
            var method = serviceInfo.GetMethod(binder.Name);
            //var methodAttribute = method.GetAttribute<MethodAttribute>();
            //var methodParamters = method.MethodInfo.GetParameters();
            var returnType = method.MethodInfo.ReturnType;
            var request = new RequestJsonMessage
            {
                Service = controlName,
                Method = binder.Name,
                Token = clientConnect.TokenInfo.Token
            };
            request.Args = args.ToList();
            object response = null;
            try
            {
                response = SendRequest(serviceInfo, method, args);
            }
            catch (Exception ero)
            {
                var msg = ero.Message;
                if(ero is RequestException)
                {
                    msg = (ero as RequestException).ToString();
                }
                ThrowError(msg, "500");
            }
            if (returnType == typeof(void))
            {
                result = null;
                return true;
            }
            result = response;

            return true;

        }

        static string GetFormData(Dictionary<string, object> dic)
        {
            var list = new List<string>();
            //like Args[MapSpid]=1&Args[StartTime]1=&Args[EndTime]=&Args[Status]=&_search=false&nd=1573883648354&rows=100&page=1&sidx=&sord=asc
            foreach (var kv in dic)
            {
                var value = kv.Value;
                if (value == null)
                {
                    continue;
                }
                var type = value.GetType();
                if (value is IDictionary)
                {
                    var dic2 = value as IDictionary;
                    foreach (string key in dic2.Keys)
                    {
                        var value2 = dic2[key];
                        list.Add($"{kv.Key}[{key}]={value2}");
                    }
                }
                else if (type != typeof(string) && type.IsClass)
                {
                    var pros = type.GetProperties();
                    foreach (var p in pros)
                    {
                        list.Add($"{kv.Key}[{p.Name}]={p.GetValue(value)}");
                    }
                }
                else
                {
                    list.Add($"{kv.Key}={value}");
                }
            }
            return string.Join("&", list);
        }
    }
}
