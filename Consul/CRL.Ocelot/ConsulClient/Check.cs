using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.ConsulClient
{
    public class Check
    {
        public string Node { get; set; }
        public string CheckID { get; set; }
        public string Name { get; set; }
        public ServiceCheckStatus Status { get; set; }
        public string Notes { get; set; }
        public string Output { get; set; }
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string[] ServiceTags { get; set; }
    }
}
