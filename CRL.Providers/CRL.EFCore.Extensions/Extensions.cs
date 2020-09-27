using CRL.DBAccess;
using CRL.LambdaQuery;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using CRL.MySql;
using System.Linq;
using System.Reflection;

namespace CRL.EFCore.Extensions
{
    public static partial class Extensions
    {
        static Extensions()
        {
            SettingConfig.StringFieldLength = 50;
            var builder = DBConfigRegister.GetInstance();
            builder.UseMySql();
        }

        static DBHelper getDBHelper(DbContext dbContext)
        {
            var dbConnection = dbContext.Database.GetDbConnection();
            var builder = DBConfigRegister.GetInstance();
            builder.RegisterDBAccessBuild(dbLocation =>
            {
                return new DBAccessBuild(DBType.MSSQL, dbConnection);
            });
            var dbConnectionTypeName = dbConnection.GetType().Name;
            DBType dBType = DBType.MSSQL;
            switch (dbConnectionTypeName)
            {
                case "SqlConnection":
                    dBType = DBType.MSSQL;
                    break;
                case "MySqlConnection":
                    dBType = DBType.MYSQL;
                    break;
                case "OracleConnection":
                    dBType = DBType.ORACLE;
                    break;
            }
            var dBAccessBuild = new DBAccessBuild(dBType, dbConnection);
            var helper = DBConfigRegister.GetDBHelper(dBAccessBuild);
            return helper;
        }
        public static IAbsDBExtend GetDBExtend(this DbContext dbContext)
        {
            var helper = getDBHelper(dbContext);
            var dbContext2 = new DbContextInner(helper, new DBLocation() {  });
            return DBExtendFactory.CreateDBExtend(dbContext2);
        }
        public static ILambdaQuery<T> GetLambdaQuery<T>(this DbContext dbContext) where T : class
        {
            var helper = getDBHelper(dbContext);
            var db = new DbContextInner(helper, new DBLocation());
            var query = LambdaQueryFactory.CreateLambdaQuery<T>(db);
            return query;
        }
        public static void CreateTable<T>(this DbContext dbContext)
        {
            var dbExtend = GetDBExtend(dbContext);
            ModelCheck.CreateTable(typeof(T), dbExtend as AbsDBExtend, out var message);
            Console.WriteLine(message);
        }
        public static void ConfigEntityTypeBuilder<T>(this EntityTypeBuilder<T> builder) where T : class
        {
            var tableInfo = TypeCache.GetTable(typeof(T));
            tableInfo.TableName = typeof(T).Name;
            var table = builder.Metadata.GetAnnotations().Find(b => b.Name == "Relational:TableName");
            if (table != null)
            {
                tableInfo.TableName = table.Value.ToString();
            }
            var pros = builder.Metadata.GetProperties();
            var unionIndexs = new List<Microsoft.EntityFrameworkCore.Metadata.Internal.Index>();
            foreach (Property item in pros)
            {
                var a = tableInfo.FieldsDic.TryGetValue(item.Name, out var f);
                if (!a)
                {
                    continue;
                }
                f.MapingName = item.GetColumnName();
                f.ColumnType = item.GetColumnType();
                if (item.GetMaxLength() > 0)
                {
                    f.Length = item.GetMaxLength().Value;
                }
                if (item.IsPrimaryKey())
                {
                    var idGenerate = item.PropertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>();
                    f.KeepIdentity = true;
                    if (idGenerate != null)
                    {
                        f.KeepIdentity = idGenerate.DatabaseGeneratedOption == System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None;
                    }
                    f.IsPrimaryKey = true;
                    tableInfo.PrimaryKey = f;
                }
                if (item.IsIndex())
                {
                    var index = item.Indexes.First();
                    var isUnique = index.IsUnique;
                    if (index.Properties.Count == 1)
                    {
                        f.FieldIndexType = isUnique ? Attribute.FieldIndexType.非聚集唯一 : Attribute.FieldIndexType.非聚集;
                    }
                    else
                    {
                        unionIndexs.Add(index);
                    }
                }
            }
            foreach (var index in unionIndexs)//unionIndexs
            {
                var isUnique = index.IsUnique;
                var fields = new List<string>();
                foreach (var p in index.Properties)
                {
                    tableInfo.FieldsDic.TryGetValue(p.Name, out var f);
                    fields.Add(f.MapingName);
                }
                var indexName = string.Join("_", index.Properties.Select(b => b.Name));
                AbsPropertyBuilder.SetUnionIndex<T>(indexName, fields, isUnique ? Attribute.FieldIndexType.非聚集唯一 : Attribute.FieldIndexType.非聚集);
            }
        }
    }
}
