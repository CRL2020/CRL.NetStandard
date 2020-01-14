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
using CRL.DBAccess;
using CRL.DBAdapter;
using CRL.LambdaQuery;
namespace CRL.Oracle
{
    internal class ORACLEDBAdapter : DBAdapterBase
    {
        public ORACLEDBAdapter(DbContext _dbContext)
            : base(_dbContext)
        {
        }
        public override bool CanCompileSP
        {
            get
            {
                return false;
            }
        }
        public override DBType DBType
        {
            get { return DBType.ORACLE; }
        }
        #region 创建结构
        /// <summary>
        /// 创建存储过程脚本
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public override string GetCreateSpScript(string spName, string script)
        {
            throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            string template = string.Format(@"EXECUTE  ' {1} ';", spName, script);
            return template;
        }

        /// <summary>
        /// 获取字段类型映射
        /// </summary>
        /// <returns></returns>
        public override Dictionary<Type, string> FieldMaping()
        {
            Dictionary<Type, string> dic = new Dictionary<Type, string>();
            //字段类型对应
            dic.Add(typeof(System.String), "VARCHAR2({0})");
            dic.Add(typeof(System.Decimal), "NUMBER");
            dic.Add(typeof(System.Double), "DOUBLE PRECISION");
            dic.Add(typeof(System.Single), "FLOAT(24)");
            dic.Add(typeof(System.Boolean), "INTEGER");
            dic.Add(typeof(System.Int32), "INTEGER");
            dic.Add(typeof(System.Int16), "INTEGER");
            dic.Add(typeof(System.Enum), "INTEGER");
            dic.Add(typeof(System.Byte), "INTEGER");
            dic.Add(typeof(System.DateTime), "TIMESTAMP");
            dic.Add(typeof(System.UInt16), "INTEGER");
            dic.Add(typeof(System.Object), "NARCHAR2(30)");
            dic.Add(typeof(System.Byte[]), "BLOB");
            dic.Add(typeof(System.Guid), "VARCHAR2(50)");
            return dic;
        }

        /// <summary>
        /// 获取列类型和默认值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override string GetColumnType(Attribute.FieldInnerAttribute info, out string defaultValue)
        {
            Type propertyType = info.PropertyType;
            //Dictionary<Type, string> dic = GetFieldMaping();
            defaultValue = info.DefaultValue;

            //int默认值
            if (string.IsNullOrEmpty(defaultValue))
            {
                if (!info.IsPrimaryKey && propertyType == typeof(System.Int32))
                {
                    defaultValue = "0";
                }
                //datetime默认值
                if (propertyType == typeof(System.DateTime))
                {
                    defaultValue = "TIMESTAMP";
                }
            }
            string columnType;
            columnType = GetDBColumnType(propertyType);
            //超过3000设为ntext
            if (propertyType == typeof(System.String) && info.Length > 3000)
            {
                columnType = "CLOB";
            }
            if (info.Length > 0)
            {
                columnType = string.Format(columnType, info.Length);
            }
            //if (info.IsPrimaryKey)
            //{
            //    columnType = "NUMBER(4) Not Null Primary Key";
            //}
            if (info.IsPrimaryKey)
            {
                columnType = " " + columnType + " Primary Key ";
            }

            if (!string.IsNullOrEmpty(info.ColumnType))
            {
                columnType = info.ColumnType;
            }
            return columnType;
        }
        /// <summary>
        /// 创建字段脚本
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override string GetCreateColumnScript(Attribute.FieldInnerAttribute field)
        {
            string str = string.Format("alter table {0} add {1} {2};", field.TableName, field.MapingName, field.ColumnType);
            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                str += string.Format(" default '{0}' ", field.DefaultValue);
            }
            if (field.NotNull)
            {
                str += " not null";
            }
            return str;
        }

