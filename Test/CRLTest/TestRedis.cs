using System;
using System.Collections.Generic;
using System.Text;
using CRL.RedisProvider;
namespace CRLTest
{
    class TestRedis
    {
        static TestRedis()
        {
            var configBuilder = new CRL.Core.ConfigBuilder();
            configBuilder.UseRedis(t =>
            {
                return "Server_204@127.0.0.1:6389";
            });
            configBuilder.UseRedisSession();
        }
        public static void Insert()
        {
            var client = new RedisClient();
            int count = 15000;
            for (int i = 0; i < count; i++)
            {
                var key = $"testKey_{i}";
                client.KSet(key, 1, null);
                Console.WriteLine($"set {key} {i}/{count}");
            }
        }
        public static void Remove()
        {
            var client = new RedisClient();
            client.BatchRemove("testKey_*");
        }
    }
}
