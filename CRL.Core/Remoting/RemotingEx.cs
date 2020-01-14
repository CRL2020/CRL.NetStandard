using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    public class RemotingEx : Exception
    {
        public RemotingEx(string msg):base(msg)
        {
        }
        public string Code { get; set; }
    }
}
