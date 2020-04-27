#if NETSTANDARD
using CRL.Core.Remoting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
#endif
namespace CRL.DynamicWebApi.NetCore
{

#if NETSTANDARD
    public class DynamicApiMiddleware
    {
        private readonly RequestDelegate _next;
        ServerCreater _serverCreater;
        IServiceProvider _serviceProvider;
        public DynamicApiMiddleware(RequestDelegate next, ServerCreater serverCreater, IServiceProvider serviceProvider)
        {
            _next = next;
            _serverCreater = serverCreater;
            _serviceProvider = serviceProvider;
            //_serverCreater.CreateApi();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var prefix = httpContext.Request.Path.Value.Split('/')[1];
            if (serviceInfo.apiPrefixCache.ContainsKey(prefix))
            {
                await OnRequest(httpContext, prefix);
                return;
            }
            await _next(httpContext);
        }

        async Task OnRequest(HttpContext context,string prefix)
        {
            var request = context.Request;
            var response = context.Response;
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            var path = request.Path.Value;
            var arry = path.Split('/');
            var service = arry[2];
            var method = arry[3];
            var token = request.Headers["token"];
            var requestMsg = new RequestJsonMessage()
            {
                Service = service,
                Method = method,
                Token = token,
                ApiPrefix = prefix
            };
            if (request.ContentLength > 0)
            {
                var reader = new StreamReader(request.Body);
                var args = await reader.ReadToEndAsync();
                requestMsg.Args = args.ToObject<List<object>>();
            }
            //var instance = ServerCreater.Instance;
            var server = _serverCreater.GetServer();
            var result = server.InvokeResult(requestMsg, type =>
             {
                 //获取注入的AbsService
                 return _serviceProvider.GetService(type);
             });
            await response.WriteAsync(result.ToJson());
        }
    }
#endif
}
