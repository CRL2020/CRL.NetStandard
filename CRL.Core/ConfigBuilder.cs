using CRL.Core.Log;
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
            __Current = this;
        }
        static ConfigBuilder()
        {
            __Current = new ConfigBuilder();
        }
        internal static ConfigBuilder __Current;
        public Func<IWebContext, AbsSession> __SessionCreater;
    }
}
