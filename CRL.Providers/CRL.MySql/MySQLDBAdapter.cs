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

using CRL.DBAccess;
using CRL.DBAdapter;
using CRL.LambdaQuery;

namespace CRL.MySql
{
    internal class MySQLDBAdapter : DBAdapterBase
    {
        public MySQLDBAdapter(DbContextInner _dbContext)
            : base(_dbContext)
        {
        }
        #region 创建结构
        /// <summary>
        /// 创建存储过程脚本
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public override string GetCreateSpScript(string spName, string script)
        {
            throw new NotSupportedException("MySql不支持动态创建存储过程");
        }

        /// <summary>
        /// 获取字段类型映射
        /// </summary>
        /// <returns></returns>
        public override Dictionary<Type, string> FieldMaping()
        {
            Dictionary<Type, string> dic = new Dictionary<Type, string>();
            //字段类型对应
            dic.Add(typeof(System.String), "varchar({0})");
            dic.Add(typeof(System.Decimal), "decimal(18, 2)");
            dic.Add(typeof(System.Double), "float");
            dic.Add(typeof(System.Single), "real");
            dic.Add(typeof(System.Boolean), "tinyint(1)");
            dic.Add(typeof(System.Int32), "int");
            dic.Add(typeof(System.Int16), "SMALLINT");
            dic.Add(typeof(System.Int64), "bigint");
            dic.Add(typeof(System.Enum), "int");
            dic.Add(typeof(System.Byte), "SMALLINT");
            dic.Add(typeof(System.DateTime), "datetime");
            dic.Add(typeof(System.UInt16), "SMALLINT");
            dic.Add(typeof(System.Object), "varchar(30)");
            dic.Add(typeof(System.Byte[]), "varbinary({0})");
            dic.Add(typeof(System.Guid), "varchar(50)");
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
                    defaultValue = " CURRENT_TIMESTAMP";
                }
            }
            string columnType;

            columnType = GetDBColumnType(propertyType);
            //超过3000设为ntext
            if (propertyType == typeof(System.String) && info.Length > 3000)
            {
                columnType = "text";
            }
            if (info.Length > 0)
            {
                columnType = string.Format(columnType, info.Length);
            }
            if (info.IsPrimaryKey)
            {
                if (info.KeepIdentity)
                {
                    columnType = " " + columnType + " primary key";
                }
                else
                {
                    //todo 只有数值型才能自增
                    if (info.PropertyType != typeof(string))
                    {
                        columnType = " " + columnType + " primary key auto_increment";
                    }
                }
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
            string str = string.Format("alter table `{0}` add {1} {2}", field.TableName, field.MapingName, field.ColumnType);
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
            //            ALTER TABLE table_name ADD INDEX index_name (column_list)
            //ALTER TABLE table_name ADD UNIQUE(column_list)
            //ALTER TABLE table_name ADD PRIMARY KEY(column_list)
            if(filed.IsPrimaryKey)
            {
                return "";
                return string.Format("ALTER TABLE `{0}` modify `{1}` int auto_increment", filed.TableName, filed.MapingName);
            }
            string indexScript = string.Format("ALTER TABLE `{0}` ADD {2} ({1}) ", filed.TableName, filed.MapingName,
                filed.FieldIndexType == Attribute.FieldIndexType.非聚集唯一 ? "UNIQUE" : "INDEX index_" + filed.MapingName);
            return indexScript;
        }

