using CRL.Core.EventBus.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.EventBus
{
    class QueueFactory
    {
        static ConcurrentDictionary<string, IQueue> clients = new ConcurrentDictionary<string, IQueue>();
        public static IQueue GetQueueClient()
        {
            var config = QueueConfig.GetConfig();
            var key = $"CRL_{config.QueueName}";
            var a = clients.TryGetValue(key, out IQueue client);
            if (!a)
            {
                client = CreateClient(config);
                clients.TryAdd(key, client);
            }
            return client;
        }
        static IQueue CreateClient(QueueConfig config)
        {
            //queueName = $"CRL_EVB_{queueName}";
            return new Queue.RabbitMQ(config);
        }
        public void DisposeAll()
        {
            foreach (var kv in clients)
            {
                var d = kv.Value as IDisposable;
                d.Dispose();
            }
        }
    }
}
