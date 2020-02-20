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
        string __queueName;
        public DirectRabbitMQ(string host, string user, string pass, string queueName) : base(host, user, pass)
        {
            __queueName = queueName;
            __exchangeName = "defaultDirectExc";
            channel.ExchangeDeclare(__exchangeName, ExchangeType.Direct, false, false, null);
            channel.QueueDeclare(queueName, true, false, false, null);
            Log($"队列:{queueName}初始化");
        }
        public void Publish(object msg)
        {
            var sendBytes = Encoding.UTF8.GetBytes(msg.ToJson());
            var routingKey = __queueName;
            channel.BasicPublish(__exchangeName, routingKey, __basicProperties, sendBytes);
        }

        public void BeginReceive<T>(Action<T> onReceive)
        {
            Log($"队列:{__queueName} 开始订阅");
            var routingKey = __queueName;
            channel.QueueBind(__queueName, __exchangeName, routingKey);
            base.BaseBeginReceive(__queueName,onReceive);
        }

        public void BeginReceive(Type type, Action<object> onReceive)
        {
            Log($"队列:{__queueName} 开始订阅");
            var routingKey = __queueName;
            channel.QueueBind(__queueName, __exchangeName, routingKey);
            var consumer = new EventingBasicConsumer(channel);
            //6. 绑定消息接收后的事件委托
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                var obj = message.ToObject(type);
                try
                {
                    onReceive(obj);

                }
                catch (Exception ero)
                {
                    Log($"{__queueName}订阅消息时发生错误{ero}");
                    throw ero;
                }

                //确认该消息已被消费
                channel.BasicAck(ea.DeliveryTag, false);
            };
            //7. 启动消费者
            channel.BasicConsume(__queueName, false, consumer);
        }


    }
}
