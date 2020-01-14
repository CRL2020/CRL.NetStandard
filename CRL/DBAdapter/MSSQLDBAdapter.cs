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

namespace CRL.DBAdapter
{

    internal class MSSQLDBAdapter : DBAdapterBase
    {
        public MSSQLDBAdapter(DbContext _dbContext)
            : base(_dbContext)
        {
        }
        public override bool CanCompileSP
        {
            get
            {
                return true;
            }
        }
        #region 创建结构

        /// <summary>
        /// 创建存储过程脚本
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public override string GetCreateSpScript(string spName, string script)
        {
            string template = string.Format(@"
if not exists(select * from sysobjects where name='{0}' and type='P')
begin
exec sp_executesql N' {1} '
end", spName, script);
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
            dic.Add(typeof(System.String), "nvarchar({0})");
            dic.Add(typeof(System.Decimal), "decimal(18, 2)");
            dic.Add(typeof(System.Double), "float");
            dic.Add(typeof(System.Single), "real");
            dic.Add(typeof(System.Boolean), "bit");
            dic.Add(typeof(System.Int32), "int");
            dic.Add(typeof(System.Int16), "SMALLINT");
            dic.Add(typeof(System.Enum), "int");
            dic.Add(typeof(System.Byte), "tinyint");
            dic.Add(typeof(System.DateTime), "datetime");
            dic.Add(typeof(System.UInt16), "SMALLINT");
            dic.Add(typeof(System.Int64), "bigint");
            dic.Add(typeof(System.Object), "nvarchar(30)");
            dic.Add(typeof(System.Byte[]), "varbinary({0})");
            dic.Add(typeof(System.Guid), "uniqueidentifier");
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
                    defaultValue = "(0)";
                }
                //datetime默认值
                if (propertyType == typeof(System.DateTime))
                {
                    defaultValue = "(getdate())";
                }
            }
            string columnType;
            columnType = GetDBColumnType(propertyType);
            //超过3000设为ntext
            if (propertyType == typeof(System.String) && info.Length > 3000)
            {
                columnType = "ntext";
            }
            if (info.Length > 0)
            {
                columnType = string.Format(columnType, info.Length);
            }
            if (info.IsPrimaryKey)
            {
                if (info.KeepIdentity)
                {
                    columnType = columnType + " ";
                }
                else
                {
                    //todo 只有数值型才能自增
                    if (info.PropertyType != typeof(string))
                    {
                        columnType = columnType + " IDENTITY(1,1) ";
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
            string str = string.Format("alter table [{0}] add {1} {2}", field.TableName, field.MapingName, field.ColumnType);
            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                str += string.Format(" default({0})", field.DefaultValue);
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
            if (filed.IsPrimaryKey)
            {
                return "";
                //mssql不能用语句设为自增
                //return string.Format("alter table [{0}] add constraint PK{0} primary key ([{0}])", filed.TableName, filed.MapingName);
            }
            string indexScript = string.Format("CREATE {2} NONCLUSTERED INDEX  IX_INDEX_{0}_{1}  ON dbo.[{0}]([{1}])", filed.TableName, filed.MapingName, filed.FieldIndexType == Attribute.FieldIndexType.非聚集唯一 ? "UNIQUE" : "");
            return indexScript;
        }

        /// <summary>
        /// 创建表脚本
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override void CreateTable(DbContext dbContext, List<Attribute.FieldInnerAttribute> fields, string tableName)
        {
            var defaultValues = new List<string>();
            string script = string.Format("create table [{0}] (\r\n", tableName);
            List<string> list2 = new List<string>();
            string primaryKey = "";
            foreach (Attribute.FieldInnerAttribute item in fields)
            {
                if (item.IsPrimaryKey)
                {
                    primaryKey = item.MapingName;
                }
                string nullStr = item.NotNull ? "NOT NULL" : "";
                string str = string.Format("[{0}] {1} {2} ", item.MapingName, item.ColumnType, nullStr);
                list2.Add(str);
                //生成默认值语句
                if (!string.IsNullOrEmpty(item.DefaultValue))
                {
                    string v = string.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}]", tableName, item.MapingName, item.DefaultValue);
                    defaultValues.Add(v);
                }
            }
            script += string.Join(",\r\n", list2.ToArray());
            if (!string.IsNullOrEmpty(primaryKey))
            {
                script += string.Format(@" CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
(
	[{1}] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
", tableName, primaryKey);

            }
            script += ") ON [PRIMARY]";
            //var list3 = GetIndexScript();
            //defaultValues.AddRange(list3);
            var helper = dbContext.DBHelper;
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
            get { return DBType.MSSQL; }
        }
        #region SQL查询

        public override string GetTableFields(string tableName)
        {
            string sql = "Select name,name from syscolumns Where ID=OBJECT_ID('" + tableName + "')";
            return sql;
        }
        //static System.Collections.Concurrent.ConcurrentDictionary <string, DataTable> cacheTables = new System.Collections.Concurrent.ConcurrentDictionary<string, DataTable>();
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="details"></param>
        /// <param name="keepIdentity"></param>
        public override void BatchInsert(DbContext dbContext, System.Collections.IList details, bool keepIdentity = false)
        {
            if (details.Count == 0)
                return;
            var type = details[0].GetType();
            var table = TypeCache.GetTable(type);
            var tableName = KeyWordFormat(table.TableName);
            var helper = dbContext.DBHelper;
            //DataTable tempTable;
            //var key = string.Format("{0}_{1}", dbContext.DBHelper.DatabaseName, type.Name);
            //if (!cacheTables.ContainsKey(key))
            //{
            //    var sb = new StringBuilder();
            //    GetSelectTop(sb,"*", b=>
            //    {
            //        b.Append(" from " + tableName + " where 1=0");
            //    }, "", 1);
            //    var sql = sb.ToString();
            //    var tempTable2 = helper.ExecDataTable(sql);
            //    //cacheTables.TryAdd(key, tempTable2);//暂每次动态查询
            //    tempTable = tempTable2.Clone();//创建一个副本
            //}
            //else
            //{
            //    tempTable = cacheTables[key].Clone();
            //}
            var sb = new StringBuilder();
            GetSelectTop(sb, "*", b =>
            {
                b.Append(" from " + tableName + " where 1=0");
            }, "", 1);
            var sql = sb.ToString();
            var tempTable = helper.ExecDataTable(sql);

            ////字段顺序得和表一至,不然插入出错
            //DataTable tempTable = new DataTable() { TableName = tableName };
            //foreach (var f in table.Fields)
            //{
            //    var column = new DataColumn() { ColumnName = f.MapingName, DataType = f.PropertyType, AllowDBNull = true };
            //    tempTable.Columns.Add(column);
            //}
            var typeArry = table.Fields;
            foreach (var item in details)
            {
                DataRow dr = tempTable.NewRow();
                foreach (Attribute.FieldInnerAttribute info in typeArry)
                {
                    string name = info.MapingName;
                    object value = info.GetValue(item);
                    if (!keepIdentity)
                    {
                        if (info.IsPrimaryKey)
                            continue;
                    }
                    var value2 = ObjectConvert.CheckNullValue(value,info.PropertyType);
                    dr[name] = value2;
                }
                tempTable.Rows.Add(dr);
            }
            helper.InsertFromDataTable(tempTable, tableName, keepIdentity);
        }

        /// <summary>
        /// 插入对象,并返回主键
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override object InsertObject<T>(DbContext dbContext, T obj)
        {
            Type type = obj.GetType();
            var helper = dbContext.DBHelper;
            var table = TypeCache.GetTable(type);
            var primaryKey = table.PrimaryKey;
            
            var sql = GetInsertSql(dbContext, table, obj);
            if (primaryKey.KeepIdentity)
            {
                SqlStopWatch.Execute(helper, sql);
                return primaryKey.GetValue(obj);
            }
            else
            {
                sql += ";SELECT scope_identity() ;";
                return SqlStopWatch.ExecScalar(helper,sql);
            }
        }
        /// <summary>
        /// 获取 with(nolock)
        /// </summary>
        /// <returns></returns>
        public override string GetWithNolockFormat(bool v)
        {
            if (!v)
            {
                return "";
            }
            return " with (nolock)";
        }
        /// <summary>
        /// 获取前几条语句
        /// </summary>
        /// <param name="fields">id,name</param>
        /// <param name="query">from table where 1=1</param>
        /// <param name="sort"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public override void GetSelectTop(StringBuilder sb, string fields, Action<StringBuilder> query,string sort, int top)
        {
            //string sql = string.Format("select {0} {1} {2} {3}", top == 0 ? "" : "top " + top, fields, query, sort);
            //string sql = "select " + (top == 0 ? "" : "top " + top) + fields + query + sort;
            //return sql;
            //sb.AppendFormat("select {0} {1} {2} {3}", top == 0 ? "" : "top " + top, fields, query, sort);
            //return;
            sb.Append("select ");
            if (top > 0)
            {
                sb.AppendFormat("top {0} ", top);
            }
            sb.Append(fields);
            query(sb);
            if (!string.IsNullOrEmpty(sort))
            {
                sb.Append(sort);
            }
        }
        #endregion

        #region 系统查询
        public override string GetAllTablesSql(string db)
        {
            return "select Lower(name),name from sysobjects where  type='u'";
        }
        public override string GetAllSPSql(string db)
        {
            return "select name,id from sysobjects where  type='P'";
        }
        #endregion

        #region 模版
        public override string SpParameFormat(string name, string type, bool output)
        {
            string str = "";
            if (!output)
            {
                str = "@{0} {1},";
            }
            else
            {
                str = "@{0} {1} output,";
            }
            return string.Format(str, name, type);
        }
        public override string KeyWordFormat(string value)
        {
            return string.Format("[{0}]", value);
        }
        //public override string FieldNameFormat(Attribute.FieldAttribute field)
        //{
        //    if(string.IsNullOrEmpty(field.MapingNameFormat))
        //    {
        //        return field.MapingName;
        //    }
        //    return field.MapingNameFormat;
        //}
        public override string TemplateGroupPage
        {
            get
            {
                string str = @"
--group分页
CREATE PROCEDURE [dbo].{name}
{parame}
--参数传入 @pageSize,@pageIndex
AS
set  nocount  on
declare @start nvarchar(20) 
declare @end nvarchar(20)
declare @pageCount INT

begin

    --获取记录数
	  select @count=count(0) from (select count(*) as a  {sql}) t
    if @count = 0
    return
    if @count = 0
        set @count = 1

    --取得分页总数
    set @pageCount=(@count+@pageSize-1)/@pageSize

    /**当前页大于总页数 取最后一页**/
    --if @pageIndex>@pageCount
        --set @pageIndex=@pageCount

	--计算开始结束的行号
	set @start = @pageSize*(@pageIndex-1)+1
	set @end = @start+@pageSize-1 
	SELECT * FROM (select {fields},ROW_NUMBER() OVER ( Order by {rowOver} ) AS RowNumber {sql}) T WHERE T.RowNumber BETWEEN @start AND @end 
end
";
                return str;
            }
        }

        public override string TemplatePage
        {
            get
            {
                string str = @"
--表分页
CREATE PROCEDURE [dbo].{name}
{parame}
--参数传入 @pageSize,@pageIndex
AS
set  nocount  on
declare @start nvarchar(20) 
declare @end nvarchar(20)
declare @pageCount INT

begin

    --获取记录数
	  select @count=count(0) {sql}
    if @count = 0
    return
    if @count = 0
        set @count = 1

    --取得分页总数
    set @pageCount=(@count+@pageSize-1)/@pageSize

    /**当前页大于总页数 取最后一页**/
    --if @pageIndex>@pageCount
        --set @pageIndex=@pageCount

	--计算开始结束的行号
	set @start = @pageSize*(@pageIndex-1)+1
	set @end = @start+@pageSize-1 
	SELECT * FROM (select {fields},ROW_NUMBER() OVER ( Order by {rowOver} ) AS RowNumber {sql}) T WHERE T.RowNumber BETWEEN @start AND @end order by RowNumber
end

";
                return str;
            }
        }

        public override string TemplateSp
        {
            get
            {
                string str = @"
CREATE PROCEDURE [dbo].{name}
{parame}
AS
set  nocount  on
	{sql}
";
                return str;
            }
        }
        public override string SqlFormat(string sql)
        {
            return sql;
        }
        #endregion

        #region 函数格式化
        public override string SubstringFormat(string field, int index, int length)
        {
            return string.Format(" SUBSTRING({0},{1},{2})", field, index+1, length);
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
        public virtual string StringNotContainsFormat(string field, string parName)
        {
            return string.Format("CHARINDEX({1},{0})<=0", field, parName);
        }
        public override string BetweenFormat(string field, string parName, string parName2)
        {
            return string.Format("{0} between {1} and {2}", field, parName, parName2);
        }

        public override string DateDiffFormat(string field, string format, string parName)
        {
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
        #endregion
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
        public override string GetParamName(string name, object index)
        {
            return string.Format("@{0}{1}", name, index);
        }
        public override string GetColumnUnionIndexScript(string tableName, string indexName, List<string> columns)
        {
            var script = string.Format("create index [{1}] on [{0}] ({2}) with (drop_existing = on)", tableName, indexName, string.Join(",", columns.ToArray()));
            return script;
        }
        public override string DateTimeFormat(string field, string format)
        {
            return string.Format("CONVERT(varchar(100), {0}, {1})", field, format);
        }
    }

    internal class MSSQL2000DBAdapter : MSSQLDBAdapter
    {
        public MSSQL2000DBAdapter(DbContext _dbContext)
            : base(_dbContext)
        {
        }
        public override DBType DBType
        {
            get { return DBType.MSSQL2000; }
        }
        /// <summary>
        /// 创建表脚本
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override void CreateTable(DbContext dbContext, List<Attribute.FieldInnerAttribute> fields, string tableName)
        {
            var helper = dbContext.DBHelper;
            var defaultValues = new List<string>();
            string script = string.Format("create table [{0}] (\r\n", tableName);
            List<string> list2 = new List<string>();
            foreach (Attribute.FieldInnerAttribute item in fields)
            {
                string nullStr = item.NotNull ? "NOT NULL" : "";
                string str = string.Format("[{0}] {1} {2} ", item.MapingName, item.ColumnType, nullStr);
                list2.Add(str);
                //生成默认值语句
                if (!string.IsNullOrEmpty(item.DefaultValue))
                {
                    string v = string.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}]", tableName, item.MapingName, item.DefaultValue);
                    defaultValues.Add(v);
                }
            }
            script += string.Join(",\r\n", list2.ToArray());
            //            script += string.Format(@" CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
            //(
            //	[Id] ASC
            //)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
            //", tableName);
            script += ") ON [PRIMARY]";
            //var list3 = GetIndexScript();
            //defaultValues.AddRange(list3);
            helper.Execute(script);
            foreach (string s in defaultValues)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    helper.Execute(s);
                }
            }
        }
     
    }
}
