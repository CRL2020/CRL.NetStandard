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
        //Core.RabbitMQ.DirectRabbitMQ client2;
        string _host, _user, _pass, _queueName;
        string exchangeName = "CRLEventBusExc";
        //string routingKey => _queueName;
        public RabbitMQ(QueueConfig queueConfig)
        {
            //var queueName = "CRLEventBusQueue";
            _host = queueConfig.Host;
            _user = queueConfig.User;
            _pass = queueConfig.Pass;
            _queueName = queueConfig.QueueName;
            client = new Core.RabbitMQ.DirectRabbitMQ(_host, _user, _pass, exchangeName);
        }
        public void Publish(string routingKey, object msg)
        {
            client.Publish(routingKey, msg);
        }
        //Queue<Tuple<string,object>> localQueue = new Queue<Tuple<string, object>>();
        //Core.ThreadWork threadWork;
        //DateTime publishTime = DateTime.Now;

        ///// <summary>
        ///// 批量订阅处理
        ///// </summary>
        //public void OnSubscribe(string routingKey, Type objType, int take, Action<System.Collections.IEnumerable> onReceive)
        //{
        //    client2 = new Core.RabbitMQ.DirectRabbitMQ(_host, _user, _pass, exchangeName);
        //    threadWork = new ThreadWork();
        //    threadWork.Start("OnSubscribeIEnumerable<T>", () =>
        //     {
        //         var ts = DateTime.Now - publishTime;
        //         if (ts.TotalSeconds > 1)
        //         {
        //             rePublish(routingKey, take);
        //         }
        //         return true;
        //     }, 0.5);
        //    var typeInner = objType.GetGenericArguments()[0];
        //    client.BeginReceive(_queueName, routingKey, typeInner, msg =>
        //      {
        //          localQueue.Enqueue(msg);
        //          publishTime = DateTime.Now;
        //         //转成集合
        //         if (localQueue.Count >= take)
        //          {
        //              rePublish(routingKey, take);
        //          }
        //      });

        //    client2.BeginReceive(_queueName + "_L", routingKey + "_L", objType, msgs =>
        //     {
        //         var list = msgs as System.Collections.IEnumerable;
        //         onReceive(list);
        //     });
        //}
        //void rePublish(EventDeclare ed)
        //{
        //    int i = 0;
        //    var list = new List<object>();
        //    while (i <= ed.ListTake && ed.CacheData.Count > 0)
        //    {
        //        var obj = ed.CacheData.Dequeue();
        //        list.Add(obj);
        //        i += 1;
        //    }
        //    ed.CacheDataTime = DateTime.Now;
        //    if (list.Count > 0)
        //    {
        //        client.Publish(ed.Name, list);
        //    }
        //}

        public void Subscribe(EventDeclare eventDeclare)
        {
            client.BeginReceiveString(_queueName, eventDeclare.Name, (msg, key) =>
            {
                var ed = SubscribeService.GetEventDeclare(key);
                if (ed == null)
                {
                    return;
                }

                if (ed.IsArry)
                {
                    var obj = msg.ToObject(ed.DataType.GenericTypeArguments[0]);
                    ed.CacheData.Enqueue(obj);
                    ed.CacheDataTime = DateTime.Now;
                    //转成集合
                    if (ed.CacheData.Count >= ed.ListTake)
                    {
                        ed.rePublish(key + "_L");
                    }
                }
                else
                {
                    var obj = msg.ToObject(ed.DataType);
                    ed.MethodInvoke.Invoke(ed.InstanceInvoke(), new object[] { obj });
                }
            });
        }
        //public void OnSubscribeAsync(string routingKey, Type objType, Func<object, Task> onReceive)
        //{
        //    client.BeginReceiveAsync(_queueName, routingKey, objType, msg =>
        //    {
        //        var obj = msg.ToObject(objType);
        //        return onReceive(obj);
        //    });
        //}
        public void Dispose()
        {
            client?.Dispose();
            //client2?.Dispose();
        }
    }
}
