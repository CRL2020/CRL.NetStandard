using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CRL.Core.Session
{
    public class SessionManage
    {
        public static AbsSession GetSessionClient(IWebContext context)
        {
            if (ConfigBuilder.current?.__SessionCreater == null)
            {
                return new WebSession(context);
            }
            return ConfigBuilder.current?.__SessionCreater(context);
        }
    }
}
