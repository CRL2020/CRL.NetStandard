using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;

namespace CRL.EventBus.Queue
{
    class RabbitMQ : AbsQueue
    {
        CRL.RabbitMQ.DirectRabbitMQ client;
        QueueConfig _queueConfig;
        string exchangeName = "CRLEventBusExc";

        public RabbitMQ(QueueConfig queueConfig, bool async)
        {
            Name = $"{Guid.NewGuid().ToString()}_{async}";
            _queueConfig = queueConfig;
            client = new CRL.RabbitMQ.DirectRabbitMQ(queueConfig.Host, queueConfig.User, queueConfig.Pass, exchangeName, async);
        }
        public override void Publish<T>(string routingKey, params T[] msgs)
        {
            if (string.IsNullOrEmpty(routingKey))
            {
                routingKey = msgs.First().GetType().Name;
            }
            client.Publish(routingKey, msgs);
        }

        public override void Subscribe(EventDeclare eventDeclare)
        {
            var queueName = _queueConfig.QueueName;
            if (!string.IsNullOrEmpty(eventDeclare.QueueName))
            {
                queueName = eventDeclare.QueueName;
            }
            var routingKey = eventDeclare.Name;
            if(eventDeclare.IsCopy)
            {
                routingKey = eventDeclare.GetArrayName();
            }
            //同步订阅
            client.BeginReceiveString(queueName, routingKey, OnReceiveString);
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
            //异步订阅
            client.BeginReceiveAsync(queueName, routingKey, OnReceiveAsync);
        }
        
        public override void Dispose()
        {
            client?.Dispose();
        }
        public override long CleanQueue(string name)
        {
            return client.CleanQueue(name);
        }
        public override long GetQueueLength(string name)
        {
            return client.GetQueueLength(name);
        }
    }
}
