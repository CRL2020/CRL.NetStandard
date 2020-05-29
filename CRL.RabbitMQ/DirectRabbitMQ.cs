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
    public class DirectRabbitMQ : AbsRabbitMQ
    {
        //string queueName;
        public DirectRabbitMQ(string host, string user, string pass, string exchangeName, bool consumersAsync = false) : base(host, user, pass, consumersAsync)
        {
            __exchangeName = exchangeName;
            Log($"Direct队列:初始化");
        }
        public void Publish<T>(string routingKey, params T[] msgs)
        {
            BasePublish(ExchangeType.Direct, routingKey, msgs);
        }

        public void BeginReceive<T>(string queueName, string routingKey, Action<T,string> onReceive)
        {
            var channel = CreateConsumerChannel();
            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, __exchangeName, routingKey);
            Log($"开始消费,类型:Direct 队列:{queueName} Key:{routingKey}");
            base.BaseBeginReceive(channel, queueName, onReceive);
        }

        public void BeginReceiveString(string queueName, string routingKey, Action<string, string> onReceive)
        {
            var channel = CreateConsumerChannel();

            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, __exchangeName, routingKey);
            Log($"开始消费,类型:Direct 队列:{queueName} Key:{routingKey}");
            base.BaseBeginReceiveString(channel, queueName, onReceive);
        }
        public void BeginReceiveAsync(string queueName, string routingKey, Func<string, string, Task> onReceive)
        {
            var channel = CreateConsumerChannel();

            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, __exchangeName, routingKey);
            Log($"开始消费,类型:Direct 队列:{queueName} Key:{routingKey}");
            base.BaseBeginReceiveAsync(channel, queueName, onReceive);
        }
    }
}
