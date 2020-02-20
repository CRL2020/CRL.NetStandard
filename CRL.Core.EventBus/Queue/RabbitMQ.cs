using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.EventBus.Queue
{
    class RabbitMQ : IQueue
    {
        Core.RabbitMQ.DirectRabbitMQ client;
        Core.RabbitMQ.DirectRabbitMQ client2;
        string _host, _user, _pass, _queueName;
        public RabbitMQ(QueueConfig queueConfig, string queueName)
        {
            _host = queueConfig.Host;
            _user = queueConfig.User;
            _pass = queueConfig.Pass;
            _queueName = queueName;
            client = new Core.RabbitMQ.DirectRabbitMQ(_host, _user, _pass, queueName);
        }
        public void Publish(object msg)
        {
            client.Publish(msg);
        }
        Queue<object> localQueue = new Queue<object>();
        Core.ThreadWork threadWork;
        DateTime publishTime = DateTime.Now;

        /// <summary>
        /// 批量订阅处理
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="take"></param>
        /// <param name="onReceive"></param>
        public void OnSubscribe(Type objType, int take, Action<System.Collections.IEnumerable> onReceive)
        {
            client2 = new Core.RabbitMQ.DirectRabbitMQ(_host, _user, _pass, _queueName + "_m");
            threadWork = new ThreadWork();
            threadWork.Start("OnSubscribeIEnumerable<T>", () =>
             {
                 var ts = DateTime.Now - publishTime;
                 if (ts.TotalSeconds > 1)
                 {
                     rePublish(take);
                 }
                 return true;
             }, 1);
            var typeInner = objType.GetGenericArguments()[0];
            client.BeginReceive(typeInner, msg =>
             {
                 localQueue.Enqueue(msg);
                 publishTime = DateTime.Now;
                 //转成集合
                 if (localQueue.Count >= take)
                 {
                     rePublish(take);
                 }
             });

            client2.BeginReceive(objType, msgs =>
             {
                 var list = msgs as System.Collections.IEnumerable;
                 onReceive(list);
             });
        }
        void rePublish(int take)
        {
            int i = 0;
            var list = new List<object>();
            while (i <= take && localQueue.Count > 0)
            {
                var obj = localQueue.Dequeue();
                list.Add(obj);
                i += 1;
            }
            publishTime = DateTime.Now;
            if (list.Count > 0)
            {
                client2.Publish(list);
            }
        }

        public void OnSubscribe(Type objType, Action<object> onReceive)
        {
            client.BeginReceive(objType, onReceive);
        }
        public void Dispose()
        {
            client?.Dispose();
            client2?.Dispose();
        }
    }
}
