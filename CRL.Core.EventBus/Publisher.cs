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
        public void Publish<T>(string key, T msg)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = typeof(T).Name;
            }
            var ed = SubscribeService.GetEventDeclare(key);
            if (ed == null)
            {
                return;
            }
            var client = QueueFactory.GetQueueClient();
            client.Publish(key, msg);
        }
    }
}
