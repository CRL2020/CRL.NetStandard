using CRL.Core.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CRL.Core.Remoting
{
    public class MessageBase
    {
        public string Token
        {
            get; set;
        }
        public string ApiPrefix { get; set; }
        public string Service { get; set; }
        public string Method { get; set; }
        public List<object> Args { get; set; }
        //public HttpPostedFile httpPostedFile;
    }

    public class RequestJsonMessage
    {
        public string Token
        {
            get; set;
        }
        public string ApiPrefix { get; set; }

        public string Service { get; set; }
        public string Method { get; set; }
        /// <summary>
        /// 按索引
        /// </summary>
        public List<object> Args { get; set; }
        public string MsgId { get; set; }

        public static RequestJsonMessage FromBuffer(string buffer)
        {
            return buffer.ToObject<RequestJsonMessage>();
        }
        //public HttpPostedFile httpPostedFile;

        public string ToBuffer()
        {
            return this.ToJson();
        }
    }
    public class ResponseJsonMessage
    {
        public string Token
        {
            get; set;
        }
        public bool Success { get; set; }
        public string Data { get; set; }
        public static ResponseJsonMessage CreateError(string msg, string code)
        {
            return new ResponseJsonMessage() { Success = false, Msg = msg, Data = code };
        }
        public object GetData(Type type)
        {
            return Data.ToObject(type);
        }
        public Dictionary<int, object> Outs
        {
            get; set;
        }
        public void SetData(object data)
        {
            Data = data.ToJson();
        }
        public string Msg { get; set; }
        public string MsgId { get; set; }
        /// <summary>
        /// webSocket用
        /// </summary>
        public string MsgType { get; set; }

        public static ResponseJsonMessage FromBuffer(string buffer)
        {
            return buffer.ToObject<ResponseJsonMessage>();
        }

        public string ToBuffer()
        {
            return this.ToJson();
        }
    }
}
