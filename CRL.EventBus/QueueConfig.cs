using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CRL.EventBus
{
    internal enum MQType
    {
        RabbitMQ,
        Redis,
        MongoDb
    }
    public class QueueConfig
    {

        public void UseRabbitMQ(string host,string user,string pass)
        {
            Host = host;
            Pass = pass;
            User = user;
            MQType = MQType.RabbitMQ;
        }
        public void UseRedis(string conn)
        {
            var cb = new CRL.Core.ConfigBuilder();
            RedisProvider.Extension.UseRedis(cb, conn);
            ConnString = conn;
            MQType = MQType.Redis;
        }
        public void UseMongoDb(string conn)
        {
            ConnString = conn;
            MQType = MQType.MongoDb;
        }
        internal string Host;
        internal string User;
        internal string Pass;
        internal string QueueName = "EventBusQueue";
        internal MQType MQType;

        internal string ConnString { get; set; }

        internal Assembly[] SubscribeAssemblies;

    }
}
