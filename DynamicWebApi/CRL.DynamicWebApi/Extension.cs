using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.DynamicWebApi
{
    public static class Extension
    {
        public static ServerCreater CreatetApi(this ServerCreater serverCreater)
        {
            var server = new ApiServer();
            serverCreater.SetServer(server);
            return serverCreater;
        }
    }
}
