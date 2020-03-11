using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using System.Threading;
using System.Collections.Concurrent;

namespace CRL.Core.RabbitMQ
{
    //https://www.cnblogs.com/sheng-jie/p/7192690.html
    //https://www.cnblogs.com/julyluo/p/6265775.html
    public abstract class AbsRabbitMQ : IDisposable
    {
        protected IConnection connection;
        protected List<IModel> consumerChannels = new List<IModel>();
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
        public AbsRabbitMQ(string host, string user, string pass, bool consumersAsync = false)
        {
            factory = new ConnectionFactory
            {
                UserName = user,//用户名
                Password = pass,//密码
                HostName = host,//rabbitmq ip
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                DispatchConsumersAsync = consumersAsync
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

        protected void BasePublish<T>(string exchangeType, string routingKey, params T[] msgs)
        {
            var channel = RentChannel();
            try
            {
                channel.ExchangeDeclare(__exchangeName, exchangeType, false, false, null);
                foreach (var msg in msgs)
                {
                    var sendBytes = Encoding.UTF8.GetBytes(msg.ToJson());
                    channel.BasicPublish(__exchangeName, routingKey, __basicProperties, sendBytes);
                }
            }
            catch (Exception ero)
            {
                throw ero;
            }
            finally
            {
                ReturnChannel(channel);
            }
        }


        protected void BaseBeginReceive<T>(IModel channel, string queueName, Action<T,string> onReceive)
        {
            BaseBeginReceiveString(channel, queueName, (msg,key) =>
              {
                  var obj = msg.ToObject<T>();
                  onReceive(obj, key);
              });
        }

        protected void BaseBeginReceiveString(IModel channel, string queueName, Action<string,string> onReceive)
        {
            var consumer = new EventingBasicConsumer(channel);
            //6. 绑定消息接收后的事件委托
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                //var obj = message.ToObject(type);
                onReceive(message, ea.RoutingKey);

                //确认该消息已被消费
                channel.BasicAck(ea.DeliveryTag, false);
            };
            //7. 启动消费者
            channel.BasicConsume(queueName, false, consumer);
        }
        protected void BaseBeginReceiveAsync(IModel channel,string queueName, Func<string,string, Task> onReceive)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            //6. 绑定消息接收后的事件委托
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body);
                //var obj = message.ToObject(type);
                await onReceive.Invoke(message, ea.RoutingKey);

                //确认该消息已被消费
                channel.BasicAck(ea.DeliveryTag, false);
            };
            //7. 启动消费者
            channel.BasicConsume(queueName, false, consumer);
        }
        protected IModel CreateConsumerChannel()
        {
            if (!connection.IsOpen)
            {
                TryConnect();
            }
            var channel = connection.CreateModel();
            //func(channel);
            channel.CallbackException += (sender, ea) =>
            {
                Log("CallbackException " + ea.Exception);

                //consumerChannel.Dispose();
                //consumerChannel = CreateConsumerChannel(func);
            };
            consumerChannels.Add(channel);
            return channel;
        }
        #region pool
        static readonly object SLock = new object();
        ConcurrentQueue<IModel> _pool = new ConcurrentQueue<IModel>();
        int _count;
        int _maxSize = 20;
        IModel RentChannel()
        {
            lock (SLock)
            {
                while (_count > _maxSize)
                {
                    Thread.SpinWait(1);
                }
                return RentBaseChannel();
            }
        }
        IModel RentBaseChannel()
        {
            if (_pool.TryDequeue(out var model))
            {
                Interlocked.Decrement(ref _count);

                return model;
            }
            if (!connection.IsOpen)
            {
                TryConnect();
            }
            try
            {
                model = connection.CreateModel();
            }
            catch (Exception e)
            {
                throw new Exception("connection.CreateModel时发生错误:" + e.Message);
            }

            return model;
        }
        bool ReturnChannel(IModel connection)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize)
            {
                _pool.Enqueue(connection);

                return true;
            }

            Interlocked.Decrement(ref _count);
            return false;
        }
        #endregion
        public void Dispose()
        {
            foreach(var c in consumerChannels)
            {
                c?.Dispose();
            }
            _maxSize = 0;

            while (_pool.TryDequeue(out var channel))
            {
                channel.Dispose();
            }
            connection?.Dispose();
        }
    }
}
