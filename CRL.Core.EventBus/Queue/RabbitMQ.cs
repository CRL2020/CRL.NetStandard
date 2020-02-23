using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace CRL.Core.EventBus.Queue
{
    class RabbitMQ : IQueue
    {
        Core.RabbitMQ.DirectRabbitMQ client;
        QueueConfig _queueConfig;
        string exchangeName = "CRLEventBusExc";
        public string Name
        {
            get;
        }
        public RabbitMQ(QueueConfig queueConfig, bool async)
        {
            Name = $"{Guid.NewGuid().ToString()}_{async}";
            _queueConfig = queueConfig;
            client = new Core.RabbitMQ.DirectRabbitMQ(queueConfig.Host, queueConfig.User, queueConfig.Pass, exchangeName, async);
        }
        public void Publish(string routingKey, object msg)
        {
            if (string.IsNullOrEmpty(routingKey))
            {
                routingKey = msg.GetType().Name;
            }
            client.Publish(routingKey, msg);
        }
  
        public void Subscribe(EventDeclare eventDeclare)
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

        public void SubscribeAsync(EventDeclare eventDeclare)
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
        #region inner
        void OnReceiveString(string msg, string key)
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
                try
                {
                    ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { obj });
                }
                catch (Exception ero)
                {
                    Console.WriteLine($"{key}订阅消息时发生错误{ero}");
                    throw ero;
                }
    
            }
        }
        Task OnReceiveAsync(string msg, string key)
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
                try
                {
                    return (Task)ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { obj });
                }
                catch (Exception ero)
                {
                    Console.WriteLine($"{key}订阅消息时发生错误{ero}");
                    throw ero;
                }

            }
        }
        #endregion
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