        /// <summary>
        /// 创建索引脚本
        /// </summary>
        /// <param name="filed"></param>
        /// <returns></returns>
        public override string GetColumnIndexScript(Attribute.FieldInnerAttribute filed)
        {
            if(filed.IsPrimaryKey)
            {
                return "";
            }
            string indexName = string.Format("pk_{0}_{1}", filed.TableName, filed.MapingName);
            string indexScript = string.Format("create {3} index {0} on {1}({2}); ", indexName, filed.TableName, filed.MapingName, filed.FieldIndexType == Attribute.FieldIndexType.非聚集唯一 ? "UNIQUE" : "");
            return indexScript;
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        public override void CreateTable(DbContext dbContext, List<Attribute.FieldInnerAttribute> fields, string tableName)
        {
            var helper = dbContext.DBHelper;
            var lines = new List<string>();
            //tableName = tableName.ToUpper();
            string script = string.Format("create table {0}(\r\n", tableName);
            List<string> list2 = new List<string>();
            string primaryKey = "id";
            foreach (Attribute.FieldInnerAttribute item in fields)
            {
                if (item.IsPrimaryKey)
                {
                    primaryKey = item.MapingName;
                }
                var columnType = GetDBColumnType(item.PropertyType);
                string nullStr = item.NotNull ? "NOT NULL" : "";
                string str = string.Format("{0} {1} {2} ", item.MapingName, item.ColumnType, nullStr);

                list2.Add(str);

            }
            script += string.Join(",\r\n", list2.ToArray());
            script += ")";
            string sequenceName = string.Format("{0}_sequence", tableName);
            string triggerName = string.Format("{0}_trigge", tableName);
            string sequenceScript = string.Format("Create Sequence {0} MINVALUE 1  MAXVALUE 999999999999 INCREMENT BY 1 START WITH 1 NOCACHE CYCLE", sequenceName);
            string triggerScript = string.Format(@"
create or replace trigger {0}
  before insert on {1}   
  for each row
declare
  nextid number;
begin
  IF :new.{3} IS NULL or :new.{3}=0 THEN
    select {2}.nextval 
    into nextid
    from sys.dual;
    :new.{3}:=nextid;
  end if;
end ;", triggerName, tableName, sequenceName, primaryKey);
            lines.Add(sequenceScript);
            //defaultValues.Add(triggerScript); 暂不用触发器,不能编译成功
            //script += script2;
            helper.SetParam("script", script);
            helper.Run("sp_ExecuteScript");
            //helper.SetParam("script", sequenceScript);
            //helper.Run("sp_ExecuteScript");
            //helper.SetParam("script", triggerScript);
            //helper.Run("sp_ExecuteScript");

            foreach (string s in lines)
            {
                try
                {
                    helper.Execute(s);
                }
                catch (Exception ero) { };
            }
        }
        #endregion

        #region SQL查询
        public override string GetTableFields(string tableName)
        {
            return $"select column_name,column_name from user_tab_columns where Table_Name='{tableName.ToUpper()}';";
        }
        /// <summary>
        /// 批量插入,mysql不支持批量插入
        /// </summary>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public override void BatchInsert(DbContext dbContext, System.Collections.IList details, bool keepIdentity = false)
        {
            var helper = dbContext.DBHelper;
            foreach (var item in details)
            {
                helper.ClearParams();
                InsertObject(dbContext, item as IModel);
            }

        }

        /// <summary>
        /// 获取插入语法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override object InsertObject<T>(DbContext dbContext, T obj)
        {
            Type type = obj.GetType();
            var helper = dbContext.DBHelper;
            var table = TypeCache.GetTable(type);
   
            var primaryKey = table.PrimaryKey;
            object id;
            if (primaryKey.KeepIdentity)
            {
                id = primaryKey.GetValue(obj);
            }
            else
            {
                string sequenceName = string.Format("{0}_sequence", table.TableName);
                var sqlGetIndex = string.Format("select {0}.nextval from dual", sequenceName);//oracle不能同时执行多条语句
                id = SqlStopWatch.ExecScalar(helper, sqlGetIndex);
                primaryKey.SetValue(obj, Convert.ChangeType(id, primaryKey.PropertyType));
            }
            
            var sql = GetInsertSql(dbContext, table, obj);
            //helper.SetParam(primaryKey.MapingName, id);
            SqlStopWatch.Execute(helper, sql);
            //var helper2 = helper as OracleHelper;
            //int id = helper2.Insert(sql,sequenceName);
            return id;
        }
        /// <summary>
        /// 获取 with(nolock)
        /// </summary>
        /// <returns></returns>
        public override string GetWithNolockFormat(bool v)
        {
            return "";
        }
        /// <summary>
        /// 获取前几条语句
        /// </summary>
        /// <param name="fields">id,name</param>
        /// <param name="query">from table where 1=1</param>
        /// <param name="top"></param>
        /// <returns></returns>
        public override void GetSelectTop(StringBuilder sb, string fields, Action<StringBuilder> query,string sort, int top)
        {
            sb.Append("select ");
            sb.Append(fields);
            query(sb);
            if (top > 0)
            {
                if (sb.ToString().ToLower().Contains("where"))
                {
                    sb.Append(" and ROWNUM<=" + top);
                }
                else
                {
                    sb.Append(" where ROWNUM<=" + top);
                }
            }
            sb.Append(sort);
            //string sql = string.Format("select {0} {1} {2} {3}", fields, query, top == 0 ? "" : " and ROWNUM<=" + top,sort);
            //return sql;
        }
        #endregion

        #region 系统查询
        public override string GetAllTablesSql(string db)
        {
            return "SELECT lower(table_name),table_name FROM user_TABLES";
        }
        public override string GetAllSPSql(string db)
        {
            return "select object_name,1 from user_objects where object_type='PROCEDURE'";
        }
        #endregion

        #region 模版
        public override string SpParameFormat(string name, string type, bool output)
        {
            string str = "";
            if (!output)
            {
                str = "{0} in {1},";
            }
            else
            {
                str = "{0} out {1},";
            }
            return string.Format(str, name, type);
        }
        static string keyWords = " ACCESS  ADD  ALL  ALTER  AND  ANY  AS  ASC  AUDIT  BETWEEN  BY  CHAR CHECK  CLUSTER  COLUMN  COMMENT  COMPRESS  CONNECT  CREATE  CURRENT DATE  DECIMAL  DEFAULT  DELETE  DESC  DISTINCT  DROP  ELSE  EXCLUSIVE EXISTS  FILE  FLOAT FOR  FROM  GRANT  GROUP  HAVING  IDENTIFIED IMMEDIATE  IN  INCREMENT  INDEX  INITIAL  INSERT  INTEGER  INTERSECT INTO  IS  LEVEL  LIKE  LOCK  LONG  MAXEXTENTS  MINUS  MLSLABEL  MODE MODIFY  NOAUDIT  NOCOMPRESS  NOT  NOWAIT  NULL  NUMBER  OF  OFFLINE ON  ONLINE  OPTION  OR  ORDER P CTFREE PRIOR PRIVILEGES PUBLIC RAW RENAME RESOURCE REVOKE ROW ROWID ROWNUM ROWS SELECT SESSION SET SHARE SIZE SMALLINT START SUCCESSFUL SYNONYM SYSDATE TABLE THEN TO TRIGGER UID UNION UNIQUE UPDATE USER VALIDATE VALUES VARCHAR VARCHAR2 VIEW WHENEVER WHERE WITH ";
        public override string KeyWordFormat(string value)
        {
            //keyword 
            if (keyWords.Contains(" " + value.ToUpper() + " "))
            {
                return value + "_";
            }
            return value;
        }
        public override string TemplateGroupPage
        {
            get
            {
                throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            }
        }

        public override string TemplatePage
        {
            get
            {
                throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            }
        }

        public override string TemplateSp
        {
            get
            {
                throw new NotSupportedException("ORACLE不支持动态创建存储过程");
            }
        }
        public override string SqlFormat(string sql)
        {
            return System.Text.RegularExpressions.Regex.Replace(sql, @"@(\w+)", ":$1");
        }
        #endregion

        public override string GetColumnUnionIndexScript(string tableName, string indexName, List<string> columns)
        {
            var script = string.Format("create index {1} on {0} ({2}) TABLESPACE users", tableName, indexName, string.Join(",", columns.ToArray()));
            return script;
        }
        public override string DateTimeFormat(string field, string format)
        {
            return string.Format("to_date({0},'{1}')", field, format);
        }

        #region 函数语法
        public override string SubstringFormat(string field, int index, int length)
        {
            return string.Format(" substr({0},{1},{2})", field, index, length);
        }

        public override string StringLikeFormat(string field, string parName)
        {
            return string.Format("{0} LIKE {1}", field, parName);
        }

        public override string StringNotLikeFormat(string field, string parName)
        {
            return string.Format("{0} NOT LIKE {1}", field, parName);
        }

        public override string StringContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})>0", field, parName);
        }
        public override string StringNotContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})<=0", field, parName);
        }

        public override string BetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} between {1} and {2}", field, parName, parName2);
        }
        public override string NotBetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} not between {1} and {2}", field, parName, parName2);
        }
        public override string DateDiffFormat(string field, string format, string parName)
        {
            //todo
            return string.Format("DateDiff({0},{1},{2})", format, field, parName);
        }

        public override string InFormat(string field, string parName)
        {
            return string.Format("{0} IN ({1})", field, parName);
        }
        public override string NotInFormat(string field, string parName)
        {
            return string.Format("{0} NOT IN ({1})", field, parName);
        }
        public override string CastField(string field, Type fieldType)
        {
            var dic = FieldMaping();
            if (!dic.ContainsKey(fieldType))
            {
                throw new CRLException(string.Format("没找到对应类型的转换{0} 在字段{1}", fieldType, field));
            }
            var type = dic[fieldType];
            type = string.Format(type, 100);
            return string.Format("CAST({0} as {1})", field, type);
        }
        public override string IsNotFormat(bool isNot)
        {
            return isNot ? " is not " : " is ";
        }
        public override string ToUpperFormat(string field)
        {
            return string.Format("upper({0})", field);
        }
        public override string ToLowerFormat(string field)
        {
            return string.Format("lower({0})", field);
        }
        public override string IsNull(string field, object value)
        {
            return string.Format("isnull({0},{1})", field, value);
        }
        public override string LengthFormat(string field)
        {
            return string.Format("len({0})", field);
        }
        public override string Trim(string field)
        {
            return string.Format("ltrim(rtrim({0})) ", field);
        }
        public override string TrimStart(string field)
        {
            return string.Format("ltrim({0}) ", field);
        }
        public override string TrimEnd(string field)
        {
            return string.Format("rtrim({0}) ", field);
        }
        public override string Replace(string field, string find, string rep)
        {
            return string.Format("replace({0},{1},{2}) ", field, find, rep);
        }
        public override string Distinct(string field)
        {
            return string.Format("Distinct({0}) ", field);
        }
        public override string DistinctCount(string field)
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
        public override string PageSqlFormat(DBHelper db, string fields, string rowOver, string condition, int start, int end, string sort)
        {
            string sql = "SELECT * FROM (select {0},ROW_NUMBER() OVER ( Order by {1} ) AS RowNumber {2}) T WHERE T.RowNumber BETWEEN {3} AND {4} order by RowNumber";
            return string.Format(sql, fields, rowOver, condition, start, end);
            //return $"select T.* from (select *,rownum rn {condition} order by {sort}) T where rn between {start} and {end};";
        }
        /// <summary>
        /// 获取关联更新语名
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="condition"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override string GetRelationUpdateSql(string t1, string t2, string condition, string setValue, LambdaQuery.LambdaQueryBase query)
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
        public override string GetRelationDeleteSql(string t1, string t2, string condition, LambdaQuery.LambdaQueryBase query)
        {
            string table = string.Format("{0} t1", KeyWordFormat(t1), KeyWordFormat(t2));
            string sql = string.Format("delete t1 from {0} {1}", table, condition);
            return sql;
        }
        public override string GetFieldConcat(string field, object value, Type type)
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
        public override string GetParamName(string name, object index)
        {
            return string.Format(":{0}{1}", name, index);
        }

    }
}
