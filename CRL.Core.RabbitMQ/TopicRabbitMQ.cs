using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using RabbitMQ.Client.Events;

namespace CRL.Core.RabbitMQ
{
    /// <summary>
    /// 主题队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TopicRabbitMQ : AbsRabbitMQ
    {
        public TopicRabbitMQ(string host, string user, string pass, string exchangeName) : base(host, user, pass)
        {
            __exchangeName = exchangeName;
            //channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, false, false, null);
            Log($"Topic队列: 初始化");
        }
        public void Publish(object msg, string routeKey)
        {
            if (!connection.IsOpen)
            {
                TryConnect();
            }
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(__exchangeName, ExchangeType.Topic, false, false, null);
                var sendBytes = Encoding.UTF8.GetBytes(msg.ToJson());
                //发布消息
                channel.BasicPublish(__exchangeName, routeKey, __basicProperties, sendBytes);
            }
        }
        public void BeginReceive<T>(Action<T> onReceive, string routingKey)
        {
            consumerChannel = CreateConsumerChannel((channel) =>
            {
                var queueName = channel.QueueDeclare().QueueName;
                //绑定队列到topic类型exchange，需指定路由键routingKey
                channel.QueueBind(queueName, __exchangeName, routingKey);
                Log($"消费队列名为:{queueName}");
                base.BaseBeginReceive(channel, queueName, onReceive);
            }); 
        }
    }
}
