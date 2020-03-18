using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.EventBus
{
    internal enum MQType
    {
        RabbitMQ,
        Redis,
        MongoDb
    }
    public class QueueConfig
    {
        public static QueueConfig Instance;
        static QueueConfig()
        {
            Instance = new QueueConfig();
        }
        public void UseRabbitMQ(string host,string user,string pass)
        {
            var config = new QueueConfig()
            {
                Host = host,
                Pass = pass,
                User = user,
                MQType = MQType.RabbitMQ
            };
            Instance = config;
        }
        public void UseRedis(string conn)
        {
            var cb = new CRL.Core.ConfigBuilder();
            RedisProvider.Extension.UseRedis(cb, conn);
            var config = new QueueConfig();
            config.ConnString = conn;
            config.MQType = MQType.Redis;
            Instance = config;
        }
        public void UseMongoDb(string conn)
        {
            var cb = new CRL.Core.ConfigBuilder();
            var config = new QueueConfig();
            config.ConnString = conn;
            config.MQType = MQType.MongoDb;
            Instance = config;
        }
        internal string Host;
        internal string User;
        internal string Pass;
        internal string QueueName = "EventBusQueue";
        internal MQType MQType;

        internal string ConnString { get; set; }

        internal static QueueConfig GetConfig()
        {
            return Instance;
        }

    }
}
