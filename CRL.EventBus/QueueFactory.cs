using CRL.EventBus.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CRL.EventBus
{
    class QueueFactory
    {
        static ConcurrentDictionary<string, AbsQueue> clients = new ConcurrentDictionary<string, AbsQueue>();
        public static AbsQueue GetQueueClient(QueueConfig config, EventDeclare eventDeclare)
        {
            var _queueName = config.QueueName;
            var key = $"CRL_{_queueName}_{eventDeclare.IsAsync}";
            var a = clients.TryGetValue(key, out AbsQueue client);
            if (!a)
            {
                client = CreateClient(config, eventDeclare.IsAsync);
                clients.TryAdd(key, client);
            }
            return client;
        }
        public static AbsQueue CreateClient(QueueConfig config, bool async)
        {
            AbsQueue instance = null;
            switch (config.MQType)
            {
                case MQType.RabbitMQ:
                    instance = new Queue.RabbitMQ(config, async);
                    break;
                case MQType.Redis:
                    instance = new Queue.Redis(config);
                    break;
                case MQType.MongoDb:
                    instance = new Queue.MongoDb(config);
                    break;
            }
            return instance;
        }
        public static void DisposeAll()
        {
            foreach (var kv in clients)
            {
                var d = kv.Value as IDisposable;
                d.Dispose();
            }
        }
    }
}
