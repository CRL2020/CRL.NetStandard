using CRL.RedisProvider;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CRL.Core.Extension;
namespace CRL.EventBus.Queue
{
    class Redis : AbsQueue
    {
        StackExchangeRedisHelper client;
        QueueConfig _queueConfig;
        public Redis(QueueConfig queueConfig)
        {
            client = new StackExchangeRedisHelper("");
            _queueConfig = queueConfig;
        }
        public override void Dispose()
        {
     
        }

        public override void Publish<T>(string routingKey, params T[] msgs)
        {
            client.Publish(routingKey, msgs.Select(b => b.ToJson()).ToArray());
        }
        public override void Subscribe(EventDeclare eventDeclare)
        {
            var queueName = _queueConfig.QueueName;
            if (!string.IsNullOrEmpty(eventDeclare.QueueName))
            {
                queueName = eventDeclare.QueueName;
            }
            var routingKey = eventDeclare.Name;
            if (eventDeclare.IsCopy)
            {
                routingKey = eventDeclare.GetArrayName();
            }
            client.Subscribe(routingKey, OnReceiveString);
        }

        public override void SubscribeAsync(EventDeclare eventDeclare)
        {
            var queueName = _queueConfig.QueueName;
            if (!string.IsNullOrEmpty(eventDeclare.QueueName))
            {
                queueName = eventDeclare.QueueName;
            }
            var routingKey = eventDeclare.Name;
            if (eventDeclare.IsCopy)
            {
                routingKey = eventDeclare.GetArrayName();
            }
            var task = client.SubscribeAsync(routingKey, OnReceiveString);
            task.Wait();
        }
    }
}
