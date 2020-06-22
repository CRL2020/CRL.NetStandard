using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CRL.Core.Extension;
using System.Threading.Tasks;
using CRL.Core;
//不要引用MongoDB.Bson

namespace CRL.EventBus.Queue
{
    class MongoDb : AbsQueue
    {
        IMongoDatabase database;
        QueueConfig _queueConfig;
        List<Core.ThreadWork> threads = new List<ThreadWork>();
        public MongoDb(QueueConfig queueConfig)
        {
            _queueConfig = queueConfig;
            var ConnStr = queueConfig.ConnString;
            var lastIndex = ConnStr.LastIndexOf("/");
            var DatabaseName = ConnStr.Substring(lastIndex + 1);//like mongodb://localhost:27017/db1
            var client = new MongoClient(ConnStr);
            database = client.GetDatabase(DatabaseName);
        }
        public override void Dispose()
        {
            foreach(var t in threads)
            {
                t.Stop();
            }
        }

        public override void Publish<T>(string routingKey, params T[] msgs)
        {
            var name = $"CRL_QUEUE_{routingKey}";
            var coll = database.GetCollection<MongoData>(name);
            if (msgs.Length == 1)
            {
                coll.InsertOne(new MongoData() { Data = msgs.First().ToJson() });
                return;
            }
            var list = msgs.Select(b => new MongoData { Data = b.ToJson() });
            coll.InsertMany(list);
        }

        public override void Subscribe(EventDeclare eventDeclare)
        {
            var ed = eventDeclare;
            if(ed.IsCopy)
            {
                return;
            }
            var thread = new Core.ThreadWork() { Args = ed };
            thread.Start(ed.Name, SubscribeData, ed.ThreadSleepSecond);
            threads.Add(thread);
        }

        bool SubscribeData(object args)
        {
            var ed = args as EventDeclare;
            var name = $"CRL_QUEUE_{ed.Name}";
            var coll = database.GetCollection<MongoDataRead>(name);
            var list = coll.Find(b => true).SortBy(b => b.Time).Limit(ed.ListTake).ToList();
            if (list.Count == 0)
            {
                return true;
            }
            if (ed.IsArray)
            {
                var listInstance = DynamicMethodHelper.CreateCtorFuncFromCache(ed.EventDataType)();
                var innerType = ed.EventDataType.GenericTypeArguments[0];
                var method = ed.EventDataType.GetMethod("Add");
                foreach (var m in list)
                {
                    var item = m.Data.ToObject(innerType);
                    method.Invoke(listInstance, new object[] { item });
                }
                ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { listInstance });
                var ids = list.Select(b => b._id).ToArray();
                coll.DeleteMany(b => ids.Contains(b._id));
            }
            else
            {
                foreach(var m in list)
                {
                    var item = m.Data.ToObject(ed.EventDataType);
                    ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { item });
                    coll.DeleteOne(b => b._id == m._id);
                }
            }
            return true;
        }
        async Task<bool> SubscribeDataSync(object args)
        {
            var ed = args as EventDeclare;
            var name = $"CRL_QUEUE_{ed.Name}";
            var coll = database.GetCollection<MongoDataRead>(name);
            var list = await coll.Find(b => true).SortBy(b => b.Time).Limit(ed.ListTake).ToListAsync();
            if (list.Count == 0)
            {
                return true;
            }
            if (ed.IsArray)
            {
                var objInstance = DynamicMethodHelper.CreateCtorFuncFromCache(ed.EventDataType)();
                var innerType = ed.EventDataType.GenericTypeArguments[0];
                var method = ed.EventDataType.GetMethod("Add");
                foreach (var m in list)
                {
                    var item = m.Data.ToObject(innerType);
                    method.Invoke(objInstance, new object[] { item });
                }
                await (Task)ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { objInstance });
                var ids = list.Select(b => b._id).ToArray();
                await coll.DeleteManyAsync(b => ids.Contains(b._id));
            }
            else
            {
                foreach (var m in list)
                {
                    var item = m.Data.ToObject(ed.EventDataType);
                    await (Task)ed.MethodInvoke.Invoke(ed.CreateServiceInstance(), new object[] { item });
                    await coll.DeleteOneAsync(b => b._id == m._id);
                }
            }
            return true;
        }
        public override void SubscribeAsync(EventDeclare eventDeclare)
        {
            var ed = eventDeclare;
            if (ed.IsCopy)
            {
                return;
            }
            var thread = new Core.ThreadWork() { Args = ed };
            thread.Start(ed.Name, (args) =>
            {
                SubscribeDataSync(args).Wait();
                return true;
            }, ed.ThreadSleepSecond);
            threads.Add(thread);
        }
        public override long CleanQueue(string name)
        {
            name = $"CRL_QUEUE_{name}";
            var coll = database.GetCollection<MongoDataRead>(name);
            return coll.DeleteMany(b => true).DeletedCount;
        }
        public override long GetQueueLength(string name)
        {
            name = $"CRL_QUEUE_{name}";
            var coll = database.GetCollection<MongoDataRead>(name);
            return coll.Find(b => true).CountDocuments();
        }
    }
    class MongoData
    {
        public string Data
        {
            get; set;
        }
        public DateTime Time
        {
            get; set;
        } = DateTime.Now;
    }
    class MongoDataRead: MongoData
    {
        public MongoDB.Bson.ObjectId _id
        {
            get; set;
        } = MongoDB.Bson.ObjectId.GenerateNewId();
    }
}
