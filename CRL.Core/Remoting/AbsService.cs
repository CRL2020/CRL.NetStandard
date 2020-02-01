using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CRL.Core.Remoting
{
    public abstract class AbsService
    {
        //public object ServiceProvider;
        static string __token;
        protected string CurrentUserName;
        protected object CurrentUserTag;
        public void SetUser(string user)
        {
            var v = ServerCreater.SessionManage.GetSession(user);
            if (v != null)
            {
                CurrentUserTag = v.Item2;
            }
            CurrentUserName = user;
        }
        public string GetToken()
        {
            return __token;
        }
        /// <summary>
        /// 保存Session
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <param name="tag"></param>
        protected void SaveSession(string user, string token, object tag = null)
        {
            ServerCreater.SessionManage.SaveSession(user, token, tag);
            __token = string.Format("{0}@{1}", user, token);
        }
        //HttpPostedFile postFile;
        //public void SetPostFile(HttpPostedFile file)
        //{
        //    postFile = file;
        //}
        ///// <summary>
        ///// 获取发送的文件
        ///// </summary>
        ///// <returns></returns>
        //protected HttpPostedFile GetPostFile()
        //{
        //    return postFile;
        //}
    }
}
