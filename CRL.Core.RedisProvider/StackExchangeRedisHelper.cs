
using CRL.Core;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
namespace CRL.Core.RedisProvider
{
    public class StackExchangeRedisHelper
    {
        private static readonly string Coonstr = RedisClient.GetRedisConn();
        private static object _locker = new Object();
        private static ConnectionMultiplexer _instance = null;
        static string host;
        /// <summary>
        /// 使用一个静态属性来返回已连接的实例，如下列中所示。这样，一旦 ConnectionMultiplexer 断开连接，便可以初始化新的连接实例。
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if(RedisClient.GetRedisConn==null)
                {
                    throw new Exception("请实现RedisClient.GetRedisConn");
                }
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            if(string.IsNullOrEmpty(Coonstr))
                            {
                                throw new Exception("请实现RedisClient.GetRedisConn");
                            }
                            if (Coonstr.Contains("@"))
                            {
                                var arry = Coonstr.Split(',');
                                var arry1 = arry[0].Split('@');
                                var pass = "";
                                var ip = "";
                                if (arry1.Length > 1)
                                {
                                    pass = arry1[0];
                                    ip = arry1[1];
                                }
                                else
                                {
                                    ip = arry1[0];
                                }
                                host = ip;
                                var options = ConfigurationOptions.Parse(ip);
                                options.Password = pass;
                                _instance = ConnectionMultiplexer.Connect(options);
                            }
                            else
                            {
                                _instance = ConnectionMultiplexer.Connect(Coonstr);
                            }
                            
                        }
                    }
                }
                //注册如下事件
                //_instance.ConnectionFailed += MuxerConnectionFailed;
                //_instance.ConnectionRestored += MuxerConnectionRestored;
                //_instance.ErrorMessage += MuxerErrorMessage;
                //_instance.ConfigurationChanged += MuxerConfigurationChanged;
                //_instance.HashSlotMoved += MuxerHashSlotMoved;
                //_instance.InternalError += MuxerInternalError;
                return _instance;
            }
        }
        int _db = -1;
        public StackExchangeRedisHelper(int db=-1)
        {
            _db = db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase()
        {
            return Instance.GetDatabase(_db);
        }
        public List<string> SearchKey(string key)
        {
            var db = _db < 0 ? 0 : _db;
            return Instance.GetServer(host).Keys(db, key + "*").Select(b => b.ToString()).ToList();
        }

        private string MergeKey(string key)
        {
            return key;
        }
        /// <summary>
        /// 根据key获取缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            key = MergeKey(key);
            return Deserialize<T>(GetDatabase().StringGet(key));
        }
        /// <summary>
        /// 根据key获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            key = MergeKey(key);
            return GetDatabase().StringGet(key);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set(string key, object value, TimeSpan expireIn)
        {
            key = MergeKey(key);
            return GetDatabase().StringSet(key, Serialize(value), expireIn);
        }

        /// <summary>
        /// 判断在缓存中是否存在该key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            key = MergeKey(key);
            return GetDatabase().KeyExists(key);  //可直接调用
        }
        #region hash

        /// <summary>
        /// 移除指定的记录
        /// </summary>
        /// <param name="key">需要移除的主键,一般为对象ID值</param>
        /// <returns></returns>
        public bool HRemove(string hashId, string key)
        {
            return GetDatabase().HashDelete(hashId,key);
        }

        /// <summary>
        /// 存储对象到缓存中
        /// </summary>
        /// <param name="key">需要写入的主键,一般为对象ID值,必须是文本/数字等对象</param>
        /// <param name="value">对象</param>
        public bool HSet(string hashId, string key, object value)
        {
            return GetDatabase().HashSet(hashId, key, Serialize(value));
        }

        /// <summary>
        /// 根据ID获取指定对象
        /// </summary>
        /// <param name="key">需要获取的主键,一般为对象ID值</param>
        /// <returns></returns>
        public T HGet<T>(string hashId, string key)
        {
            if (string.IsNullOrEmpty(hashId))
            {
                throw new Exception("hashId为空");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("key为空");
            }

            var result = GetDatabase().HashGet(hashId, key);
            if (!result.HasValue)
            {
                return default(T);
            }
            return Deserialize<T>(result);
        }
        public bool HContainsKey(string hashId, string key)
        {
            if (string.IsNullOrEmpty(hashId))
            {
                throw new Exception("hashId为空");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("key为空");
            }
            return GetDatabase().HashExists(hashId,key);
        }
        public List<T> HGetAll<T>(string hashId)
        {
            var result = GetDatabase().HashGetAll(hashId);
            var list = new List<T>();
            foreach(var item in result)
            {
                var obj= Deserialize<T>(item.Value);
                list.Add(obj);
            }
            return list;
        }
        public long GetHashCount(string hashId)
        {
            return GetDatabase().HashLength(hashId);
        }

        #endregion

        /// <summary>
        /// 移除指定key的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            key = MergeKey(key);
            return GetDatabase().KeyDelete(key);
        }

        /// <summary>
        /// 异步设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task SetAsync(string key, object value)
        {
            key = MergeKey(key);
            await GetDatabase().StringSetAsync(key, Serialize(value));
        }

        /// <summary>
        /// 根据key获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(string key)
        {
            key = MergeKey(key);
            object value = await GetDatabase().StringGetAsync(key);
            return value;
        }

        /// <summary>
        /// 实现递增
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long Increment(string key)
        {
            key = MergeKey(key);
            //三种命令模式
            //Sync,同步模式会直接阻塞调用者，但是显然不会阻塞其他线程。
            //Async,异步模式直接走的是Task模型。
            //Fire - and - Forget,就是发送命令，然后完全不关心最终什么时候完成命令操作。
            //即发即弃：通过配置 CommandFlags 来实现即发即弃功能，在该实例中该方法会立即返回，如果是string则返回null 如果是int则返回0.这个操作将会继续在后台运行，一个典型的用法页面计数器的实现：
            return GetDatabase().StringIncrement(key, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// 实现递减
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long Decrement(string key, string value)
        {
            key = MergeKey(key);
            return GetDatabase().HashDecrement(key, value, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        static string Serialize(object o)
        {
            return StringHelper.SerializerToJson(o);
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        static T Deserialize<T>(string data)
        {
            if (data == null)
            {
                return default(T);
            }
            return SerializeHelper.DeserializeFromJson<T>(data);
        }
       

      

        #region  消息发布
        /// <summary>
        /// 当作消息代理中间件使用
        /// 消息组建中,重要的概念便是生产者,消费者,消息中间件。
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public long Publish(string channel, string message)
        {
            ISubscriber sub = Instance.GetSubscriber();
            //return sub.Publish("messages", "hello");
            return sub.Publish(channel, message);
        }

        /// <summary>
        /// 在消费者端得到该消息并输出
        /// </summary>
        /// <param name="channelFrom"></param>
        /// <returns></returns>
        public void Subscribe<T>(string channelFrom, Action<T> callBack)
        {
            ISubscriber sub = Instance.GetSubscriber();
            sub.Subscribe(channelFrom, (channel, message) =>
            {
                try
                {
                    var obj = Deserialize<T>(message);
                    callBack(obj);
                }
                catch (Exception ero)
                {
                    EventLog.Log(ero.ToString(), "RedisOnSubscribe");
                }
            });
        }
        #endregion

        /// <summary>
        /// GetServer方法会接收一个EndPoint类或者一个唯一标识一台服务器的键值对
        /// 有时候需要为单个服务器指定特定的命令
        /// 使用IServer可以使用所有的shell命令，比如：
        /// DateTime lastSave = server.LastSave();
        /// ClientInfo[] clients = server.ClientList();
        /// 如果报错在连接字符串后加 ,allowAdmin=true;
        /// </summary>
        /// <returns></returns>
        public IServer GetServer(string host, int port)
        {
            IServer server = Instance.GetServer(host, port);
            return server;
        }

        /// <summary>
        /// 获取全部终结点
        /// </summary>
        /// <returns></returns>
        public EndPoint[] GetEndPoints()
        {
            EndPoint[] endpoints = Instance.GetEndPoints();
            return endpoints;
        }
        public long ListRightPush<T>(string key, T value)
        {
            return ListRightPush(key, Serialize(value));
            //return GetDatabase().ListRightPush(key, Serialize(value));
        }
        public long ListRightPush(string key, string value)
        {
            return GetDatabase().ListRightPush(key, value);
        }
        public long ListRemove(string key, object value)
        {
            return GetDatabase().ListRemove(key, Serialize(value));
        }
        public List<T> ListRange<T>(string key, long start, long end)
        {
            var list = new List<T>();
            var result = GetDatabase().ListRange(key, start, end);
            foreach (var json in result)
            {
                var obj = Deserialize<T>(json.ToString());
                list.Add(obj);
            }
            return list;
        }
        public void ListTrim(string key, long start, long end)
        {
            GetDatabase().ListTrim(key, start, end);
        }
        public long ListLength(string key)
        {
            return GetDatabase().ListLength(key);
        }
    }
}
