using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CRL.Core.Remoting
{
    public interface ISessionManage
    {
        void SaveSession(string user, ApiSessionData data);
        //bool CheckSession(string user, string token, ParameterInfo[] argsName, List<object> args, out string error);
        ApiSessionData GetSession(string user);
        //void RefreshSession(string user);
    }
    public class ApiSessionData
    {
        public ApiSessionData(string token,string tag)
        {
            Token = token;
            Tag = tag;
        }
        public string Token
        {
            get;set;
        }
        public string Tag
        {
            get; set;
        }
        public DateTime ExpTime
        {
            get; set;
        }
    }
    public class SignCheck
    {
        static string getObjSign(Type type, object obj)
        {
            string sign = "";
            var dic = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (obj == null)
            {
                return sign;
            }
            var pro = type.GetProperties();
            foreach (var p in pro)
            {
                var value = p.GetValue(obj);
                if (p.PropertyType.IsEnum || p.PropertyType == typeof(bool))
                {
                    value = Convert.ToInt32(value);
                }
                else if (p.PropertyType == typeof(DateTime))
                {
                    value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (p.PropertyType == typeof(decimal))
                {
                    value = ((decimal)value).ToString("F");
                }
                dic.Add(p.Name.ToLower(), value);
            }
            var list = new List<string>();
            foreach (var kv in dic)
            {
                list.Add(string.Format("{0}={1}", kv.Key, kv.Value));
            }
            sign = string.Join("&", list);
            return sign;
        }
        public static string CreateSign(string key, ParameterInfo[] argsName, List<object> args)
        {
            var list = new List<string>();
            for (int i = 0; i < argsName.Length; i++)
            {
                var p = argsName[i];
                var value = args[i];
                if(p.ParameterType.IsClass)
                {
                    list.Add(getObjSign(p.ParameterType, value));
                }
                else
                {
                    list.Add(string.Format("{0}={1}", p.Name, value));
                }
            }
            list.RemoveAll(b => b == "");
            var str = string.Join("&", list);
            var sign = Core.StringHelper.EncryptMD5(str + "&" + key);
            return sign;
        }
    }
    public class SessionManage : ISessionManage
    {
        static ConcurrentDictionary<string, ApiSessionData> sessions = new ConcurrentDictionary<string, ApiSessionData>();
        /// <summary>
        /// 登录后返回新的TOKEN
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        public void SaveSession(string user, ApiSessionData data)
        {
            data.ExpTime = DateTime.Now.AddMinutes(20);
            if (!sessions.TryGetValue(user, out ApiSessionData token2))
            {
                sessions.TryAdd(user, data);
            }
            else
            {
                sessions[user] = data;
            }
        }

     
        public ApiSessionData GetSession(string user)
        {
            sessions.TryGetValue(user, out ApiSessionData v);
            return v;
        }
    }
}
