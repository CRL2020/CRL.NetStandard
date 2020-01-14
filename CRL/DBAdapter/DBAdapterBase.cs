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
using System.Data;
using System.Text.RegularExpressions;
using CRL.DBAccess;

namespace CRL.DBAdapter
{
    public abstract class DBAdapterBase
    {
        //internal DbContext dbContext;
        //protected DBHelper helper;
        protected DBType dbType;
        public DBAdapterBase(DbContext _dbContext)
        {
            //dbContext = _dbContext;
            //helper = dbContext.DBHelper;
            dbType = _dbContext.DBHelper.CurrentDBType;
        }
        /// <summary>
        /// 是否支持编译存储过程
        /// </summary>
        public virtual bool CanCompileSP
        {
            get
            {
                return false;
            }
        }
        static Dictionary<DBType, DBAdapterBase> DBAdapterBaseCache = new Dictionary<DBType, DBAdapterBase>();
        /// <summary>
        /// 根据数据库类型获取适配器
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static DBAdapterBase GetDBAdapterBase(DbContext dbContext)
        {
            DBAdapterBase db = null;
            var a = DBAdapterBaseCache.TryGetValue(dbContext.DBHelper.CurrentDBType, out db);
            if(a)
            {
                return db;
            }
            var configBuilder = SettingConfigBuilder.current;
            var exists = configBuilder.DBAdapterBaseRegister.TryGetValue(dbContext.DBHelper.CurrentDBType, out Func<DbContext, DBAdapter.DBAdapterBase> func);
            if (!exists)
            {
                throw new CRLException("找不到对应的DBAdapte" + dbContext.DBHelper.CurrentDBType);
            }
            return func(dbContext);
        }
        public abstract DBType DBType { get; }
        #region 创建结构
        /// <summary>
        ///获取列类型和默认值
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public abstract string GetColumnType(CRL.Attribute.FieldInnerAttribute info,out string defaultValue);
        /// <summary>
        /// 获取字段类型转换
        /// </summary>
        /// <returns></returns>
        public abstract System.Collections.Generic.Dictionary<Type, string> FieldMaping();
        static Dictionary<DBType, Dictionary<Type, string>> _FieldMaping = new Dictionary<DBType, Dictionary<Type, string>>();
        protected System.Collections.Generic.Dictionary<Type, string> GetFieldMaping()
        {
            if (!_FieldMaping.ContainsKey(dbType))
            {
                _FieldMaping.Add(dbType, FieldMaping());
            }
            return _FieldMaping[dbType];
        }
        /// <summary>
        /// 获取字段数据库类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetDBColumnType(Type type)
        {
            var dic = GetFieldMaping();
            if (!type.FullName.StartsWith("System."))
            {
                //继承的枚举
                type = type.BaseType;
            }
            if (Nullable.GetUnderlyingType(type) != null)
            {
                //Nullable<T> 可空属性
                type = type.GenericTypeArguments[0];
            }
            if (!dic.ContainsKey(type))
            {
                throw new CRLException(string.Format("找不到对应的字段类型映射 {0} 在 {1}", type, this));
            }
            return dic[type];
        }
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="filed"></param>
        /// <returns></returns>
        public abstract string GetColumnIndexScript(CRL.Attribute.FieldInnerAttribute filed);

        public abstract string GetColumnUnionIndexScript(string tableName, string indexName, List<string> columns);

        /// <summary>
        /// 增加列
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public abstract string GetCreateColumnScript(CRL.Attribute.FieldInnerAttribute field);
        /// <summary>
        /// 创建存储过程
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public abstract string GetCreateSpScript(string spName, string script);
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        public abstract void CreateTable(DbContext dbContext, List<Attribute.FieldInnerAttribute> fields, string tableName);
        #endregion

        #region SQL查询
        /// <summary>
        /// 批量插入方法
        /// </summary>
        /// <param name="details"></param>
        /// <param name="keepIdentity">否保持自增主键</param>
        public abstract void BatchInsert(DbContext dbContext, System.Collections.IList details, bool keepIdentity = false);
        public abstract string DateTimeFormat(string field,string format);

        /// <summary>
        /// 查询表所有字段名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract string GetTableFields(string tableName);
        /// <summary>
        /// 获取UPDATE语法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="setString"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string GetUpdateSql(string table, string setString, string where)
        {
            string sql = string.Format("update {0} set {1} {2}", KeyWordFormat(table), setString, where);
            return sql;
        }
        /// <summary>
        /// 获取删除语法
        /// </summary>
        /// <param name="table"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual string GetDeleteSql(string table, string where)
        {
            string sql = string.Format("delete from {0} {1}", KeyWordFormat(table), where);
            return sql;
        }

        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract object InsertObject<T>(DbContext dbContext, T obj);

