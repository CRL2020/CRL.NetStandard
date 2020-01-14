/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.DBAccess;
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
  

    /// <summary>
    /// 表示CacheServer处理数据的方法委托
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public delegate CacheServer.ResultData ExpressionDealDataHandler(CacheServer.Command command);

    /// <summary>
    /// as bool TransMethod(out string error);
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public delegate bool TransMethod(out string error);

    public class DBAccessBuild
    {
        DBType _DBType;
        string _connectionString;
        public DBAccessBuild(DBType dbType, string connectionString)
        {
            _DBType = dbType;
            _connectionString = connectionString;
        }
        internal DBHelper GetDBHelper()
        {
            var configBuilder = SettingConfigBuilder.current;
            var exists = configBuilder.DBHelperRegister.TryGetValue(_DBType, out Func<string, DBHelper> func);
            if (!exists)
            {
                throw new CRLException("未配置对应的数据库类型:" + _DBType);
            }
            return func(_connectionString);
        }
    }
    /// <summary>
    /// 框架部署,请实现委托
    /// </summary>
    public class SettingConfig
    {
        static SettingConfig()
        {
            #region 注册默认数据库类型
            var configBuilder = SettingConfigBuilder.current;
            configBuilder.RegisterDBType(DBType.MSSQL, (conn) =>
             {
                 return new SqlHelper(conn);
             }, (context) =>
             {
                 return new DBAdapter.MSSQLDBAdapter(context);
             });
            configBuilder.RegisterDBType(DBType.MSSQL2000, (conn) =>
            {
                return new Sql2000Helper(conn);
            }, (context) =>
            {
                return new DBAdapter.MSSQL2000DBAdapter(context);
            });
            configBuilder.RegisterDBExtend<CRL.DBExtend.RelationDB.DBExtend>(DBType.MSSQL, (context) =>
             {
                 return new DBExtend.RelationDB.DBExtend(context);
             });
            #endregion
        }
        #region 委托
        /// <summary>
        /// 获取数据连接
        /// </summary>
        [Obsolete("使用SettingConfigBuilder内实现")]
        public static Func<DBLocation, DBAccessBuild> GetDbAccess
        {
            set
            {
                var configBuilder = SettingConfigBuilder.current;
                configBuilder.DbAccessCreaterCache.Add(value);
            }
        }
        /// <summary>
        /// 注册数据访问实现
        /// 按优先顺序添加,不成立则返回null
        /// </summary>
        /// <param name="func"></param>
        [Obsolete("使用SettingConfigBuilder内实现")]
        public static void RegisterDBAccessBuild(Func<DBLocation, DBAccessBuild> func)
        {
            var configBuilder = SettingConfigBuilder.current;
            configBuilder.DbAccessCreaterCache.Add(func);
        }
        internal static DBAccessBuild GetDBAccessBuild(DBLocation location)
        {
            var configBuilder = SettingConfigBuilder.current;
            foreach (var m in configBuilder.DbAccessCreaterCache)
            {
                var act = m(location);
                if (act != null)
                {
                    return act;
                }
            }
            throw new CRLException($"未找到对应的数据访问实现");
        }

        #endregion

        /// <summary>
        /// 清除所有内置缓存
        /// </summary>
        public static void ClearCache(string dataBase)
        {
            MemoryDataCache.CacheService.Clear(dataBase);
        }
        #region 设置
        /// <summary>
        /// 是否使用属性更改通知
        /// 如果使用了,在查询时就不设置源对象克隆
        /// 在实现了属性构造后,可设为true
        /// </summary>
        public static bool UsePropertyChange = false;

        /// <summary>
        /// string字段默认长度
        /// </summary>
        public static int StringFieldLength = 30;
        /// <summary>
        /// 是否检测表结构,生产服务器可将此值设为FALSE
        /// </summary>
        public static bool CheckModelTableMaping = true;

        /// <summary>
        /// 是否自动跟踪对象状态
        /// 为否则需要调用IMode.BeginTracking(),使更新时能识别
        /// query.__TrackingModel,同时生效,才进行跟踪
        /// </summary>
        public static bool AutoTrackingModel = true;

        /// <summary>
        /// 是否使用主从读写分离
        /// 启用后,不会自动检查表结构
        /// 在事务范围内,查询按主库
        /// </summary>
        public static bool UseReadSeparation = false;

        /// <summary>
        /// 是否记录SQL语句调用
        /// </summary>
        public static bool LogSql = false;
        /// <summary>
        /// 生成参数是否与字段名一致
        /// </summary>
        public static bool FieldParameName = false;
        /// <summary>
        /// 是否替换SQL拼接参数
        /// </summary>
        public static bool ReplaceSqlParameter = false;//生成存储过程时不能替换
        /// <summary>
        /// 默认nolock
        /// </summary>
        public static bool QueryWithNoLock = true;
        /// <summary>
        /// 分页是否编译存储过程
        /// </summary>
        public static bool CompileSp = true;
        #endregion
    }


    public class SettingConfigBuilder
    {
        internal Dictionary<DBType, Func<string, DBHelper>> DBHelperRegister = new Dictionary<DBType, Func<string, DBHelper>>();
        internal Dictionary<DBType, Func<DbContext, DBAdapter.DBAdapterBase>> DBAdapterBaseRegister = new Dictionary<DBType, Func<DbContext, DBAdapter.DBAdapterBase>>();
        internal Dictionary<DBType, Func<DbContext, AbsDBExtend>> AbsDBExtendRegister = new Dictionary<DBType, Func<DbContext, AbsDBExtend>>();

        internal List<Func<DBLocation, DBAccessBuild>> DbAccessCreaterCache = new List<Func<DBLocation, DBAccessBuild>>();

        internal Dictionary<Type, object> LocationRegister = new Dictionary<Type, object>();

        public SettingConfigBuilder()
        {
            current = this;
        }
        static SettingConfigBuilder()
        {
            current = new SettingConfigBuilder();
        }
        internal static SettingConfigBuilder current;

        public SettingConfigBuilder RegisterDBType(DBType dBType, Func<string, DBHelper> funcDb, Func<DbContext, DBAdapter.DBAdapterBase> funcDBAdapter)
        {
            if (!DBHelperRegister.ContainsKey(dBType))
            {
                DBHelperRegister.Add(dBType, funcDb);
                DBAdapterBaseRegister.Add(dBType, funcDBAdapter);
            }
            return this;
        }
        public SettingConfigBuilder RegisterDBExtend<T1>(DBType dBType, Func<DbContext, AbsDBExtend> func) where T1 : AbsDBExtend
        {
            if (!AbsDBExtendRegister.ContainsKey(dBType))
            {
                AbsDBExtendRegister.Add(dBType, func);
            }
            return this;
        }

        /// <summary>
        /// 获取数据连接
        /// </summary>
        public Func<DBLocation, DBAccessBuild> GetDbAccess
        {
            set
            {
                DbAccessCreaterCache.Add(value);
            }
        }
        /// <summary>
        /// 注册数据访问实现
        /// 按优先顺序添加,不成立则返回null
        /// </summary>
        /// <param name="func"></param>
        public void RegisterDBAccessBuild(Func<DBLocation, DBAccessBuild> func)
        {
            DbAccessCreaterCache.Add(func);
        }
        public void RegisterLocation<T>(Func<Attribute.TableInnerAttribute, T, Location> func)
        {
            LocationRegister.Add(typeof(T), func);
        }
        internal Func<Attribute.TableInnerAttribute, T, Location> GetLocation<T>()
        {
            var a = LocationRegister.TryGetValue(typeof(T), out object value);
            if (a)
            {
                return value as Func<Attribute.TableInnerAttribute, T, Location>;
            }
            return null;
        }
    }

}
