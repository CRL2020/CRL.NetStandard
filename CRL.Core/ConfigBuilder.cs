using CRL.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CRL.Core
{
    public class ConfigBuilder
    {
        public ConfigBuilder()
        {
            current = this;
        }
        static ConfigBuilder()
        {
            current = new ConfigBuilder();
        }
        internal static ConfigBuilder current;
        public Func<IWebContext, AbsSession> __SessionCreater;
    }
}
