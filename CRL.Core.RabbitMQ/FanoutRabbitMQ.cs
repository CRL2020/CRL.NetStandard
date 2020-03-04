using CRL.Core.Extension;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.RabbitMQ
{
    /// <summary>
    /// 广播队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FanoutRabbitMQ: AbsRabbitMQ
    {
        public FanoutRabbitMQ(string host, string user, string pass, string exchangeName, bool consumersAsync = false) : base(host, user, pass, consumersAsync)
        {
            __exchangeName = exchangeName;
            Log($"Fanout队列:初始化");
        }
        public virtual void Publish(object msg)
        {
            if (!connection.IsOpen)
            {
                TryConnect();
            }
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(__exchangeName, ExchangeType.Fanout, false, false, null);
                var sendBytes = Encoding.UTF8.GetBytes(msg.ToJson());
                //发布消息
                channel.BasicPublish(__exchangeName, "", __basicProperties, sendBytes);
            }
        }

        public void BeginReceive<T>(Action<T, string> onReceive)
        {
            var channel = CreateConsumerChannel();
    
            var queueName = channel.QueueDeclare().QueueName;
            //绑定队列到指定fanout类型exchange，无需指定路由键
            channel.QueueBind(queue: queueName, exchange: __exchangeName, routingKey: "");
            Log($"开始消费,类型: Fanout 队列:{queueName} Key:");
            base.BaseBeginReceive(channel, queueName, onReceive);

        }
    }
}
