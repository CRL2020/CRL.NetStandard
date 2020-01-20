using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public class HostAddress
    {
        public string address;
        public int port;
        public string serviceNamePrefix;
        /// <summary>
        /// 返回带HTTP的格式
        /// </summary>
        /// <returns></returns>
        public string GetHttpAddress()
        {
            var str = address + "/";
            if (str.EndsWith("/"))
            {
                str = str.TrimEnd('/');
            }
            if (!str.StartsWith("http"))
            {
                str = $"http://{str}";
            }
            if (port > 0)
            {
                str += $":{port}";
            }
            if (!string.IsNullOrEmpty(serviceNamePrefix))
            {
                serviceNamePrefix = serviceNamePrefix.TrimStart('/');
                serviceNamePrefix = serviceNamePrefix.TrimEnd('/');
                str += $"/{serviceNamePrefix}";
            }
            return str;
        }
    }
    public abstract class AbsClient: DynamicObject, IDisposable
    {
        HostAddress hostAddress;
        public HostAddress HostAddress
        {
            get
            {
                //获取consol服务发现
                if (clientConnect.__GetConsulAgent != null)
                {
                    var address = clientConnect.__GetConsulAgent();
                    if (!string.IsNullOrEmpty(hostAddress.serviceNamePrefix))
                    {
                        address.serviceNamePrefix = hostAddress.serviceNamePrefix;
                    }
                    return address;
                }
                return hostAddress;
            }
            set
            {
                hostAddress = value;
            }
        }
    
        public string ServiceName
        {
            get
            {
                return serviceInfo.ServiceType.Name;
            }
        }
        public Type ServiceType
        {
            get
            {
                return serviceInfo.ServiceType;
            }
        }
        public serviceInfo serviceInfo;
        public AbsClient(AbsClientConnect _clientConnect)
        {
            clientConnect = _clientConnect;
        }
        protected AbsClientConnect clientConnect;
        protected void ThrowError(string msg, string code)
        {
            clientConnect.OnError?.Invoke(msg, code);
            throw new RemotingEx(msg) { Code = code };

            //else
            //{
            //    throw new RemotingEx(msg) { Code = code };
            //}
        }
        public virtual void Dispose()
        {

        }
        /// <summary>
        /// 生成请求的token
        /// </summary>
        /// <param name="argsName"></param>
        /// <param name="args"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected string CreateAccessToken(ParameterInfo[] argsName, List<object> args, TokenInfo token)
        {
            if (token.IsJwtToken)
            {
                //使用jwt token
                return token.Token;
            }
            if (token.UseSign && !string.IsNullOrEmpty(token.Token))
            {
                var arry = token.Token.Split('@');
                var sign = SignCheck.CreateSign(arry[1], argsName, args);
                return string.Format("{0}@{1}", arry[0], sign);
            }
            return token.Token;
        }
    }
}
