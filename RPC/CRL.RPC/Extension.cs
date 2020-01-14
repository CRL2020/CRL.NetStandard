using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.RPC
{
    public static class Extension
    {
        public static ServerCreater CreatetRPC(this ServerCreater serverCreater, int port)
        {
            var server = new RPCServer(port);
            serverCreater.SetServer(server);
            return serverCreater;
        }
    }
}
