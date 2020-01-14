using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ConsulClient
{
    public class CheckRegistrationInfo
    {
        //public string Name { get; set; }
        //public string CheckID { get; set; }
        public string Interval { get; set; }
        //public string Notes {get;set; }
        public string DeregisterCriticalServiceAfter {get;set; }
        //public string Script {get;set; }
        //public string DockerContainerID { get; set; }
        //public string Shell { get; set; }
        public string HTTP { get; set; }
        public bool TLSSkipVerify { get; set; }
        //public string TCP { get; set; }
        //public string TTL { get; set; }
        //public string ServiceID { get; set; }
        //public ServiceCheckStatus Status { get; set; }
    }
}
