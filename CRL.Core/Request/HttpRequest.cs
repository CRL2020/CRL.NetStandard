using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Security;
namespace CRL.Core.Request
{
	public class HttpRequest
	{
        /// <summary>
        /// http post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="enc"></param>
        /// <param name="contentType"></param>
        /// <param name="proxyHost">代理</param>
        /// <returns></returns>
        public static string HttpPost(string url, string data, Encoding enc = null, string contentType = "application/x-www-form-urlencoded", string proxyHost = "")
        {
            if (enc == null)
            {
                enc = Encoding.UTF8;
            }
            var request = new ImitateWebRequest(new Uri(url).Host, enc);
            request.ContentEncoding = enc;
            request.ContentType = contentType;
            request.ProxyHost = proxyHost;
            return request.Post(url, data);
        }
        /// <summary>
        /// 指定编码GET
        /// </summary>
        /// <param name="url"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string HttpGet(string url, Encoding enc = null)
        {
            if (enc == null)
            {
                enc = Encoding.UTF8;
            }
            return HttpGet(url, null, enc);
        }
        /// <summary>
        /// 返回字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="proxyHost"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string HttpGet(string url, string proxyHost, Encoding enc)
        {
            var requestInstance = new ImitateWebRequest(new Uri(url).Host, enc);
            requestInstance.ProxyHost = proxyHost;
            return requestInstance.Get(url);
        }
        /// <summary>
        /// 返回流
        /// </summary>
        /// <param name="url"></param>
        /// <param name="proxyHost">代理地址</param>
        /// <returns></returns>
        public static Stream HttpGet(string url, string proxyHost, out HttpWebRequest request)
        {
            var requestInstance = new ImitateWebRequest(new Uri(url).Host, Encoding.UTF8);
            requestInstance.ProxyHost = proxyHost;
            return requestInstance.GetStream(url, out request);
        }
	}
}
