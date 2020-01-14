using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CRL.Core.ConsulClient
{
    public class ServiceInfo
    {
        public string ID { get; set; }
        public string Service { get; set; }
        /// <summary>
        /// 带http
        /// </summary>
        public string Address { get; set; }
        public int Port { get; set; }
        public string[] Tags { get; set; }
    }
}
