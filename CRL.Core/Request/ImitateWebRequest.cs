using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CRL.Core.Request
{
    /// <summary>
    /// 模拟WEB请求,并附带上COOKIE
    /// </summary>
    public class ImitateWebRequest
    {
        static ImitateWebRequest()
        {
            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
        }
        /// <summary>
        /// 创建附带COOKIE的请求
        /// </summary>
        /// <param name="_cookieName">指定识别同一网站的COOKIE名</param>
        /// <param name="_encoding"></param>
        public ImitateWebRequest(string _cookieName, Encoding _encoding = null)
        {
            if (_encoding == null)
            {
                _encoding = Encoding.UTF8;
            }
            cookieName = _cookieName;
            ContentEncoding = _encoding;
        }
        #region 请求属性
        /// <summary>
        /// 代理
        /// </summary>
        public string ProxyHost;
        Encoding _ContentEncoding = Encoding.UTF8;
        /// <summary>
        /// 发送编码
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                return _ContentEncoding;
            }
            set
            {
                _ContentEncoding = value;
                ResponseEncoding = value;
            }
        }
        /// <summary>
        /// 返回编码
        /// </summary>
        public Encoding ResponseEncoding = Encoding.UTF8;
        /// <summary>
        /// 设置HTTP标头的值
        /// </summary>
        public string Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/vnd.ms-excel, application/msword, application/x-shockwave-flash, */*";
        /// <summary>
        /// 设置内容类型
        /// </summary>
        public string ContentType = "application/x-www-form-urlencoded";
        //private static Encoding _Encoder = Encoding.UTF8;
        private static object lockObj = new object();

        /// <summary>
        /// 请求是是否附加上Cookie
        /// </summary>
        public bool RequestWidthCookie = true;
        #endregion
        string cookieName = "";
        private Dictionary<string, CookieCollection> siteCookies = new Dictionary<string, CookieCollection>();
        Dictionary<string, object> heads = new Dictionary<string, object>();
        /// <summary>
        /// 添加头部信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetHead(string key, object value)
        {
            heads.Add(key, value);
        }
        static string GetHost(string url)
        {
            if (url.IndexOf("http://") > -1)
            {
                url = new Uri(url).Host;
            }
            if (url.Split(new char[] { '.' }).Length > 2)
            {
                url = url.Substring(url.IndexOf(".") + 1);
            }
            return url;
        }
        /// <summary>
        /// 附加COOKIE
        /// </summary>
        /// <param name="cookie"></param>
        public void AddCoolie(Cookie cookie)
        {
            //CookieCollection c = GetCurrentCookie();
            //c.Add(cookie);
            //site_cookies[currentHost] = c;
            CookieCollection c = new CookieCollection();
            c.Add(cookie);
            SaveCookies(c);
        }
        /// <summary>
        /// 清除COOKIE
        /// </summary>
        /// <param name="name"></param>
        public void CleanCookie(string name = "")
        {
            if (!string.IsNullOrEmpty(name))
            {
                siteCookies.Remove(name);
            }
            else
            {
                siteCookies.Clear();
            }
        }
        /// <summary>
        /// 获取当前COOKIE
        /// </summary>
        /// <returns></returns>
        public CookieCollection GetCurrentCookie()
        {
            CookieCollection cookie;
            if (siteCookies.ContainsKey(cookieName))
            {
                cookie = siteCookies[cookieName];
            }
            else
            {
                cookie = new CookieCollection();
                siteCookies.Add(cookieName, cookie);
            }
            return cookie;
        }
        private void SaveCookies(CookieCollection newCookies)
        {
            lock (lockObj)
            {
                CookieCollection oldCookies = GetCurrentCookie();
                CookieCollection tmpCookies = new CookieCollection();
                string log = "";
                foreach (Cookie c in newCookies)
                {
                    tmpCookies.Add(c);
                    log += string.Format("Name:[{0}] Value:[{1}] Domain:[{2}]\r\n", c.Name, c.Value, c.Domain);
                }
                if (log != "")
                {
                    //CRL.Core.EventLog.Log(cookieName + "    保存COOKIE " + log);
                }
                foreach (Cookie c in oldCookies)
                {
                    bool blnExist = false;
                    foreach (Cookie newcookie in tmpCookies)
                    {
                        if (newcookie.Name.Equals(c.Name))
                        {
                            blnExist = true;
                            break;
                        }
                    }
                    if (!blnExist)
                    {
                        tmpCookies.Add(c);
                    }
                }
                siteCookies[cookieName] = tmpCookies;
            }
        }

        private MemoryStream CopyStream(Stream stream)
        {
            MemoryStream result = new MemoryStream();
            byte[] buffer = new byte[0x1000];
            while (true)
            {
                int size = stream.Read(buffer, 0, 0x1000);
                if (size <= 0)
                {
                    result.Seek(0L, SeekOrigin.Begin);
                    return result;
                }
                result.Write(buffer, 0, size);
            }
        }
        /// <summary>
        /// 请求内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetSource(string url)
        {
            var dataStream = GetStream(url,out HttpWebRequest request);
            string responseFromServer;
            try
            {
                using (var reader = new StreamReader(dataStream, ResponseEncoding))
                {
                    responseFromServer = reader.ReadToEnd();
                }
                dataStream.Close();
            }
            catch (Exception ero)
            {
                dataStream.Close();
                request?.Abort();
                throw ero;
            }
            request?.Abort();
            return responseFromServer;
        }
        /// <summary>
        /// 请求内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Get(string url)
        {
            return GetSource(url);
        }
        /// <summary>
        /// POST内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Post(string url, string data)
        {
            string out_str;
            return SendData(url,"POST", data, out out_str);
        }
        public string Put(string url, string data)
        {
            string out_str;
            return SendData(url, "PUT", data, out out_str);
        }
        /// <summary>
        /// POST内容,并返回跳转后的URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="now_url"></param>
        /// <returns></returns>
        public string SendData(string url,string method, string data, out string now_url)
        {
            string str = "";
            HttpWebResponse response;
            HttpWebRequest request = null;
            try
            {
                request = CreateWebRequest(url, method, data);
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                if (response != null && ContentType == "application/json")
                {
                    using (StreamReader requestReader = new StreamReader(response.GetResponseStream(), ResponseEncoding))
                    {
                        str = requestReader.ReadToEnd();
                    }
                }
                else
                {
                    str = ex.Message;
                }
                response?.Close();
                request?.Abort();
                throw new RequestException(url, data, str);
            }
            //var errorCodes = new int[] { 404, 500 };
            //if (errorCodes.Contains((int)response.StatusCode))
            //{
            //    throw new Exception("服务器返回内部错误"+ response.StatusCode);
            //}
            SaveCookies(response.Cookies);
            now_url = response.ResponseUri.ToString();

            try
            {
                if (response.StatusCode == HttpStatusCode.Found)
                {
                    string url1 = response.Headers["Location"];
                    now_url = url1;
                    CurrentUrl = url1;
                    return GetSource(url1);
                }
                using (StreamReader requestReader = new StreamReader(response.GetResponseStream(), ResponseEncoding))
                {
                    str = requestReader.ReadToEnd();
                }
            }
            catch(Exception ero)
            {
                response?.Close();
                request?.Abort();
                throw new RequestException(url, data, ero.Message);
            }
            response?.Close();
            request?.Abort();
            return str;
        }
        private static bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
        string certPasswd, certFile;
        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="_certFile"></param>
        /// <param name="_certPasswd"></param>
        public void SetCer(string _certFile, string _certPasswd)
        {
            certFile = _certFile;
            certPasswd = _certPasswd;
        }
        /// <summary>
        /// 创建请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public HttpWebRequest CreateWebRequest(string url, string method, string postData)
        {
            Uri URI = new Uri(url);
            HttpWebRequest request;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;

                if (!string.IsNullOrEmpty(certFile))
                {
                    request.ClientCertificates.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(this.certFile, this.certPasswd));
                }

            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }

            if (!string.IsNullOrEmpty(ProxyHost))
            {
                System.Net.WebProxy proxy = new WebProxy(ProxyHost);
                request.Proxy = proxy;
            }
            else
            {
                //request.Proxy = WebRequest.GetSystemWebProxy();
            }

            request.KeepAlive = true;
            request.AllowAutoRedirect = false;
            //request.Timeout = 15000;
            request.Accept = Accept;
            request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            //request.Referer = url;
            foreach (var kv in heads)
            {
                //request.Headers.Add(kv.Key, kv.Value.ToString());
                request.Headers[kv.Key] = kv.Value.ToString();
            }
            if (request.CookieContainer == null)
            {
                request.CookieContainer = new CookieContainer();
            }
            if (RequestWidthCookie)
            {
                //附加上Cookie
                CookieCollection tmpCookies = GetCurrentCookie();
                foreach (Cookie cookie in tmpCookies)
                {
                    cookie.Domain = URI.Host;
                    if (cookie.Value.Length != 0)
                    {
                        request.CookieContainer.Add(cookie);
                    }
                }
            }
            //RequestWidthCookie = true;
            if ((method!="GET") && (postData.Length > 0))
            {
                request.ContentType = ContentType;
                request.Method = method;
                request.ServicePoint.Expect100Continue = false;
                //EventLog.Log(request.ToJson(), "post");
                byte[] b = ContentEncoding.GetBytes(postData);
                request.ContentLength = b.Length;
                using (Stream sw = request.GetRequestStream())
                {
                    try
                    {
                        sw.Write(b, 0, b.Length);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return request;
        }
        /// <summary>
        /// 重新定位后当前地址
        /// </summary>
        public string CurrentUrl;
        /// <summary>
        /// 获取请求Stream
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Stream GetStream(string url, out HttpWebRequest request)
        {
            MemoryStream dataStream;
            request = null;
            try
            {
                request = CreateWebRequest(url, "GET",null);
            }
            catch (Exception ero)
            {
                request?.Abort();
                throw new RequestException(url, "", ero.Message);
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                SaveCookies(response.Cookies);
                if (response.StatusCode == HttpStatusCode.Found)
                {
                    string url1 = response.Headers["Location"];
                    CurrentUrl = url1;
                    return GetStream(url1, out request);
                }
                dataStream = CopyStream(response.GetResponseStream());
            }
            catch (Exception ero)
            {
                request?.Abort();
                response?.Close();
                throw new RequestException(url, "", ero.Message);
            }
            return dataStream;
        }
    }
    public class RequestException : Exception
    {
        public string Url;
        public string Args;
        public RequestException(string url,string args,string ero):base(ero)
        {
            Url = url;
            Args = args;
        }
        public override string ToString()
        {
            return $"发送请求时失败,{Message} 在URL:{Url}";
        }
    }
}
