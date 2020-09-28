/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.DBAccess;
using CRL.DBAdapter;
using CRL.LambdaQuery;
using CRL.Sharding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRL
{
    public interface IDbConfigRegister
    {
        /// <summary>
        /// 注册数据访问实现
        /// 按优先顺序添加,不成立则返回null
        /// </summary>
        /// <param name="func"></param>
        void RegisterDBAccessBuild(Func<DBLocation, DBAccessBuild> func);
        /// <summary>
        /// 注册定位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        void RegisterLocation<T>(Func<Attribute.TableInnerAttribute, T, Location> func);

    }

    public class DBConfigRegister : IDbConfigRegister
    {
        internal Dictionary<DBType, Func<DBAccessBuild, DBHelper>> DBHelperRegister = new Dictionary<DBType, Func<DBAccessBuild, DBHelper>>();
        internal Dictionary<DBType, Func<DbContextInner, DBAdapter.DBAdapterBase>> DBAdapterBaseRegister = new Dictionary<DBType, Func<DbContextInner, DBAdapter.DBAdapterBase>>();
        internal Dictionary<DBType, Func<DbContextInner, AbsDBExtend>> AbsDBExtendRegister = new Dictionary<DBType, Func<DbContextInner, AbsDBExtend>>();

        internal List<Func<DBLocation, DBAccessBuild>> DbAccessCreaterRegister = new List<Func<DBLocation, DBAccessBuild>>();

        internal Dictionary<DBType, Type> LambdaQueryTypeRegister = new Dictionary<DBType, Type>();

        internal Dictionary<Type, object> LocationRegister = new Dictionary<Type, object>();
        static DBConfigRegister instance;

        DBConfigRegister()
        {
        }
        static DBConfigRegister()
        {
            instance = new DBConfigRegister();

            #region 注册默认数据库类型
            var configBuilder = instance;
            configBuilder.RegisterDBType(DBType.MSSQL, (dBAccessBuild) =>
            {
                return new SqlHelper(dBAccessBuild);
            }, (context) =>
            {
                return new DBAdapter.MSSQLDBAdapter(context);
            });
            configBuilder.RegisterDBType(DBType.MSSQL2000, (dBAccessBuild) =>
            {
                return new Sql2000Helper(dBAccessBuild);
            }, (context) =>
            {
                return new DBAdapter.MSSQL2000DBAdapter(context);
            });
            configBuilder.RegisterDBExtend<CRL.DBExtend.RelationDB.DBExtend>(DBType.MSSQL, (context) =>
            {
                return new DBExtend.RelationDB.DBExtend(context);
            });
            configBuilder.RegisterLambdaQueryType(DBType.MSSQL, typeof(RelationLambdaQuery<>));
            #endregion

        }
        public static IDbConfigRegister GetInstance()
        {
            return instance;
        }

        public DBConfigRegister RegisterDBType(DBType dBType, Func<DBAccessBuild, DBHelper> funcDb, Func<DbContextInner, DBAdapter.DBAdapterBase> funcDBAdapter)
        {
            if (!DBHelperRegister.ContainsKey(dBType))
            {
                DBHelperRegister.Add(dBType, funcDb);
                DBAdapterBaseRegister.Add(dBType, funcDBAdapter);
            }
            return this;
        }
        public DBConfigRegister RegisterDBExtend<T1>(DBType dBType, Func<DbContextInner, AbsDBExtend> func) where T1 : AbsDBExtend
        {
            if (!AbsDBExtendRegister.ContainsKey(dBType))
            {
                AbsDBExtendRegister.Add(dBType, func);
            }
            return this;
        }

        public DBConfigRegister RegisterLambdaQueryType(DBType dBType, Type type)
        {
            if (!LambdaQueryTypeRegister.ContainsKey(dBType))
            {
                LambdaQueryTypeRegister.Add(dBType, type);
            }
            return this;
        }
        /// <summary>
        /// 注册数据访问实现
        /// 按优先顺序添加,不成立则返回null
        /// </summary>
        /// <param name="func"></param>
        public void RegisterDBAccessBuild(Func<DBLocation, DBAccessBuild> func)
        {
            DbAccessCreaterRegister.Add(func);
        }
        public void RegisterLocation<T>(Func<Attribute.TableInnerAttribute, T, Location> func)
        {
            LocationRegister.Add(typeof(T), func);
        }

        internal static Func<Attribute.TableInnerAttribute, T, Location> GetLocation<T>()
        {
            var a = instance.LocationRegister.TryGetValue(typeof(T), out object value);
            if (a)
            {
                return value as Func<Attribute.TableInnerAttribute, T, Location>;
            }
            return null;
        }

        internal static DBHelper GetDBHelper(DBLocation location)
        {
            var dBAccessBuild = GetDBAccessBuild(location);
            return GetDBHelper(dBAccessBuild);
        }
        public static DBHelper GetDBHelper(DBAccessBuild dBAccessBuild)
        {
            var exists = instance.DBHelperRegister.TryGetValue(dBAccessBuild._DBType, out var func);
            if (!exists)
            {
                throw new Exception("未配置对应的数据库类型:" + dBAccessBuild._DBType);
            }
            return func(dBAccessBuild);
        }
        internal static DBAdapterBase GetDBAdapterBase(DbContextInner dbContext)
        {
            var configBuilder = instance;
            var exists = configBuilder.DBAdapterBaseRegister.TryGetValue(dbContext.DBHelper.CurrentDBType, out var func);
            if (!exists)
            {
                throw new Exception("找不到对应的DBAdapte" + dbContext.DBHelper.CurrentDBType);
            }
            return func(dbContext);
        }

        static DBAccessBuild GetDBAccessBuild(DBLocation location)
        {
            foreach (var m in instance.DbAccessCreaterRegister)
            {
                var act = m(location);
                if (act != null)
                {
                    return act;
                }
            }
            throw new Exception($"未找到对应的数据访问实现");
        }
        internal static AbsDBExtend CreateDBExtend(DbContextInner _dbContext)
        {
            var configBuilder = instance;
            var dbType = _dbContext.DBHelper.CurrentDBType;
            if (_dbContext.DataBaseArchitecture == DataBaseArchitecture.Relation)
            {
                return new DBExtend.RelationDB.DBExtend(_dbContext);
            }
            var a = configBuilder.AbsDBExtendRegister.TryGetValue(dbType, out Func<DbContextInner, AbsDBExtend> func);
            if (!a)
            {
                throw new Exception($"未找到AbsDBExtend {dbType}");
            }
            return func(_dbContext);
        }
        internal static Type GetLambdaQueryType(DBType dBType)
        {
            var configBuilder = instance;
            var a = configBuilder.LambdaQueryTypeRegister.TryGetValue(dBType, out Type type);
            if (!a)
            {
                throw new Exception($"未找到对应的LambdaQueryType{dBType}");
            }
            return type;
        }
    }

}
