using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace CRL.Core.RabbitMQ
{
    //https://www.cnblogs.com/sheng-jie/p/7192690.html
    //https://www.cnblogs.com/julyluo/p/6265775.html
    public abstract class AbsRabbitMQ<T> : IDisposable
    {
        protected IConnection connection;
        protected IModel channel;
        protected string __exchangeName = "";
        protected IBasicProperties __basicProperties;

        public bool IsOpen
        {
            get
            {
                return connection.IsOpen;
            }
        }
        ConnectionFactory factory;
        public AbsRabbitMQ(string host, string user, string pass)
        {
            factory = new ConnectionFactory
            {
                UserName = user,//用户名
                Password = pass,//密码
                HostName = host,//rabbitmq ip
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            };
            CreateConnect();
        }
        public Action OnDisconnect;

        void CreateConnect()
        {
            //创建连接
            connection = factory.CreateConnection();

            connection.ConnectionShutdown += (s, e) =>
            {
                Log("RabbitMQ断开");
                OnDisconnect?.Invoke();
            };
            //创建通道
            channel = connection.CreateModel();
            Log($"{factory.HostName} 连接成功");
        }
        protected void Log(string msg)
        {
            Console.WriteLine(string.Format("RabbitMQ: {0}", msg));
            CRL.Core.EventLog.Log(msg, "RabbitMQ");
        }

        protected void BaseBeginReceive(string queueName, Action<T> onReceive)
        {
            var consumer = new EventingBasicConsumer(channel);
            //6. 绑定消息接收后的事件委托
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                var obj = message.ToObject<T>();
                try
                {
                    onReceive(obj);

                }
                catch (Exception ero)
                {
                    Log($"{queueName}订阅消息时发生错误{ero}");
                    throw ero;
                }

                //确认该消息已被消费
                channel.BasicAck(ea.DeliveryTag, false);
            };
            //7. 启动消费者
            channel.BasicConsume(queueName, false, consumer);
        }
        public void Dispose()
        {
            channel.Dispose();
            connection.Dispose();
        }
    }
}
