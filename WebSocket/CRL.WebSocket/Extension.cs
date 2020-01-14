using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.WebSocket
{
    public static class Extension
    {
        public static ServerCreater CreatetWebSocket(this ServerCreater serverCreater, int port)
        {
            var server = new WebSocketServer(port);
            serverCreater.SetServer(server);
            return serverCreater;
        }
    }
}
