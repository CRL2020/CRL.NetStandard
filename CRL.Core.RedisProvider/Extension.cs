using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.RedisProvider
{
    public static class Extension
    {
        /// <summary>
        /// 使用Resion
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="redisConn"></param>
        public static ConfigBuilder UseRedis(this ConfigBuilder builder,string redisConn)
        {
            RedisClient.GetRedisConn = () => redisConn;
            return builder;
        }
        /// <summary>
        /// 使用ResionSession
        /// </summary>
        /// <param name="builder"></param>
        public static ConfigBuilder UseRedisSession(this ConfigBuilder builder)
        {
            builder.__SessionCreater = (context) =>
            {
                return new RedisSession(context);
            };
            return builder;
        }
    }
}