        static System.Collections.Concurrent.ConcurrentDictionary<string, string> insertSqlCache = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
        protected string GetInsertSql(DbContext dbContext, Attribute.TableInnerAttribute table, object obj, bool fillParame = true)
        {
            Type type = obj.GetType();
            var key = string.Format("{0}_{1}", type, fillParame);
            var helper = dbContext.DBHelper;
            var tableName = table.TableName;
            //var primaryKey = table.PrimaryKey;
            var typeArry = table.Fields;
            //var reflect = ReflectionHelper.GetInfo<T>();
            string sql;
            var cached = insertSqlCache.TryGetValue(key, out sql);
            if (!cached)
            {
                sql = string.Format("insert into {0}(", KeyWordFormat(tableName));
            }
            string sql1 = "";
            string sql2 = "";
            foreach (Attribute.FieldInnerAttribute info in typeArry)
            {
                string name = info.MapingName;
                if (info.IsPrimaryKey && !info.KeepIdentity && DBType != DBType.ORACLE)
                {
                    continue;
                }
                object value = info.GetValue(obj);
  
                value = ObjectConvert.CheckNullValue(value, info.PropertyType);
                if (!cached)
                {
                    sql1 += string.Format("{0},", FieldNameFormat(info));
                    if (fillParame)
                    {
                        var par = GetParamName(name, "");
                        sql2 += string.Format("{0},", par);//@{0}
                    }
                }
                if (fillParame)
                {
                    helper.AddParam(name, value);
                }
            }
            if (!cached)
            {
                sql1 = sql1.Substring(0, sql1.Length - 1)+") values";
                sql += sql1;
                if (fillParame)
                {
                    sql2 = sql2.Substring(0, sql2.Length - 1);
                    sql += "( " + sql2 + ")";
                }
                //sql = SqlFormat(sql);
                insertSqlCache.TryAdd(key, sql);
            }
            return sql;
        }
        /// <summary>
        /// 获取查询前几条
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="query"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public abstract void GetSelectTop(StringBuilder sb, string fields, Action<StringBuilder> query, string sort, int top);

        /// <summary>
        /// 获取with nolock语法
        /// </summary>
        /// <returns></returns>
        public abstract string GetWithNolockFormat(bool v);
        #endregion

        #region  系统查询
        /// <summary>
        /// 获取所有存储过程
        /// </summary>
        /// <returns></returns>
        public abstract string GetAllSPSql(string db);
        /// <summary>
        /// 获取所有表,查询需要转为小写
        /// </summary>
        /// <returns></returns>
        public abstract string GetAllTablesSql(string db);
        #endregion

        #region 模版
        /// <summary>
        /// 存储过程参数格式化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public abstract string SpParameFormat(string name,string type,bool output);
        /// <summary>
        /// 关键字格式化,如SQL为 [field]
        /// </summary>
        public virtual string KeyWordFormat(string value)
        {
            return value;
        }
        public string FieldNameFormat(Attribute.FieldInnerAttribute field)
        {
            if (string.IsNullOrEmpty(field.MapingNameFormat))
            {
                field.MapingNameFormat = KeyWordFormat(field.MapingName);
            }
            return field.MapingNameFormat;
        }
        public virtual string TableNameFormat(Attribute.TableInnerAttribute table)
        {
            return table.TableName;
        }
        /// <summary>
        /// GROUP分页模版
        /// </summary>
        public abstract string TemplateGroupPage { get; }
        /// <summary>
        /// 查询分页模版
        /// </summary>
        public abstract string TemplatePage { get; }
        /// <summary>
        /// 存储过程模版
        /// </summary>
        public abstract string TemplateSp { get; }
        /// <summary>
        /// 语句自定义格式化处理
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual string SqlFormat(string sql)
        {
            return sql;
        }
        int parIndex = 1;
        /// <summary>
        /// 提取SQL参数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="manual"></param>
        /// <returns></returns>
        public virtual string ReplaceParameter(DBHelper db,string sql,bool manual = false)
        {
            if (!SettingConfig.ReplaceSqlParameter && !manual)
            {
                return sql;
            }
            //return sql;
            var re = @"((\s|,)*)(\w+)\s*(>|<|=|!=|>=|<=)\s*('(.*?)'|([1-9]\d*.\d*|0.\d*[1-9]\d*))(\s|,|\))";
            sql = sql + " ";
            if (!Regex.IsMatch(sql, re, RegexOptions.IgnoreCase))
            {
                return sql;
            }
            Regex r = new Regex(re, RegexOptions.IgnoreCase);
            List<string> pars = new List<string>();
            //int index = 1;
            for (var m = r.Match(sql); m.Success; m = m.NextMatch())
            {
                var name = m.Groups[3];
                var op = m.Groups[4];
                var value1 = m.Groups[6];
                var value2 = m.Groups[7];
                var value = string.IsNullOrEmpty(value2.Value) ? value1 : value2;
                var p = m.Groups[1];
                var p2 = m.Groups[8];
                var pName = GetParamName("_p", parIndex);
                db.AddParam(pName, value.ToString());
                sql = sql.Replace(m.ToString(), string.Format("{0}{1}{4}{2}{3} ", p, name, pName, p2, op));
                parIndex += 1;
            }
            return sql;
        }
        #endregion

