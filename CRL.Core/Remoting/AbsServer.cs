using CRL.Core.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public abstract class AbsServer: IDisposable
    {
        protected static Dictionary<string, serviceInfo> serviceHandle = new Dictionary<string, serviceInfo>();
        internal void Register<IService, Service>() where Service : AbsService, IService, new() where IService : class
        {
            var type = typeof(Service);
            var info = serviceInfo.GetServiceInfo(type);
            serviceHandle.Add(typeof(IService).Name, info);
        }
        protected ISessionManage sessionManage
        {
            get
            {
                return ServerCreater.SessionManage;
            }
        }
        public virtual void Start()
        {

        }
        public virtual void Dispose()
        {

        }
        public abstract object InvokeResult(object rq);
        protected class ErrorInfo
        {
            public string msg;
            public string code;
            public ErrorInfo(string m,string c)
            {
                msg = m;
                code = c;
            }
        }
        protected ErrorInfo InvokeMessage(MessageBase request, out object result, out Dictionary<int, object> outs,out string token)
        {
            result = null;
            token = "";
            outs = new Dictionary<int, object>();
            var a = serviceHandle.TryGetValue(request.Service, out serviceInfo serviceInfo);
            if (!a)
            {
                return new ErrorInfo("未找到该服务", "404");
            }
            var serviceType = serviceInfo.ServiceType;
            var service = System.Activator.CreateInstance(serviceType) as AbsService;
            var methodInfo = serviceInfo.GetMethod(request.Method);
            if (methodInfo == null)
            {
                return new ErrorInfo("未找到该方法" + request.Method, "404");
            }
            var checkToken = true;
            var allowAnonymous = serviceInfo.GetAttribute<AllowAnonymousAttribute>();
            var allowAnonymous2 = methodInfo.GetAttribute<AllowAnonymousAttribute>();
            if (allowAnonymous != null || allowAnonymous2 != null)
            {
                checkToken = false;
            }
            var loginAttr = methodInfo.GetAttribute<LoginPointAttribute>();
            if (loginAttr != null)
            {
                checkToken = false;
            }
            var paramters = request.Args;
            var method = methodInfo.MethodInfo;
            var methodParamters = methodInfo.Parameters;
            outs = new Dictionary<int, object>();
            int i = 0;
            foreach (var p in methodParamters)
            {
                var value = paramters[i];

                if (p.Attributes == ParameterAttributes.Out)
                {
                    outs.Add(i, null);
                }
                else
                {
                    if (value != null)
                    {
                        if (value.GetType() != p.ParameterType)
                        {
                            var value2 = value.ToJson().ToObject(p.ParameterType);
                            paramters[i] = value2;
                        }
                    }
                    else
                    {
                        paramters[i] = value;
                    }
                }
                i += 1;
            }
            if (request.httpPostedFile != null)
            {
                service.SetPostFile(request.httpPostedFile);
            }

            if (request.Args.Count != methodParamters.Count())
            {
                return new ErrorInfo("参数计数不正确" + request.ToJson(), "500");
            }

            if (checkToken)//登录切入点不验证
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return new ErrorInfo("请求token为空,请先登录", "401");
                    //throw new Exception("token为空");
                }
                string error;
                if (_jwtTokenCheck != null)
                {
                    #region 使用jwt认证
                    if (!_jwtTokenCheck(request, out string jwtUser, out error))
                    {
                        return new ErrorInfo($"jwt认证失败:{error}", "401");
                    }
                    service.SetUser(jwtUser);
                    #endregion
                }
                else
                {
                    #region 使用简单登录认证
                    var tokenArry = request.Token.Split('@');
                    if (tokenArry.Length < 2)
                    {
                        return new ErrorInfo("token不合法 user@token", "401");
                        //throw new Exception("token不合法 user@token");
                    }
                    var a2 = CheckSession(tokenArry[0], tokenArry[1], methodParamters, paramters, out error);
                    if (!a2)
                    {
                        return new ErrorInfo(error, "401");
                    }
                    //Core.CallContext.SetData("currentUser", tokenArry[0]);
                    service.SetUser(tokenArry[0]);
                    #endregion
                }
            }

            var args3 = paramters?.ToArray();
            result = method.Invoke(service, args3);
            foreach (var kv in new Dictionary<int, object>(outs))
            {
                var value = args3[kv.Key];
                outs[kv.Key] = value;
            }
            if (loginAttr != null)//登录方法后返回新TOKEN
            {
                token = service.GetToken();
            }
            return null;
        }
        bool CheckSession(string user, string token, ParameterInfo[] argsName, List<object> args, out string error)
        {
            error = "";
            //var exists = sessions.TryGetValue(user, out Tuple<string, object> v);
            var v = sessionManage.GetSession(user);
            if (v == null)
            {
                error = "未找到API登录状态,请重新登录";
                return false;
            }
            var serverToken = v.Item1;
            if (ServerCreater.__CheckSign)//使用简单签名
            {
                serverToken = SignCheck.CreateSign(serverToken, argsName, args);
            }
            if (token != serverToken)
            {
                error = "token验证失败";
                return false;
            }
            return true;
        }
        internal JwtTokenCheckHandler _jwtTokenCheck;
    }
}
