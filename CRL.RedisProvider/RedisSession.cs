using CRL.RedisProvider;
using CRL.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CRL.Core;

namespace CRL.RedisProvider
{
    /// <summary>
    /// 用户状态管理
    /// </summary>
    public class RedisSession : AbsSession
    {
        RedisClient client;
        static int timeOut = 30;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_context"></param>
        public RedisSession(IWebContext _context) : base(_context)
        {
            //var cookie = _context.Request.Cookies.Get(SessionName);
            //if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            //{
            //    SessionId = NewGuid();
            //    _context.Response.Cookies.Add(new HttpCookie(SessionName, SessionId));
            //    _context.Request.Cookies.Add(new HttpCookie(SessionName, SessionId));
            //}
            //else
            //{
            //    SessionId = cookie.Value;
            //}
            SessionId = _context.GetCookie(SessionName);
            client = new RedisClient(1);
        }

        /// <summary>
        /// 生成一个新ID
        /// </summary>
        /// <returns></returns>
        private string NewGuid()
        {
            return SessionName + Guid.NewGuid().ToString();
        }

        SessionObj getSession()
        {
            var obj = client.KGet<SessionObj>(SessionId);
            if (obj == null)
            {
                return null;
            }
            return obj;
        }
        public override T Get<T>(string name)
        {
            var sessionObj = getSession();
            if (sessionObj == null)
            {
                return default(T);
            }
            var dic = sessionObj.Data;
            if (dic == null)
            {
                return default(T);
            }
            string value;
            var a = dic.TryGetValue(name, out value);
            if (!a)
            {
                return default(T);
            }
            return SerializeHelper.DeserializeFromJson<T>(value);
        }

        public override void Set(string name,object value)
        {
            var sessionObj = getSession();
            if (sessionObj == null)
            {
                sessionObj = new SessionObj();
            }
            if (sessionObj.Data == null)
            {
                sessionObj.Data = new Dictionary<string, string>();
            }
            sessionObj.Data[name] = SerializeHelper.SerializerToJson(value);
            client.KSet(SessionId, sessionObj, new TimeSpan(0, timeOut, 0));
        }

        public override void Remove(string name)
        {
            var sessionObj = getSession();
            if (sessionObj == null)
            {
                return;
            }
            if (sessionObj.Data == null)
            {
                return;
            }
            sessionObj.Data.Remove(name);
            client.KSet(SessionId, sessionObj, new TimeSpan(0, timeOut, 0));
        }

        public override void Clean()
        {
            client.Remove(SessionId);
        }
        public override void Refresh()
        {
            //每个请求会刷新
            client.KSetEntryIn(SessionId, new TimeSpan(0, timeOut, 0));
        }

        ///// <summary>
        ///// 获取当前用户信息
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public override T Get<T>()
        //{
        //    return client.KGet<T>(SessionId);
        //}

        ///// <summary>
        ///// 用户是否在线
        ///// </summary>
        ///// <returns></returns>
        //public override bool IsLogin()
        //{
        //    return client.KIsExist(SessionId);
        //}

        ///// <summary>
        ///// 登录
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="obj"></param>
        //public override void Login<T>(T obj)
        //{
        //    client.KSet(SessionId, obj, new TimeSpan(0, Managers.TimeOut, 0));
        //}

        ///// <summary>
        ///// 退出
        ///// </summary>
        //public override void Quit()
        //{
        //    client.KRemove(SessionId);
        //}

        ///// <summary>
        ///// 续期
        ///// </summary>
        //public override void Postpone()
        //{
        //    client.KSetEntryIn(SessionId, new TimeSpan(0, Managers.TimeOut, 0));
        //}
    }
}

