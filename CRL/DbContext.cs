/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL
{
    /// <summary>
    /// 数据访问上下文
    /// </summary>
    public class DbContext
    {
        /// <summary>
        /// 数据访问上下文
        /// </summary>
        /// <param name="dbHelper"></param>
        /// <param name="dbLocation"></param>
        public DbContext(DBHelper dbHelper, DBLocation dbLocation)
        {
            DBHelper = dbHelper;
            DBLocation = dbLocation;
            //todo 按数据库类型类型判断
            DataBaseArchitecture = dbHelper.CurrentDBType == DBType.MongoDB ? DataBaseArchitecture.NotRelation : CRL.DataBaseArchitecture.Relation;
        }
        /// <summary>
        /// 数据库架构类型
        /// </summary>
        internal DataBaseArchitecture DataBaseArchitecture;
        /// <summary>
        /// 数据库连接定位
        /// </summary>
        public DBLocation DBLocation;
        /// <summary>
        /// 数据访问
        /// </summary>
        public DBHelper DBHelper;
        /// <summary>
        /// 是否使用分表定位
        /// </summary>
        public bool UseSharding = false;

        /// <summary>
        /// 当前查询参数索引
        /// 为了让多次操作能串行,参数索引放在这
        /// </summary>
        internal int parIndex = 0;
        internal DBHelper GetDBHelper(DataAccessType accessType = DataAccessType.Default)
        {
            DBLocation.DataAccessType = accessType;
            var helper = SettingConfig.GetDBAccessBuild(DBLocation).GetDBHelper();
            return helper;
        }
    }
    /// <summary>
    /// 数据访问类型
    /// </summary>
    public enum DataAccessType
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default,
        /// <summary>
        /// 读
        /// </summary>
        Read
    }
    /// <summary>
    /// 数据库连接定位
    /// 通过判断Type或DataBase判断数据连接
    /// 优先ShardingDataBase判断
    /// </summary>
    public class DBLocation
    {
        /// <summary>
        /// 调用的类型
        /// </summary>
        public Type ManageType;
        /// <summary>
        /// 对象管理名称
        /// </summary>
        public string ManageName;
        /// <summary>
        /// 分库指定的数据库
        /// </summary>
        public Sharding.Location ShardingLocation;
        /// <summary>
        /// 访问类型,当不是在事务内查询时,返回Read
        /// </summary>
        public DataAccessType DataAccessType;
    }
}
