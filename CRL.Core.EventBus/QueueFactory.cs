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
        public static IQueue GetQueueClient(QueueConfig config, EventDeclare eventDeclare)
        {
            var _queueName = config.QueueName;
            if (!string.IsNullOrEmpty(eventDeclare.QueueName))//每个队列一个客户端
            {
                _queueName = eventDeclare.QueueName;
            }
            var key = $"CRL_{_queueName}_{eventDeclare.IsAsync}";
            var a = clients.TryGetValue(key, out IQueue client);
            if (!a)
            {
                client = CreateClient(config, eventDeclare.IsAsync);
                clients.TryAdd(key, client);
            }
            return client;
        }
        public static IQueue CreateClient(QueueConfig config, bool async)
        {
            return new Queue.RabbitMQ(config, async);
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
