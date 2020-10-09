using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// 请求内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Get(string url)
        {
            return SendData(url, "GET", "");
        }
        /// <summary>
        /// POST内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Post(string url, string data)
        {
            return SendData(url,"POST", data);
        }
        public string Put(string url, string data)
        {
            return SendData(url, "PUT", data);
        }
        static SimplePool<HttpClient> httpClientPool = new SimplePool<HttpClient>(()=>
        {
            return new HttpClient();
        },20,1);
        public async Task<string> SendDataAsync(string url, string method, string data)
        {
            //httpclient的问题
            //https://www.cnblogs.com/jlion/p/12813692.html
            var httpClient = httpClientPool.Rent();
            httpClient.DefaultRequestHeaders.Clear();
            //httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Add("ContentType", ContentType);
            httpClient.DefaultRequestHeaders.Add("Accept", Accept);
            foreach (var kv in heads)
            {
                httpClient.DefaultRequestHeaders.Add(kv.Key, kv.Value.ToString());
            }
            if (RequestWidthCookie)
            {
                var cookies = new List<string>();
                foreach(Cookie c in GetCurrentCookie())
                {
                    var str = $"{c.Name}={c.Value}";
                    cookies.Add(str);
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", string.Join("&", cookies));
            }
            var content = new StringContent(data, ContentEncoding);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentType);
            HttpResponseMessage response;
            switch (method)
            {
                case "POST":
                    response = await httpClient.PostAsync(url, content);
                    break;
                case "PUT":
                    response = await httpClient.PutAsync(url, content);
                    break;
                case "DELETE":
                    response = await httpClient.DeleteAsync(url);
                    break;
                default:
                    response = await httpClient.GetAsync(url);
                    break;
            }
            httpClientPool.Return(httpClient);

            string result;
            using (var myResponseStream = await response.Content.ReadAsStreamAsync())
            {
                using (var myStreamReader = new StreamReader(myResponseStream, ResponseEncoding))
                {
                    result = myStreamReader.ReadToEnd();
                }
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new RequestException(url, data, result);
            }
            //httpClientPool.Return(httpClient);
            return result;
        }
        /// <summary>
        /// POST内容,并返回跳转后的URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="now_url"></param>
        /// <returns></returns>
        public string SendData(string url,string method, string data)
        {
            return SendDataAsync(url, method, data).Result;
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
