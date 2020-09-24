/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
//using System.Transactions;
using CRL.LambdaQuery;
//using System.Messaging;
using CRL.Core;
//using MongoDB.Driver.Linq;
using System.Collections;

namespace CRL
{
    /// <summary>
    /// 基本业务方法封装
    /// </summary>
    /// <typeparam name="T">源对象</typeparam>
    public abstract class ProviderOrigin<T>: IProvider
        where T : IModel, new()
    {
        /// <summary>
        /// 是否检查重复插入,默认为true
        /// 判断重复为相同的属性值,AddTime除外,3秒内唯一
        /// </summary>
        protected internal virtual bool CheckRepeatedInsert
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 重写以获取指定的管理名称
        /// </summary>
        public virtual string ManageName
        {
            get
            {
                return "";
            }
        }
        /// <summary>
        /// 数据定位时获取当前类型
        /// </summary>
        public Type ModelType
        {
            get
            {
                return typeof(T);
            }
        }
        /// <summary>
        /// 创建当前调用上下文唯一实例
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static T2 CreateInstance<T2>() where T2 : class, new()
        {
            //return new T();
            string contextName = "Instance." + typeof(T2);
            var instance = CallContext.GetData<T2>(contextName);
            if (instance == null)
            {
                instance = new T2();
                CallContext.SetData(contextName, instance);
            }
            return instance;
        }
        /// <summary>
        /// 基本业务方法封装
        /// </summary>
        public ProviderOrigin()
        {
            dbLocation = new DBLocation() { ManageType = GetType() };
        }

        #region redis
        RedisProvider.RedisClient _RedisClient;
        protected RedisProvider.RedisClient RedisClient
        {
            get
            {
                if (_RedisClient == null)
                {
                    _RedisClient = new RedisProvider.RedisClient(RedisDbIndex);
                }
                return _RedisClient;
            }
        }
        #region redis 仅HASH 如果重载了，则在操作实体时写入REDIS
        protected virtual int RedisDbIndex
        {
            get
            {
                return -1;
            }
        }
        /// <summary>
        /// 重写获到HashId
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual string GetRedisHashId(T obj)
        {
            return "";
        }
        /// <summary>
        /// 重写获取Key
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual string GetRedisHashKey(T obj)
        {
            return "";
        }
        protected string GetHashId(T obj)
        {
            var str = GetRedisHashId(obj);
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            var hashId = string.Format("{0}_{1}", obj.GetType().Name, GetRedisHashId(obj));
            return hashId;
        }
        protected void SetToRedis(T obj, TimeSpan? timeOut = null)
        {
            var hashId = GetHashId(obj);
            if (string.IsNullOrEmpty(hashId))
            {
                return;
            }
            var key = GetRedisHashKey(obj);
            RedisClient.HSet(hashId, key, obj);
            if (timeOut != null)
            {
                RedisClient.KSetEntryIn(hashId, timeOut.Value);
            }
        }
        protected bool DeleteFromRedis(T obj, bool all = false)
        {
            var hashId = GetHashId(obj);
            if (string.IsNullOrEmpty(hashId))
            {
                return false;
            }
            if (all)
            {
                return RedisClient.Remove(hashId);
            }
            var key = GetRedisHashKey(obj);
            return RedisClient.HRemove(hashId, key);
        }
        protected T GetFromRedis(T obj)
        {
            var hashId = GetHashId(obj);
            if (string.IsNullOrEmpty(hashId))
            {
                return default(T);
            }
            var key = GetRedisHashKey(obj);
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            var data = RedisClient.HGet<T>(hashId, key);
            return data;
        }
        protected List<T> GetAllFromRedis(T obj)
        {
            var hashId = GetHashId(obj);
            if (string.IsNullOrEmpty(hashId))
            {
                return new List<T>();
            }
            //var key = GetRedisHashKey(obj);
            var data = RedisClient.HGetAll<T>(hashId) ?? new List<T>();
            return data;
        }
        protected bool ExistsFromRedis(T obj)
        {
            var hashId = GetHashId(obj);
            if (string.IsNullOrEmpty(hashId))
            {
                return false;
            }
            var key = GetRedisHashKey(obj);
            return RedisClient.HContainsKey(hashId, key);
        }
        protected long GetHashCount(T obj)
        {
            var hashId = GetHashId(obj);
            if (string.IsNullOrEmpty(hashId))
            {
                return 0;
            }
            return RedisClient.GetHashCount(hashId);
        }

        /// <summary>
        /// 更新所有数据到REDIS 
        /// 仅测试
        /// </summary>
        public int UpdateAllDataToRedis(Expression<Func<T, bool>> expression = null)
        {
            var query = GetLambdaQuery();
            if (expression == null)
            {
                query.Where(expression);
            }
            var list = query.ToList();
            var type = typeof(T).Name;
            Console.WriteLine($"{type} 总数:{list.Count}条");
            var allHash = list.Select(b => GetHashId(b)).GroupBy(b => b).Select(b => b.Key).ToList();
            foreach (var hashId in allHash)
            {
                RedisClient.Remove(hashId);
            }
            foreach (var obj in list)
            {
                var hashId = GetHashId(obj);
                if (string.IsNullOrEmpty(hashId))
                {
                    return 0;
                }
                var key = GetRedisHashKey(obj);
                RedisClient.HSet(hashId, key, obj);
                Console.WriteLine($"Update {type}:{hashId}");
            }
            return list.Count;
        }
        #endregion


        /// <summary>
        /// 获取24时失效的缓存数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetFromRedisCache(string id)
        {
            var timeOutSecond = 60 * 60 * 24;
            return RedisClient.GetCustomCache(id, timeOutSecond, () =>
            {
                var data = QueryItem(id);
                return data;
            });
        }

        #endregion
        /// <summary>
        /// 数据访问上下文
        /// </summary>
        /// <returns></returns>
        internal abstract DbContextInner GetDbContext();

        /// <summary>
        /// 当前数据访定位
        /// </summary>
        internal DBLocation dbLocation;

        /// <summary>
        /// 锁对象
        /// </summary>
        protected static object lockObj = new object();
        /// <summary>
        /// 对象被更新时,是否通知缓存服务器
        /// 在业务类中进行控制
        /// </summary>
        protected virtual bool OnUpdateNotifyCacheServer
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 创建当前类型查询表达式实列
        /// </summary>
        /// <returns></returns>
        public virtual ILambdaQuery<T> GetLambdaQuery()
        {
            var db = DBExtend as AbsDBExtend;
            //var dbContext2 = GetDbContext(true);//避开事务控制,使用新的连接
            var query = LambdaQueryFactory.CreateLambdaQuery<T>(db.dbContext);
            return query;
        }
        /// <summary>
        /// 指定查询条件创建表达式实例
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ILambdaQuery<T> GetLambdaQuery(Expression<Func<T, bool>> expression)
        {
            var query = GetLambdaQuery();
            query.Where(expression);
            return query;
        }

        #region 数据访问对象
        IAbsDBExtend _dBExtend;
        /// <summary>
        /// 数据访部对象
        /// 当前实例内只会创建一个,查询除外
        /// </summary>
        protected IAbsDBExtend DBExtend
        {
            get
            {
                 var _useCRLContext = CallContext.GetData<bool>(Base.UseCRLContextFlagName);
                 if (_useCRLContext)//对于数据库事务,只创建一个上下文
                 {
                     return GetDBExtend();
                 }
                if (_dBExtend == null)
                {
                    _dBExtend = GetDBExtend();
                }
                return _dBExtend;
            }
            set
            {
                _dBExtend = value;
            }
        }
        /// <summary>
        /// 数据访问对象[基本方法]
        /// 按指定的类型
        /// </summary>
        /// <returns></returns>
        protected AbsDBExtend GetDBExtend()
        {
            AbsDBExtend db = null;
            string contextName = GetType().Name;//同一线程调用只创建一次
            var _useCRLContext = CallContext.GetData<bool>(Base.UseCRLContextFlagName);
            if (_useCRLContext)//对于数据库事务,只创建一个上下文
            {            
                //todo 由于内置缓存问题,参数不能一直变化,不然生成重复缓存和重复存储过程
                contextName = Base.CRLContextName;
                db = CallContext.GetData<AbsDBExtend>(contextName);
                if (db != null)
                {
                    return db;
                }
            }

            var dbContext2 = GetDbContext();
            if (_useCRLContext)//使用CRLContext,需由CRLContext来关闭数据连接
            {
                dbContext2.DBHelper.AutoCloseConn = false;
            }
            db = DBExtendFactory.CreateDBExtend(dbContext2);
            if (dbLocation.ShardingLocation == null)
            {
                db.OnUpdateNotifyCacheServer = OnUpdateNotifyCacheServer;
            }
            if (_useCRLContext)
            {
                CallContext.SetData(contextName, db);
            }
            //占用内存..
            var allList = Base.GetCallDBContext();
            allList.Add(contextName);
            return db;
        }
        #endregion

        #region 创建结构
        /// <summary>
        /// 创建TABLE[基本方法]
        /// </summary>
        /// <returns></returns>
        public virtual string CreateTable()
        {
            var db = DBExtend;
            var str = ModelCheck.CreateTable(typeof(T),db as AbsDBExtend);
            return str;
        }
        /// <summary>
        /// 创建表索引
        /// </summary>
        public void CreateTableIndex()
        {
            DBExtend.CreateTableIndex<T>();
        }
        #endregion

        /// <summary>
        /// 写日志[基本方法]
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        void Log(string message, string type = "CRL")
        {
            EventLog.Log(message, type, false);
        }

        #region 添加
        
        /// <summary>
        /// 添加一条记录[基本方法]
        /// 异步时,会定时执行批量插入,依赖MSMQ服务
        /// </summary>
        /// <param name="p"></param>
        /// <param name="asyn">异步插入</param>
        public virtual void Add(T p, bool asyn = false)
        {
            var db = DBExtend as AbsDBExtend;
            
            db.InsertFromObj(p);
            //redis
            SetToRedis(p);
        }
        
        /// <summary>
        /// 批量插入[基本方法]
        /// 可为任意类型
        /// </summary>
        /// <param name="list"></param>
        /// <param name="keepIdentity"></param>
        public virtual void Add<T2>(List<T2> list, bool keepIdentity = false) where T2 : IModel, new()
        {
            BatchInsert(list, keepIdentity);
        }
        /// <summary>
        /// 批量插入[基本方法]
        /// 可为任意类型
        /// </summary>
        /// <param name="list"></param>
        /// <param name="keepIdentity">是否保持自增主键</param>
        public virtual void BatchInsert<T2>(List<T2> list, bool keepIdentity = false) where T2 : IModel, new()
        {
            var db = DBExtend;
            if (list == null || list.Count == 0)
            {
                return;
            }
            if (list.Count == 1)
            {
                db.InsertFromObj(list.First());
                return;
            }
            db.BatchInsert(list, keepIdentity);
            //redis
            var obj = list.First();
            if (obj is T)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    foreach (var item in list)
                    {
                        SetToRedis(item as T);
                    }
                });
            }
        }
        #endregion

        #region 查询一项
        /// <summary>
        /// 按排序查询一条
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public T QueryItem<TResult>(Expression<Func<T, bool>> expression, Expression<Func<T, TResult>> sortExpression, bool desc = true)
        {
            var query = GetLambdaQuery();
            query.Top(1);
            query.Where(expression).OrderBy(sortExpression, desc);
            var db = DBExtend;
            return db.QueryList(query as LambdaQuery<T>).FirstOrDefault();
        }
        /// <summary>
        /// 按主键查询一项[基本方法]
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T QueryItem(object id)
        {
            var db = DBExtend;
            return db.QueryItem<T>(id);
        }
        /// <summary>
        /// 按条件取单个记录[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="idDest">是否按主键倒序</param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public T QueryItem(Expression<Func<T, bool>> expression, bool idDest = true, bool compileSp = false)
        {
            //AbsDBExtend db = DBExtend;
            //return db.QueryItem(expression, idDest, compileSp);
            var query = GetLambdaQuery();
            query.Top(1);
            query.CompileToSp(compileSp);
            query.Where(expression).OrderByPrimaryKey(idDest);
            var db = DBExtend;
            return db.QueryList(query as LambdaQuery<T>).FirstOrDefault();
        }
        #endregion

        #region 查询多项
        /// <summary>
        /// 返回全部结果[基本方法]
        /// </summary>
        /// <returns></returns>
        public List<T> QueryList()
        {
            //AbsDBExtend db = GetDBExtend();
            //return db.QueryList<TModel>();
            var query = GetLambdaQuery();
            var db = DBExtend;
            return db.QueryList(query as LambdaQuery<T>);
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<T> QueryList(Expression<Func<T, bool>> expression, bool compileSp = false)
        {
            //AbsDBExtend db = GetDBExtend();
            //return db.QueryList<TModel>(expression, compileSp);
            var query = GetLambdaQuery();
            query.CompileToSp(compileSp);
            query.Where(expression);
            var db = DBExtend;
            return db.QueryList(query as LambdaQuery<T>);
        }

        #endregion

        #region 删除
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(object id)
        {
            var db = DBExtend;
            var n = db.Delete<T>(id);
            return n;
        }
        /// <summary>
        /// 按对象主键删除
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int Delete(T obj)
        {
            var db = DBExtend;
            var v = TypeCache.GetpPrimaryKeyValue(obj);
            var n = db.Delete<T>(v);
            DeleteFromRedis(obj);
            return n;
        }
        /// <summary>
        /// 按条件删除[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<T, bool>> expression)
        {
            //var db = DBExtend;
            var query = GetLambdaQuery();
            query.Where(expression);
            var n = Delete(query);
            return n;
        }
        /// <summary>
        /// 按完整查询删除
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Delete(ILambdaQuery<T> query)
        {
            var db = DBExtend;
            int n = db.Delete(query as LambdaQuery<T>);
            return n;
        }
        /// <summary>
        /// 关联删除
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete<TJoin>(Expression<Func<T, TJoin, bool>> expression)
            where TJoin : IModel, new()
        {
            //var db = DBExtend;
            var query = GetLambdaQuery();
            query.Join(expression);
            return Delete(query);
            //return DBExtend.Delete(expression);
        }
        #endregion

        #region 更新
        /// <summary>
        /// 按对象差异更新,对象需由查询创建[基本方法]
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Update<T2>(T2 item) where T2 : IModel, new()
        {
            var db = DBExtend;
            var n = db.Update(item);
            if (item is T)
            {
                SetToRedis(item as T);
            }
            return n;
        }
        /// <summary>
        /// 按主键一次更新多条数据
        /// 以多条SQL执行更新
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public int Update<T2>(List<T2> items) where T2 : IModel, new()
        {
            if (items.Count == 0)
            {
                return 0;
            }
            if (items.Count > 2000)
            {
                throw new Exception("更新数据行数不能超过2000");
            }
            var db = DBExtend;
            return db.Update(items);
        }

        /// <summary>
        /// 指定条件并按对象差异更新[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(Expression<Func<T, bool>> expression, T model)
        {
            var query = GetLambdaQuery();
            query.Where(expression);
            int n = Update(query as LambdaQuery<T>, model.GetUpdateField());
            return n;
        }

        /// <summary>
        /// 按匿名对象更新
        /// </summary>
        /// <typeparam name="TOjbect"></typeparam>
        /// <param name="expression"></param>
        /// <param name="updateValue"></param>
        /// <returns></returns>
        public int Update<TOjbect>(Expression<Func<T, bool>> expression, TOjbect updateValue) where TOjbect : class
        {
            if(updateValue is IDictionary)
            {
                return Update(expression, updateValue as IDictionary);
            }
            var properties = updateValue.GetType().GetProperties();
            var c = new ParameCollection();
            foreach (var p in properties)
            {
                c.Add(p.Name, p.GetValue(updateValue));
            }
            var query = GetLambdaQuery();
            query.Where(expression);
            int n = Update(query as LambdaQuery<T>, c);
            return n;
        }

        /// <summary>
        /// 按匿名表达式更新
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="newExpress"></param>
        /// <returns></returns>
        public int Update<TResult>(Expression<Func<T, bool>> expression, Expression<Func<T, TResult>> newExpress)
        {
          
            var c = new ParameCollection();
            if(newExpress.Body is NewExpression)
            {
                var newExp = newExpress.Body as NewExpression;
                for (int i = 0; i < newExp.Members.Count; i++)
                {
                    var m = newExp.Members[i];
                    var v = newExp.Arguments[i];
                    bool cos;
                    var value = ConstantValueVisitor.GetMemberExpressionValue(v, out cos);
                    c.Add(m.Name, value);
                }
            }
            else if(newExpress.Body is MemberInitExpression)
            {
                var memberInitExp = (newExpress.Body as MemberInitExpression);

                foreach (MemberAssignment m in memberInitExp.Bindings)
                {
                    bool cos;
                    var value = ConstantValueVisitor.GetMemberExpressionValue(m.Expression, out cos);
                    c.Add(m.Member.Name, value);
                }
            }
  
            var query = GetLambdaQuery();
            query.Where(expression);
            int n = Update(query as LambdaQuery<T>, c);
            return n;
        }

        /// <summary>
        /// 指定条件和参数进行更新[基本方法]
        /// </summary>
        /// <param name="expression">条件</param>
        /// <param name="setValue">值</param>
        /// <returns></returns>
        public int Update(Expression<Func<T, bool>> expression, IDictionary setValue)
        {
            var query = GetLambdaQuery();
            query.Where(expression);
            int n = Update(query as LambdaQuery<T>, setValue);
            return n;
            //var db = DBExtend;
            //int n = db.Update(expression, setValue);
            //return n;
        }
        /// <summary>
        /// 按完整查询条件更新
        /// </summary>
        /// <param name="query"></param>
        /// <param name="updateValue">要按字段值更新,需加前辍$ 如 c["UserId"] = "$UserId"</param>
        /// <returns></returns>
        public int Update(ILambdaQuery<T> query, IDictionary updateValue)
        {
            var db = DBExtend;
            var iDic = updateValue as Dictionary<string, object>;
            if (iDic == null)
            {
                throw new Exception("无法转换为Dictionary<string, object>");
            }
            var dic = new ParameCollection(iDic);
            return db.Update(query as LambdaQuery<T>, dic);
        }
        /// <summary>
        /// 关联更新
        /// </summary>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="expression"></param>
        /// <param name="updateValue">要按字段值更新,需加前辍$ 如 c["UserId"] = "$UserId"</param>
        /// <returns></returns>
        public int Update<TJoin>(Expression<Func<T, TJoin, bool>> expression, IDictionary updateValue)
            where TJoin : IModel, new()
        {
            //return DBExtend.Update(expression, updateValue);
            var query = GetLambdaQuery();
            query.Join(expression);
            return Update(query as LambdaQuery<T>, updateValue);
        }
        #endregion


        #region 导入导出
        /// <summary>
        /// 导出为json[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string ExportToJson(Expression<Func<T, bool>> expression)
        {
            var list = QueryList(expression);
            var json = SerializeHelper.SerializerToJson(list);
            return json;
        }
        /// <summary>
        /// 从json导入[基本方法]
        /// </summary>
        /// <param name="json"></param>
        /// <param name="delExpression">要删除的数据</param>
        /// <param name="keepIdentity">是否保留自增主键</param>
        /// <returns></returns>
        public int ImportFromJson(string json, Expression<Func<T, bool>> delExpression, bool keepIdentity = false)
        {
            var obj = SerializeHelper.DeserializeFromJson<List<T>>(json);
            Delete(delExpression);
            BatchInsert(obj, keepIdentity);
            return obj.Count;
        }
        #endregion

        #region 统计
        /// <summary>
        /// 统计[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public int Count(Expression<Func<T, bool>> expression, bool compileSp = false)
        {
            AbsDBExtend db = GetDBExtend();
            return db.Count(expression, compileSp);
        }
        /// <summary>
        /// sum 按表达式指定字段[基本方法]
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType>(Expression<Func<T, bool>> expression, Expression<Func<T, TType>> field, bool compileSp = false)
        {
            AbsDBExtend db = GetDBExtend();
            return db.Sum(expression, field, compileSp);
        }
        /// <summary>
        /// 取最大值[基本方法]
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Max<TType>(Expression<Func<T, bool>> expression, Expression<Func<T, TType>> field, bool compileSp = false)
        {
            AbsDBExtend db = GetDBExtend();
            return db.Max(expression, field, compileSp);
        }
        /// <summary>
        /// 取最小值[基本方法]
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Min<TType>(Expression<Func<T, bool>> expression, Expression<Func<T, TType>> field, bool compileSp = false)
        {
            AbsDBExtend db = GetDBExtend();
            return db.Min(expression, field, compileSp);
        }
        #endregion

        /// <summary>
        /// 将方法调用打包,使只用一个数据连接
        /// 同CRLDbConnectionScope
        /// </summary>
        /// <param name="action"></param>
        public void PackageMethod(Action action)
        {
            using (var context = new CRLDbConnectionScope())
            {
                try
                {
                    action();
                }
                catch(Exception ero)
                {
                    context.Dispose();
                    throw ero;
                }
            }
        }
        #region 包装为事务执行
        /// <summary>
        /// 使用DbTransaction封装事务,不能跨库
        /// 请将数据访问对象写在方法体内
        /// 可嵌套调用
        /// </summary>
        /// <param name="method"></param>
        /// <param name="error"></param>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public bool PackageTrans(TransMethod method, out string error,  System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            error = "";
            var _useCRLContext = CallContext.GetData<bool>(Base.UseCRLContextFlagName);//事务已开启,内部事务不用处理
            using (var context = new CRLDbConnectionScope())
            {
                var db = GetDBExtend();
                if (!_useCRLContext)
                {
                    db.BeginTran(isolationLevel);
                }
                bool result;
                try
                {
                    result = method(out error);
                    if (!_useCRLContext)
                    {
                        if (!result)
                        {
                            db.RollbackTran();
                            CallContext.SetData(Base.UseCRLContextFlagName, false);
                            return false;
                        }
                        db.CommitTran();
                    }
                }
                catch (Exception ero)
                {
                    error = "提交事务时发生错误:" + ero.Message;
                    if (!_useCRLContext)
                    {
                        db.RollbackTran();
                        CallContext.SetData(Base.UseCRLContextFlagName, false);
                    }
                    return false;
                }
                if (!_useCRLContext)
                {
                    CallContext.SetData(Base.UseCRLContextFlagName, false);
                }
                return result;
            }
        }
        ///// <summary>
        ///// 使用TransactionScope封装事务[基本方法]
        ///// </summary>
        ///// <param name="method"></param>
        ///// <param name="error"></param>
        ///// <returns></returns>
        //public bool PackageTrans(TransMethod method, out string error)
        //{
        //    error = "";
        //    using (var trans = new TransactionScope())
        //    {
        //        CallContext.SetData(Base.UseTransactionScopeName, true);
        //        try
        //        {
        //            var a = method(out error);
        //            if (!a)
        //            {
        //                CallContext.SetData(Base.UseTransactionScopeName, false);
        //                return false;
        //            }
        //            trans.Complete();
        //        }
        //        catch (Exception ero)
        //        {
        //            CallContext.SetData(Base.UseTransactionScopeName, false);
        //            error = "提交事务时发生错误:" + ero.Message;
        //            EventLog.Log("提交事务时发生错误:" + ero, "Trans");
        //            return false;
        //        }
        //    }
        //    CallContext.SetData(Base.UseTransactionScopeName, false);
        //    return true;
        //}
        #endregion
    }

    internal interface IProvider
    {
        /// <summary>
        /// 绑定对象类型
        /// </summary>
        Type ModelType { get; }
    }
}
