using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using RabbitMQ.Client;

namespace CRL.Core.RabbitMQ
{
    /// <summary>
    /// 直连队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DirectRabbitMQ<T> : AbsRabbitMQ<T>
    {
        string __queueName;
        public DirectRabbitMQ(string host, string user, string pass, string queueName) : base(host, user, pass)
        {
            __queueName = queueName;
            __exchangeName = "defaultDirectExc";
            channel.ExchangeDeclare(__exchangeName, ExchangeType.Direct, false, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            Log($"队列:{queueName}初始化");
        }
        public void Publish(T msg)
        {
            var sendBytes = Encoding.UTF8.GetBytes(msg.ToJson());
            var routingKey = __queueName;
            channel.BasicPublish(__exchangeName, routingKey, __basicProperties, sendBytes);
        }

        public void BeginReceive(Action<T> onReceive)
        {
            Log($"队列:{__queueName} 开始订阅");
            var routingKey = __queueName;
            channel.QueueBind(__queueName, __exchangeName, routingKey);
            base.BaseBeginReceive(__queueName,onReceive);
        }

    }
}
