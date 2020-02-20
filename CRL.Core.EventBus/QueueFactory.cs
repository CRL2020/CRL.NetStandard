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
        public static IQueue GetQueueClient(string queueName, bool publisher)
        {
            var config = QueueConfig.GetConfig();
            var key = $"{queueName}_{1}";
            var a = clients.TryGetValue(key, out IQueue client);
            if (!a)
            {
                client = CreateClient(config, queueName);
                clients.TryAdd(key, client);
            }
            return client;
        }
        static IQueue CreateClient(QueueConfig config, string queueName)
        {
            return new Queue.RabbitMQ(config, queueName);
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