        #region 函数语法
        public virtual string SubstringFormat(string field, int index, int length)
        {
            return string.Format(" SUBSTRING({0},{1},{2})", field, index, length);
        }

        public virtual string StringLikeFormat(string field, string parName)
        {
            return string.Format("{0} LIKE {1}", field, parName);
        }

        public virtual string StringNotLikeFormat(string field, string parName)
        {
            return string.Format("{0} NOT LIKE {1}", field, parName);
        }

        public virtual string StringContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})>0", field, parName);
        }
        public virtual string StringNotContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})<=0", field, parName);
        }

        public virtual string BetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} between {1} and {2}", field, parName, parName2);
        }
        public virtual string NotBetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} not between {1} and {2}", field, parName, parName2);
        }
        public virtual string DateDiffFormat(string field, string format, string parName)
        {
            return string.Format("DateDiff({0},{1},{2})", format, field, parName);
        }

        public virtual string InFormat(string field, string parName)
        {
            return string.Format("{0} IN ({1})", field, parName);
        }
        public virtual string NotInFormat(string field, string parName)
        {
            return string.Format("{0} NOT IN ({1})", field, parName);
        }
        public abstract string CastField(string field,Type fieldType);
        public virtual string IsNotFormat(bool isNot)
        {
            return isNot ? " is not " : " is ";
        }
        public virtual string ToUpperFormat(string field)
        {
            return string.Format("upper({0})",field);
        }
        public virtual string ToLowerFormat(string field)
        {
            return string.Format("lower({0})", field);
        }
        public virtual string IsNull(string field, object value)
        {
            return string.Format("isnull({0},{1})", field, value);
        }
        public virtual string LengthFormat(string field)
        {
            return string.Format("len({0})", field);
        }
        public virtual string Trim(string field)
        {
            return string.Format("ltrim(rtrim({0})) ", field);
        }
        public virtual string TrimStart(string field)
        {
            return string.Format("ltrim({0}) ", field);
        }
        public virtual string TrimEnd(string field)
        {
            return string.Format("rtrim({0}) ", field);
        }
        public virtual string Replace(string field, string find, string rep)
        {
            return string.Format("replace({0},{1},{2}) ", field, find, rep);
        }
        public virtual string Distinct(string field)
        {
            return string.Format("Distinct({0}) ", field);
        }
        public virtual string DistinctCount(string field)
        {
            return string.Format("count(Distinct({0})) ", field);
        }
        #endregion

        /// <summary>
        /// 分页SQL 默认为MSSQL
        /// </summary>
        /// <param name="db"></param>
        /// <param name="fields"></param>
        /// <param name="rowOver"></param>
        /// <param name="condition"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public virtual string PageSqlFormat(DBHelper db, string fields, string rowOver, string condition,int start,int end,string sort)
        {
            string sql = "SELECT * FROM (select {0},ROW_NUMBER() OVER ( Order by {1} ) AS RowNumber {2}) T WHERE T.RowNumber BETWEEN {3} AND {4} order by RowNumber";
            return string.Format(sql, fields, rowOver, condition, start, end);
        }
        /// <summary>
        /// 获取关联更新语名
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="condition"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public virtual string GetRelationUpdateSql(string t1, string t2, string condition, string setValue, LambdaQuery.LambdaQueryBase query)
        {
            string table = string.Format("{0} t1", KeyWordFormat(t1), KeyWordFormat(t2));
            string sql = string.Format("update t1 set {0} from {1} {2}", setValue, table, condition);
            return sql;
        }
        /// <summary>
        /// 获取关联删除语句
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public virtual string GetRelationDeleteSql(string t1, string t2, string condition, LambdaQuery.LambdaQueryBase query)
        {
            string table = string.Format("{0} t1", KeyWordFormat(t1), KeyWordFormat(t2));
            string sql = string.Format("delete t1 from {0} {1}", table, condition);
            return sql;
        }
        public virtual string GetFieldConcat(string field,object value,Type type)
        {
            string str;
            if (type == typeof(string))
            {
                str = string.Format("{0}+'{1}'", field, value);
            }
            else
            {
                str = string.Format("{0}+{1}", field, value);
            }
            return str;
        }
        /// <summary>
        /// 参数名
        /// </summary>
        public abstract string GetParamName(string name, object index);
    }
}
