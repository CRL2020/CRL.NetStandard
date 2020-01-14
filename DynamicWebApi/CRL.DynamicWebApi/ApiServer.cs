
using CRL.Core.Extension;
using CRL.Core.Remoting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.DynamicWebApi
{
    public class ApiServer: AbsServer
    {
        static ApiServer()
        {
            instance = new ApiServer();
        }
        static ApiServer instance;
        internal static ApiServer Instance
        {
            get
            {
                return instance;
            }
        }
        public override object InvokeResult(object rq)
        {
            var request = rq as RequestJsonMessage;
            var response = new ResponseJsonMessage();

            try
            {
                var msgBase = new Core.Remoting.MessageBase() { Args = request.Args, Method = request.Method, Service = request.Service, Token = request.Token };
                var errorInfo = InvokeMessage(msgBase, out object result, out Dictionary<int, object> outs, out string token);
                if (errorInfo != null)
                {
                    return ResponseJsonMessage.CreateError(errorInfo.msg, errorInfo.code);
                }
                response.SetData(result);
                response.Success = true;
                response.Outs = outs;
                if (!string.IsNullOrEmpty(token))//登录方法后返回新TOKEN
                {
                    response.Token = token;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Msg = ex.InnerException?.Message;
                Console.WriteLine(ex.ToString());
                CRL.Core.EventLog.Log(ex.ToString(), request.Service);
                return ResponseJsonMessage.CreateError(ex.InnerException?.Message + $" 在{request.Service}/{request.Method}", "500");
            }
 
            return response;
        }
    }
}
