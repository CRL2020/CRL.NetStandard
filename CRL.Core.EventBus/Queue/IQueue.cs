using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRL.Core.EventBus.Queue
{
    public interface IQueue : IDisposable
    {
        string Name { get; }
        void Publish(string routingKey, object msg);
        void Subscribe(EventDeclare eventDeclare);
        void SubscribeAsync(EventDeclare eventDeclare);
    }
}
