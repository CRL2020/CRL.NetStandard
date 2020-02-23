using CRL.Core.EventBus.Queue;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Core.EventBus
{
    public interface IPublisher
    {
        void Publish<T>(string queueName, T msg);
    }
    public class Publisher: IPublisher, IDisposable
    {
        IQueue queue;
        public Publisher()
        {
            queue = QueueFactory.CreateClient(QueueConfig.GetConfig(),false);
        }

        public void Dispose()
        {
            queue.Dispose();
        }

        public void Publish<T>(string name, T msg)
        {
            queue.Publish(name, msg);
        }
    }
}
