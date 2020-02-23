using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CRL.Core.RabbitMQ
{
    /// <summary>
    /// 直连队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DirectRabbitMQ : AbsRabbitMQ
    {
        //string queueName;
        public DirectRabbitMQ(string host, string user, string pass, string exchangeName, bool consumersAsync) : base(host, user, pass, consumersAsync)
        {
            //queueName = queueName;
            __exchangeName = exchangeName;
            //channel.ExchangeDeclare(__exchangeName, ExchangeType.Direct, false, false, null);
        }
        public void Publish(string routingKey, object msg)
        {
            if (!connection.IsOpen)
            {
                TryConnect();
            }
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(__exchangeName, ExchangeType.Direct, false, false, null);
                var sendBytes = Encoding.UTF8.GetBytes(msg.ToJson());
                //var routingKey = queueName;
                channel.BasicPublish(__exchangeName, routingKey, __basicProperties, sendBytes);
            }
        }

        public void BeginReceive<T>(string queueName, string routingKey, Action<T,string> onReceive)
        {
            var channel = CreateConsumerChannel();
            consumerChannel = channel;

            channel.QueueDeclare(queueName, true, false, false, null);
            Log($"队列:{queueName} 开始订阅 {routingKey}");
            //var routingKey = queueName;
            channel.QueueBind(queueName, __exchangeName, routingKey);
            base.BaseBeginReceive(channel, queueName, onReceive);
        }

        public void BeginReceiveString(string queueName, string routingKey, Action<string, string> onReceive)
        {
            var channel = CreateConsumerChannel();
            consumerChannel = channel;

            channel.QueueDeclare(queueName, true, false, false, null);
            Log($"队列:{queueName} 开始订阅 {routingKey}");
            //var routingKey = queueName;
            channel.QueueBind(queueName, __exchangeName, routingKey);
            base.BaseBeginReceiveString(channel, queueName, onReceive);
        }
        public void BeginReceiveAsync(string queueName, string routingKey, Func<string, string, Task> onReceive)
        {
            var channel = CreateConsumerChannel();
            consumerChannel = channel;

            channel.QueueDeclare(queueName, true, false, false, null);
            Log($"队列:{queueName} 开始订阅 {routingKey}");
            //var routingKey = queueName;
            channel.QueueBind(queueName, __exchangeName, routingKey);
            base.BaseBeginReceiveAsync(channel, queueName, onReceive);
        }
    }
}
