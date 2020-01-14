using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ConsulClient
{
    public class ServiceRegistrationInfo
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string[] Tags { get; set; }
        public string Address { get; set; }
        public CheckRegistrationInfo Check { get; set; }
        public CheckRegistrationInfo[] Checks { get; set; }
        public bool EnableTagOverride { get; set; }
        public int Port { get; set; }
    }
}
