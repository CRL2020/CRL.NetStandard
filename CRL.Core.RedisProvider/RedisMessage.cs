using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRL.Core.RedisProvider
{
    /// <summary>
    /// 简单REDIS消息订阅
    /// 存储为list数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RedisMessage<T>
    {
        public virtual int Take
        {
            get
            {
                return 500;
            }
        }
        public virtual int SleepSecond
        {
            get
            {
                return 5;
            }
        }
        public static bool created = false;
        protected RedisClient client = new RedisClient();
        public Type GetMessageType()
        {
            return typeof(T);
        }
        public RedisMessage(bool start = true)
        {
            if (start)
            {
                Start();
            }
        }
        bool wait = false;
        object lockObj = new object();
        public void Start()
        {
            var hashId = GetHashId();
            if (!created)
            {
                created = true;
                Console.WriteLine("RedisMessage" + typeof(T) + "启动订阅");
                //client.Subscribe<T>(OnSubscribe);
                new ThreadWork().Start(hashId, () =>
                {
                    //var len = GetLength();
                    wait = false;
                    var all = client.ListRange<T>(hashId, 0, Take - 1);
                    if (all.Count == 0)
                    {
                        return true;
                    }
                    var a = OnSubscribe(all);
                    lock (lockObj)
                    {
                        wait = true;
                        if (a)
                        {
                            client.ListTrim(hashId, all.Count, -1);
                            //var len2 = GetLength();
                            ///CRL.Core.EventLog.Log($"删除前{len} 获取:{all.Count} 删除后:{len2} 差异{len - all.Count != len2}", GetType().Name);
                        }
                        wait = false;
                    }
                    if (rePublish.Count > 0)
                    {
                        PublishList(rePublish);
                        rePublish.Clear();
                    }
                    return true;
                }, SleepSecond);
                created = true;
            }
        }
        string GetHashId()
        {
            return string.Format("RedisMessage_{0}",typeof(T).Name);
        }
        public virtual void Publish(T message)
        {
            if (message == null)
            {
                return;
            }
            var hashId = GetHashId();
            client.ListRightPush(hashId, message);
        }
        public void DeleteAll()
        {
            var hashId = GetHashId();
            client.Remove(hashId);
        }
        public long GetLength()
        {
            var hashId = GetHashId();
            return client.ListLength(hashId);
        }
        public void PublishList(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                return;
            }
            var list = SerializeHelper.DeserializeFromJson<List<T>>(jsonData);
            PublishList(list);
        }
        public virtual void PublishList(List<T> messages)
        {
           
            if (messages == null)
            {
                return;
            }
            if (messages.Count == 0)
            {
                return;
            }
            var hashId = GetHashId();
            foreach (var m in messages)
            {
                while (wait)
                {
                    Thread.Sleep(10);
                }
                client.ListRightPush(hashId, m);
            }

        }
        protected abstract bool OnSubscribe(List<T> message);
        List<T> rePublish = new List<T>();
        /// <summary>
        /// 重新加入队列
        /// </summary>
        /// <param name="message"></param>
        protected void RePublish(T message)
        {
            rePublish.Add(message);
        }
    }
}
