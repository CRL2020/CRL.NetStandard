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
    public abstract class AbsRabbitMQ : IDisposable
    {
        protected IConnection connection;
        protected IModel consumerChannel;
        protected string __exchangeName = "";
        protected IBasicProperties __basicProperties;

        public bool IsOpen
        {
            get
            {
                return connection != null && connection.IsOpen;
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
        object sync_root = new object();
        public void TryConnect()
        {
            lock (sync_root)
            {
                int i = 0;
                while (!IsOpen && i < 5)
                {
                    try
                    {
                        CreateConnect();
                    }
                    catch (Exception ero) {
                        Log("Connection eror" + ero.Message);
                    }
                    i++;
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        void CreateConnect()
        {
            //创建连接
            connection = factory.CreateConnection();

            connection.ConnectionShutdown += (s, e) =>
            {
                Log("RabbitMQ ConnectionShutdown");
                TryConnect();
            };
            connection.CallbackException += (s, e) =>
            {
                Log("RabbitMQ CallbackException");
                TryConnect();
            };
            connection.ConnectionBlocked += (s, e) =>
            {
                Log("RabbitMQ ConnectionBlocked");
                TryConnect();
            };
            //创建通道
            //channel = connection.CreateModel();
            Log($"{factory.HostName} 连接成功");
        }
        protected void Log(string msg)
        {
            Console.WriteLine(string.Format("RabbitMQ: {0}", msg));
            CRL.Core.EventLog.Log(msg, "RabbitMQ");
        }

        protected void BaseBeginReceive<T>(IModel channel, string queueName, Action<T> onReceive)
        {
            BaseBeginReceive(channel,typeof(T), queueName, msg =>
              {
                  var obj = (T)msg;
                  onReceive(obj);
              });
        }

        protected void BaseBeginReceive(IModel channel, Type type, string queueName, Action<object> onReceive)
        {
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
                    Log($"{queueName}订阅消息时发生错误{ero}");
                    throw ero;
                }

                //确认该消息已被消费
                channel.BasicAck(ea.DeliveryTag, false);
            };
            //7. 启动消费者
            channel.BasicConsume(queueName, false, consumer);
        }
        protected void BaseBeginReceiveAsync(IModel channel, Type type, string queueName, Func<object, Task> onReceive)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            //6. 绑定消息接收后的事件委托
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                var obj = message.ToObject(type);
                try
                {
                    await onReceive.Invoke(obj);
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
        protected IModel CreateConsumerChannel(Action<IModel> func)
        {
            if (!connection.IsOpen)
            {
                TryConnect();
            }

            Log("Creating RabbitMQ consumer channel");

            var channel = connection.CreateModel();
            func(channel);
            channel.CallbackException += (sender, ea) =>
            {
                Log(ea.Exception.Message + " Recreating RabbitMQ consumer channel");

                //consumerChannel.Dispose();
                //consumerChannel = CreateConsumerChannel(func);
            };
            return channel;
        }
        public void Dispose()
        {
            consumerChannel?.Dispose();
            connection?.Dispose();
        }
    }
}
