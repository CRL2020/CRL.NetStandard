using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.GrpcExtend
{
    public class GrpcClientOptions
    {
        public string Host
        {
            get;set;
        }
        public int Port
        {
            get; set;
        }
        public void UseConsulDiscover(string consulUrl, string serviceName)
        {
            UseConsul = true;
            ConsulUrl = consulUrl;
            ConsulServiceName = serviceName;
        }
        internal bool UseConsul
        {
            get; set;
        }
        internal string ConsulUrl
        {
            get; set;
        }
        internal string ConsulServiceName
        {
            get; set;
        }
        internal Dictionary<string, Core.Remoting.PollyAttribute> MethodPolicies = new Dictionary<string, Core.Remoting.PollyAttribute>();
        /// <summary>
        /// 按方法名增加Polly策略
        /// </summary>
        /// <param name="methodName">like Greeter.SayHello,空则全局</param>
        /// <param name="pollyAttribute"></param>
        /// <returns></returns>
        public GrpcClientOptions AddPolicy(string methodName, Core.Remoting.PollyAttribute pollyAttribute)
        {
            MethodPolicies.Add(methodName.ToLower(), pollyAttribute);
            return this;
        }
    }
}
