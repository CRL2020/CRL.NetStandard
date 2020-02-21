using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRL.Core.EventBus.Queue
{
    public interface IQueue : IDisposable
    {
        void Publish(string routingKey, object msg);
        //void OnSubscribe(string routingKey, Type objType, int take, Action<System.Collections.IEnumerable> onReceive);
        void Subscribe(EventDeclare eventDeclare);
        //void OnSubscribeAsync(string routingKey, Type objType, Func<object, Task> func);
    }
}
