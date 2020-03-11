using CRL.Core.EventBus.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.EventBus
{
    public interface IPublisher
    {
        void Publish<T>(string name, params T[] msgs);
        Task PublishAsync<T>(string name, params T[] msgs);
    }
    public class Publisher : IPublisher, IDisposable
    {
        AbsQueue queue;
        public Publisher()
        {
            queue = QueueFactory.CreateClient(QueueConfig.GetConfig(), false);
        }

        public void Dispose()
        {
            queue.Dispose();
        }

        public void Publish<T>(string name, params T[] msgs)
        {
            queue.Publish(name, msgs);
        }
        public Task PublishAsync<T>(string name, params T[] msgs)
        {
            Publish(name, msgs);
            return Task.FromResult(true);
        }
    }
}
