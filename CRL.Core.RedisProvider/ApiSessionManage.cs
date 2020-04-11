using CRL.Core.Remoting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.RedisProvider
{
    public class ApiSessionManage : ISessionManage
    {
        public ApiSessionData GetSession(string user)
        {
            var client = new RedisClient(3);
            var v = client.KGet<ApiSessionData>(user);
            if (v != null)
            {
                client.KSetEntryIn(user, TimeSpan.FromMinutes(20));
            }
            return v;
        }

        public void SaveSession(string user, ApiSessionData data)
        {
            data.ExpTime = DateTime.Now.AddMinutes(20);
            var client = new RedisClient(3);
            client.KSet(user, data, TimeSpan.FromMinutes(20));
        }
    }
}
