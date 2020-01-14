using CRL.Core.Extension;
using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRL.DynamicWebApi
{
    public class ServerListener: IDisposable
    {
        HttpListener httpListenner;
        Thread thread;
        public void Start(string uriPrefix)
        {
            httpListenner = new HttpListener();
            httpListenner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListenner.Prefixes.Add(uriPrefix);
            httpListenner.Start();
            thread = new Thread(new ThreadStart(() =>
              {
                  while (true)
                  {
                      HttpListenerContext context = httpListenner.GetContext();
                      try
                      {
                          OnRequest(context);
                      }
                      catch (Exception ero)
                      {
                          Console.WriteLine("Listenn error :" + ero.Message);
                          context.Response.Close();
                      }
                  }
              }));
            thread.Start();

        }

        void OnRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            var path = request.Url.AbsolutePath;
            if (!path.StartsWith("/DynamicApi/"))
            {
                byte[] res2 = Encoding.UTF8.GetBytes(path);
                response.OutputStream.Write(res2, 0, res2.Length);
                response.Close();
                return;
            }
            var arry = path.Split('/');
            var service = arry[2];
            var method = arry[3];
            //var serviceHandle = ApiServer.serviceHandle;
            var token = request.Headers["token"];

            var requestMsg = new RequestJsonMessage()
            {
                Service = service,
                Method = method,
                Token = token,
            };
            if (request.ContentLength64 > 0)
            {
                var ms = request.InputStream;
                var data = new byte[request.ContentLength64];
                ms.Read(data, 0, data.Length);
                var args = System.Text.Encoding.UTF8.GetString(data);
                requestMsg.Args = args.ToObject<List<object>>();
            }
            var result = ApiServer.Instance.InvokeResult(requestMsg);
            byte[] res = Encoding.UTF8.GetBytes(result.ToJson());
            response.OutputStream.Write(res, 0, res.Length);
            response.Close();
        }
        public void Dispose()
        {
            if (httpListenner != null)
            {
                httpListenner.Abort();
            }
            if (thread != null)
            {
                thread.Abort();
            }
        }
    }

}
