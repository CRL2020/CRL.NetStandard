using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.EventBus
{
    public interface IPublisher
    {
        void Publish<T>(string queueName, T msg);
    }
    public class Publisher: IPublisher
    {
        public void Publish<T>(string queueName, T msg)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                queueName = typeof(T).Name;
            }
            var client = QueueFactory.GetQueueClient(queueName, true);
            client.Publish(msg);
        }
    }
}
