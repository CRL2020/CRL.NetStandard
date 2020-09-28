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
namespace CRL.Sharding
{
    /// <summary>
    /// 分表数据管理实现
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BaseProvider<TModel> : ProviderOrigin<TModel>
        where TModel : class, new()
    {
        /// <summary>
        /// 设置定位
        /// </summary>
        public BaseProvider<TModel> SetLocation(TModel args)
        {
            var table = TypeCache.GetTable(typeof(TModel));
            var func = DBConfigRegister.GetLocation<TModel>();
            if (func == null || args == null)
            {
                //throw new CRLException($"指定类型{typeof(TModel).Name} 未注册定位方法");
                //没有设置则按默认库
                return this;
            }
            var location = func(table, args);
            location.CheckNull("location");
            dbLocation.ShardingLocation = location;
            dbLocation.ShardingLocation.TableName = table.TableName;
            DBExtend = null;
            return this;
        }
        internal override DbContextInner GetDbContext()
        {
            dbLocation.ManageName = ManageName;
            var helper = DBConfigRegister.GetDBHelper(dbLocation);
            var dbContext = new DbContextInner(helper, dbLocation);

            dbContext.UseSharding = true;
            return dbContext;
        }
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="p"></param>
        /// <param name="asyn"></param>
        public override void Add(TModel p, bool asyn = false)
        {
            SetLocation(p);
            base.Add(p, asyn);
        }
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="keepIdentity"></param>
        public override void BatchInsert<T>(List<T> list, bool keepIdentity = false)
        {
            if (list.Count == 0)
            {
                return;
            }
            var first = list.First() as TModel;
            if (first == null)
            {
                throw new Exception("数据不为" + typeof(TModel));
            }
            SetLocation(first);
            base.BatchInsert(list, keepIdentity);
        }
    }
}
