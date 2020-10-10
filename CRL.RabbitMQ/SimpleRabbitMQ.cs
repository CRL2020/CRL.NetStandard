using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CRL.RabbitMQ
{
    /// <summary>
    /// 直连队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleRabbitMQ : AbsRabbitMQ
    {
        string __queueName;
        public SimpleRabbitMQ(string host, string user, string pass, string queueName, bool consumersAsync = false) : base(host, user, pass, consumersAsync)
        {
            __queueName = queueName;
            Log($"SimpleDirect队列:初始化");
        }
        public void Publish<T>(params T[] msgs)
        {
            BasePublish(ExchangeType.Direct, __queueName, msgs);
        }

        public void BeginReceive<T>(Action<T,string> onReceive)
        {
            var channel = CreateConsumerChannel();
            channel.QueueDeclare(__queueName, true, false, false, null);
            Log($"开始消费,类型:SimpleDirect 队列:{__queueName}");
            base.BaseBeginReceive(channel, __queueName, onReceive);
        }

        public void BeginReceiveString(Action<string, string> onReceive)
        {
            var channel = CreateConsumerChannel();

            channel.QueueDeclare(__queueName, true, false, false, null);
            Log($"开始消费,类型:SimpleDirect 队列:{__queueName}");
            base.BaseBeginReceiveString(channel, __queueName, onReceive);
        }
        public void BeginReceiveAsync(Func<string, string, Task> onReceive)
        {
            var channel = CreateConsumerChannel();

            channel.QueueDeclare(__queueName, true, false, false, null);
            Log($"开始消费,类型:SimpleDirect 队列:{__queueName}");
            base.BaseBeginReceiveAsync(channel, __queueName, onReceive);
        }
    }
}
