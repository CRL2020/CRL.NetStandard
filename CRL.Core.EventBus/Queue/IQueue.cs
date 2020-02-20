using System;
using System.Collections.Generic;

namespace CRL.Core.EventBus.Queue
{
    public interface IQueue : IDisposable
    {
        void Publish(object msg);
        void OnSubscribe(Type objType, int take, Action<System.Collections.IEnumerable> onReceive);
        void OnSubscribe(Type objType, Action<object> func);
    }
}
