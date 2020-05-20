using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core.Log
{
    #region LogItem
    [Serializable]
    public class LogItem
    {
        internal string Path;
        internal string Id;
        public DateTime Time
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        public string Detail
        {
            get;
            set;
        }
        public string RequestUrl
        {
            get;
            set;
        }
        public string UrlReferrer
        {
            get;
            set;
        }
        public string UserIP
        {
            get;
            set;
        }
        public string UserAgent
        {
            get;
            set;
        }

        public string Method
        {
            get; set;
        }
        public override string ToString()
        {
            var s = new StringBuilder(Time.ToString("yy-MM-dd HH:mm:ss fffff"));
            if (string.IsNullOrEmpty(Title))
            {
                Title = Detail;
                Detail = "";
            }
            if (!string.IsNullOrEmpty(Title))
            {
                s.Append("  " + Title);
            }
            if (!string.IsNullOrEmpty(RequestUrl))
            {
                s.Append("\r\nUrl:" + RequestUrl);
            }
            if (!string.IsNullOrEmpty(Method))
            {
                s.Append("\r\nMethod:" + Method);
            }
            if (!string.IsNullOrEmpty(UrlReferrer))
            {
                s.Append("\r\nUrlReferrer:" + UrlReferrer);
            }
            if (!string.IsNullOrEmpty(UserIP))
            {
                s.Append("\r\nUserIP:" + UserIP);
            }
            if (!string.IsNullOrEmpty(UserAgent))
            {
                s.Append("\r\n" + UserAgent);
            }
            if (!string.IsNullOrEmpty(Detail))
            {
                s.Append("\r\n" + Detail);
            }
            s.Append("\r\n");
            return s.ToString();
        }
    }
    #endregion

    public class HttpContext
    {
        public string RequestUrl
        {
            get;
            set;
        }
        public string UrlReferrer
        {
            get;
            set;
        }
        public string UserIP
        {
            get;
            set;
        }
        public string UserAgent
        {
            get;
            set;
        }

        public string Method
        {
            get; set;
        }
    }
}
