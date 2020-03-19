using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Grpc.Extend
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
    }
}
