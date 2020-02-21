using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRL.Core.EventBus.Queue
{
    public interface IQueue : IDisposable
    {
        void Publish(object msg);
        void OnSubscribe(Type objType, int take, Action<System.Collections.IEnumerable> onReceive);
        void OnSubscribe(Type objType, Action<object> func);
        void OnSubscribeAsync(Type objType, Func<object, Task> func);
    }
}
