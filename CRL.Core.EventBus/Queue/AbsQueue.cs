using CRL.Core.Extension;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRL.Core.EventBus.Queue
{
    abstract class AbsQueue : IDisposable
    {
        public string Name { get; set; }

        public abstract void Dispose();

        public abstract void Publish(string routingKey, object msg);
        public abstract void Publish(string routingKey, IEnumerable<object> msgs);
        public abstract void Subscribe(EventDeclare eventDeclare);
        public abstract void SubscribeAsync(EventDeclare eventDeclare);

        #region inner
        protected internal void OnReceiveString(string msg, string key)
        {
            var ed = SubscribeService.GetEventDeclare(key);
            if (ed == null)
            {
                return;
            }

            if (ed.IsArray && !ed.IsCopy)
            {
                var obj = msg.ToObject(ed.EventDataType.GenericTypeArguments[0]);
                var ed2 = SubscribeService.GetEventDeclare(ed.GetArrayName());
                ed2.setCache(obj);
                //转成集合
                if (ed2.CacheData.Count >= ed2.ListTake)
                {
                    ed2.rePublish();
                }
            }
            else
            {
                var obj = msg.ToObject(ed.EventDataType);
                ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { obj });

            }
        }
        protected internal Task OnReceiveAsync(string msg, string key)
        {
            var ed = SubscribeService.GetEventDeclare(key);
            if (ed == null)
            {
                return Task.FromResult<string>(null);
            }
            if (ed.IsArray && !ed.IsCopy)
            {

                var obj = msg.ToObject(ed.EventDataType.GenericTypeArguments[0]);
                var ed2 = SubscribeService.GetEventDeclare(ed.GetArrayName());
                ed2.setCache(obj);
                //转成集合
                if (ed2.CacheData.Count >= ed2.ListTake)
                {
                    ed2.rePublish();
                }
                return Task.FromResult<string>(null);
            }
            else
            {
                var obj = msg.ToObject(ed.EventDataType);
                return (Task)ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { obj });

            }
        }
        #endregion
    }
}
