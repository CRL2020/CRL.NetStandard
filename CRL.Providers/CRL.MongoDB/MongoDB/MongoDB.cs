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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using CRL.LambdaQuery;
using MongoDB.Driver.Linq;
using System.Collections.Concurrent;

namespace CRL.Mongo.MongoDBEx
{
    public sealed partial class MongoDBExt : AbsDBExtend
    {
        public MongoDBExt(DbContext _dbContext)
            : base(_dbContext)
        {
        }
        protected override LambdaQuery<TModel> CreateLambdaQuery<TModel>()
        {
            return new MongoDBLambdaQuery<TModel>(dbContext);
        }

        IMongoDatabase _mongoDatabase=null;
        
        IMongoDatabase _MongoDB
        {
            get {
                if (_mongoDatabase == null)
                {
                    var db = GetDBHelper();
                    var connectionString = db.ConnectionString;
                    var _client = new MongoClient(connectionString);
                    _mongoDatabase = _client.GetDatabase(db.DatabaseName);
                }
                return _mongoDatabase; }
            set { _mongoDatabase = value; }
        }

        public override void CreateTableIndex<TModel>()
        {
            var type = typeof(TModel);
            var columns = ModelCheck.GetColumns(type, this);
            foreach (Attribute.FieldInnerAttribute item in columns)
            {
                if (item.FieldIndexType != Attribute.FieldIndexType.无)
                {
                    var indexKeys = Builders<TModel>.IndexKeys.Ascending(item.MemberName);
                    GetCollection<TModel>().Indexes.CreateOne(indexKeys);
                }
            }
        }
        ///// <summary>
        ///// 返回MongoQueryable
        ///// </summary>
        ///// <typeparam name="TModel"></typeparam>
        ///// <returns></returns>
        //public override IMongoQueryable<TModel> GetMongoQueryable<TModel>()
        //{
        //    var collection = GetCollection<TModel>();
        //    return collection.AsQueryable();

        //}
        /// <summary>
        /// 获取集合名,统一按定位判断
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        protected IMongoCollection<TModel> GetCollection<TModel>()
        {
            string tableName;
            if (dbContext.DBLocation.ShardingLocation != null)
            {
                tableName = dbContext.DBLocation.ShardingLocation.TablePartName;
            }
            else
            {
                tableName = TypeCache.GetTableName(typeof(TModel), dbContext);
            }
            return _MongoDB.GetCollection<TModel>(tableName);
        }
        public override TType GetFunction<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> selectField, FunctionType functionType, bool compileSp = false)
        {
            var query = new MongoDBLambdaQuery<TModel>(dbContext);
            query.Select(selectField.Body);
            query.Where(expression);
            var collection = _MongoDB.GetCollection<TModel>(query.QueryTableName);
            object result = null;
            //https://blog.csdn.net/shiyaru1314/article/details/52370478
            //https://www.jb51.net/article/113820.htm
            //https://blog.csdn.net/u013476435/article/details/81560089
            switch (functionType)
            {
                case FunctionType.COUNT:
                    result = collection.Count(query.__MongoDBFilter);
                    break;
                default:
                    throw new NotSupportedException("MongoDB不支持的函数:" + functionType);
            }
            return ObjectConvert.ConvertObject<TType>(result);
        }
    }
}