        /// <summary>
        /// 创建表脚本
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override void CreateTable(DbContextInner dbContext, List<Attribute.FieldInnerAttribute> fields, string tableName)
        {
            var helper = dbContext.DBHelper;
            var defaultValues = new List<string>();
            string script = string.Format("create table {0}(\r\n", KeyWordFormat(tableName));
            List<string> list2 = new List<string>();
            foreach (Attribute.FieldInnerAttribute item in fields)
            {
                string nullStr = item.NotNull ? "NOT NULL" : "";
                var columnType = GetDBColumnType(item.PropertyType);

                string str = string.Format("{0} {1} {2} ", KeyWordFormat(item.MapingName), item.ColumnType, nullStr);

                list2.Add(str);
                
            }
            script += string.Join(",\r\n", list2.ToArray());
            script += ") charset utf8 collate utf8_general_ci;";
            helper.Execute(script);
            foreach (string s in defaultValues)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    helper.Execute(s);
                }
            }
        }
        #endregion
        public override DBType DBType
        {
            get { return DBType.MYSQL; }
        }
        #region SQL查询
        public override string GetTableFields(string tableName)
        {
            return "select  column_name, column_name  from Information_schema.columns  where table_Name = '" + tableName + "';";
        }
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public override void BatchInsert(DbContextInner dbContext, System.Collections.IList details, bool keepIdentity = false)
        {
            if (details.Count == 0)
                return;
            var type = details[0].GetType();
            var table = TypeCache.GetTable(type);
            var tableName = KeyWordFormat(table.TableName);
            var helper = dbContext.DBHelper;

            var sb = new StringBuilder();
            GetSelectTop(sb, "*", b =>
            {
                b.Append(" from " + tableName + " where 1=0");
            }, "", 1);
            var sql = sb.ToString();
            var tempTable = helper.ExecDataTable(sql);

            var typeArry = table.Fields;
            foreach (var item in details)
            {
                var dr = tempTable.NewRow();
                foreach (Attribute.FieldInnerAttribute info in typeArry)
                {
                    string name = info.MapingName;
                    object value = info.GetValue(item);
                    if (!keepIdentity)
                    {
                        if (info.IsPrimaryKey)
                            continue;
                    }
                    var value2 = ObjectConvert.CheckNullValue(value, info.PropertyType);
                    dr[name] = value2;
                }
                tempTable.Rows.Add(dr);
            }
            helper.InsertFromDataTable(tempTable, tableName, keepIdentity);
        }

        /// <summary>
        /// 获取插入语法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override object InsertObject<T>(DbContextInner dbContext, T obj)
        {
            Type type = obj.GetType();
            var helper = dbContext.DBHelper;
            var table = TypeCache.GetTable(type);
            var primaryKey = table.PrimaryKey;
            var sql = GetInsertSql(dbContext, table, obj);
            if (primaryKey.KeepIdentity)
            {
                SqlStopWatch.Execute(helper,sql);
                return primaryKey.GetValue(obj);
            }
            else
            {
                sql += ";SELECT LAST_INSERT_ID();";
                return SqlStopWatch.ExecScalar(helper, sql);
            }
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
            //string sql = string.Format("select {1} {2} {3} {0}", top == 0 ? "" : " LIMIT 0, " + top, fields, query, sort);
            //return sql;

            sb.Append("select ");
            sb.Append(fields);
            query(sb);
            if (!string.IsNullOrEmpty(sort))
            {
                sb.Append(sort);
            }
            sb.Append(top == 0 ? "" : " LIMIT 0," + top);
        }
        #endregion

        #region 系统查询
        public override string GetAllTablesSql(string db)
        {
            return "select lower(table_name),table_name from information_schema.tables where table_schema='" + db + "' ";
        }
        public override string GetAllSPSql(string db)
        {
            return "select `name`,1 from mysql.proc where db = '" + db + "' and `type` = 'PROCEDURE' ";
        }
        #endregion

        #region 模版
        public override string SpParameFormat(string name, string type, bool output)
        {
            string str = "";
            if (!output)
            {
                str = "in {0} {1},";
            }
            else
            {
                str = "out {0} {1},";
            }
            return string.Format(str, name, type);
        }

        public override string KeyWordFormat(string value)
        {
            return string.Format("`{0}`", value);
        }
        public override string TemplateGroupPage
        {
            get
            {
                throw new NotSupportedException("MySql不支持动态创建存储过程");
            }
        }

        public override string TemplatePage
        {
            get
            {
                throw new NotSupportedException("MySql不支持动态创建存储过程");
            }
        }

        public override string TemplateSp
        {
            get
            {
                throw new NotSupportedException("MySql不支持动态创建存储过程");
            }
        }
        public override string SqlFormat(string sql)
        {
            if (sql.Contains("@"))
            {
                sql = System.Text.RegularExpressions.Regex.Replace(sql, @"@(\w+)", "?$1");
            }
            if(System.Text.RegularExpressions.Regex.IsMatch(sql, @"\[(\w+)\]"))
            {
                sql = System.Text.RegularExpressions.Regex.Replace(sql, @"\[(\w+)\]", "`$1`");
            }
            return sql;
        }
        #endregion

        public override string SubstringFormat(string field, int index, int length)
        {
            return string.Format(" substring({0},{1},{2})", field, index + 1, length);
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
            return string.Format("find_in_set({1},{0})", field, parName);
        }
        public override string StringNotContainsFormat(string field, string parName)
        {
            return string.Format("not find_in_set({1},{0})", field, parName);
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
            return string.Format("DateDiff({0},{1})", field, parName);
        }

        public override string InFormat(string field, string parName)
        {
            return string.Format("{0} IN ({1})", field, parName);
        }

        public override string NotInFormat(string field, string parName)
        {
            return string.Format("{0} NOT IN ({1})", field, parName);
        }
        public override string PageSqlFormat(DBHelper db, string fields, string rowOver, string condition, int start, int end, string sort)
        {
            start -= 1;
            if (start < 0)
            {
                start = 0;
            }
            db.AddParam("?start", start);
            db.AddParam("?row", end - start);
            string sql = "SELECT {0} {1} {4} limit {2},{3} ";
            return string.Format(sql, fields, condition, "?start", "?row", string.IsNullOrEmpty(sort) ? "" : "order by " + sort);
        }
        public override string GetRelationUpdateSql(string t1, string t2, string condition, string setValue, LambdaQuery.LambdaQueryBase query)
        {
            //update table1,table2, set a=1 where table1.id=table2.id and  id>2
            string table = string.Format("{0} t1,{1} t2", KeyWordFormat(t1), KeyWordFormat(t2));
            var where = query.__Relations.First().Value.condition;
            if (query.Condition.Length > 0)
            {
                where += string.Format(" and {0}", query.Condition);
            }
            string sql = string.Format(@"UPDATE {0} SET {1} where {2}", table, setValue, where);
            return sql;
        }
        public override string GetRelationDeleteSql(string t1, string t2, string condition, LambdaQueryBase query)
        {
            string table = string.Format("{0} t1,{1} t2", KeyWordFormat(t1), KeyWordFormat(t2));
            var where = query.__Relations.First().Value.condition;
            if (query.Condition.Length > 0)
            {
                where += string.Format(" and {0}", query.Condition);
            }
            string sql = string.Format("delete t1 from {0} where {1}", table, where);
            return sql;
        }
        static Dictionary<Type, string> castDic = new Dictionary<Type, string>();
        public override string CastField(string field, Type fieldType)
        {
            //CAST其中类型可以为：
            //CHAR[(N)] 字符型
            //DATE  日期型
            //DATETIME  日期和时间型
            //DECIMAL  float型
            //SIGNED  int
            //TIME  时间型
            if (castDic.Count == 0)
            {
                castDic.Add(typeof(string), "CHAR");
                castDic.Add(typeof(DateTime), "DATETIME");
                castDic.Add(typeof(int), "SIGNED");
                castDic.Add(typeof(float), "DECIMAL");
                castDic.Add(typeof(TimeSpan), "TIME");
            }
            if (!castDic.ContainsKey(fieldType))
            {
                throw new System.Exception(string.Format("没找到对应类型的转换{0} 在字段{1}", fieldType, field));
            }
            var type = castDic[fieldType];
            //type = string.Format(type, 100);
            return string.Format("CAST({0} as {1})", field, type);
        }
        public override string IsNull(string field, object value)
        {
            return string.Format("IFNULL({0},{1})", field, value);
        }
        public override string GetFieldConcat(string field, object value, Type type)
        {
            if (type == typeof(string))
            {
                return string.Format("concat('{0}',{1})", value, field);
            }
            else
            {
                return string.Format("{0}+{1}", field, value);
            }
        }
        public override string GetParamName(string name, object index)
        {
            return string.Format("?{0}{1}", name, index);
        }
        public override string GetColumnUnionIndexScript(string tableName, string indexName, List<string> columns, Attribute.FieldIndexType fieldIndexType)
        {
            var script = string.Format("create index `{1}` on `{0}` ({2})", tableName, indexName, string.Join(",", columns.ToArray()));
            return script;
        }
        public override string DateTimeFormat(string field, string format)
        {
            return string.Format("date_format({0},'{1}')", field, format);
        }
        public override string GetSplitFirst(string field, string parName)
        {
            throw new NotImplementedException();
        }
    }
}
