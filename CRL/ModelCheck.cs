/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL
{
    public class ModelCheck
    {
        #region 检查表
        /// <summary>
        /// 检查索引
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="removeId"></param>
        public static void CheckIndexExists(Type type, AbsDBExtend db, bool removeId= false)
        {
            var list = GetIndexScript(type, db, removeId);
            foreach (var item in list)
            {
                try
                {
                    db.Execute(item);
                }
                catch (Exception ero)//出错,
                {
                    EventLog.Log(string.Format("创建索引失败:{0}\r\n{1}", ero.Message, item));
                }
            }
        }
        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="db"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static string CreateColumn(AbsDBExtend db, Attribute.FieldInnerAttribute item)
        {
            var dbAdapter = db._DBAdapter;
            string result = "";
            if (string.IsNullOrEmpty(item.ColumnType))
            {
                throw new Exception("ColumnType is null");
            }
            string str = dbAdapter.GetCreateColumnScript(item);
            string indexScript = "";
            if (item.FieldIndexType != Attribute.FieldIndexType.无)
            {
                indexScript = dbAdapter.GetColumnIndexScript(item);
            }
            try
            {
                db.Execute(str);
                if (!string.IsNullOrEmpty(indexScript))
                {
                    db.Execute(indexScript);
                }
                result = string.Format("创建字段:{0} {1} {2}\r\n", item.TableName, item.MemberName, item.PropertyType);
                var model = System.Activator.CreateInstance(item.ModelType) as IModel;
                try
                {
                    model.OnColumnCreated(item.MemberName);
                }
                catch (Exception ero)
                {
                    result = string.Format("添加字段:{0} {1},升级数据时发生错误:{2}\r\n", item.TableName, item.MemberName, ero.Message);
                }
                EventLog.Log(result, "", false);
            }
            catch (Exception ero)
            {
                //EventLog.Log("创建字段时发生错误:" + ero.Message);
                result = string.Format("创建字段:{0} {1}发生错误:{2}\r\n", item.TableName, item.MemberName, ero.Message);
            }
            return result;
        }
        /// <summary>
        /// 检查对应的字段是否存在,不存在则创建
        /// </summary>
        /// <param name="db"></param>
        internal static string CheckColumnExists(Type type, AbsDBExtend db)
        {
            string result = "";
            var dbAdapter = db._DBAdapter;
            List<Attribute.FieldInnerAttribute> columns = GetColumns(type, db);
            string tableName = TypeCache.GetTableName(type, db.dbContext);
            foreach (Attribute.FieldInnerAttribute item in columns)
            {
                var sb = new StringBuilder();
                //string sql = dbAdapter.GetSelectTop(item.MapingName, "from " + dbAdapter.KeyWordFormat(tableName), "", 1);
                dbAdapter.GetSelectTop(sb,item.MapingName, b=>
                {
                    b.Append(" from " + dbAdapter.KeyWordFormat(tableName));
                }, "", 1);
                var sql = sb.ToString();
                try
                {
                    db.Execute(sql);
                }
                catch(Exception ero)//出错,按没有字段算
                {
                    result += CreateColumn(db, item);

                }
            }
            return result;
        }
        internal static void SetColumnDbType(DBAdapter.DBAdapterBase dbAdapter, Attribute.FieldInnerAttribute info)
        {
            //if (info.FieldType != Attribute.FieldType.数据库字段)
            //{
            //    return;
            //}
            string defaultValue;
            Type propertyType = info.PropertyType;
            var columnType = dbAdapter.GetColumnType(info, out defaultValue);
            info.ColumnType = columnType;
            info.DefaultValue = defaultValue;
            if (info.ColumnType.Contains("{0}"))
            {
                throw new Exception(string.Format("属性:{0} 需要指定长度 ColumnType:{1}", info.MemberName, info.ColumnType));
            }
        }
        /// <summary>
        /// 获取列
        /// </summary>
        /// <returns></returns>
        public static List<Attribute.FieldInnerAttribute> GetColumns(Type type, AbsDBExtend db)
        {
            //var dbAdapter = Base.CurrentDBAdapter;
            //Type type = this.GetType();
            string tableName = TypeCache.GetTableName(type, db.dbContext);
            var typeArry = TypeCache.GetProperties(type, true).Values;
            var columns = new List<CRL.Attribute.FieldInnerAttribute>();
            foreach (var info in typeArry)
            {
                //if (info.FieldType != Attribute.FieldType.数据库字段)
                //    continue;
                SetColumnDbType(db._DBAdapter, info);
                columns.Add(info);
            }
            return columns;
        }
        internal static List<string> GetIndexScript(Type type, AbsDBExtend db, bool removeId)
        {
            var dbAdapter = db._DBAdapter;
            List<string> list2 = new List<string>();
            List<Attribute.FieldInnerAttribute> columns = GetColumns(type, db);
            foreach (Attribute.FieldInnerAttribute item in columns)
            {
                if (item.FieldIndexType != Attribute.FieldIndexType.无 || item.IsPrimaryKey)
                {
                    //string indexScript = string.Format("CREATE {2} NONCLUSTERED INDEX  IX_INDEX_{0}_{1}  ON dbo.[{0}]({1})", tableName, item.Name, item.FieldIndexType == Attribute.FieldIndexType.非聚集唯一 ? "UNIQUE" : "");
                    //主建也检查索引
                    if (removeId && item.IsPrimaryKey)
                    {
                        continue;
                    }
                    string indexScript = dbAdapter.GetColumnIndexScript(item);
                    if (!string.IsNullOrEmpty(indexScript))
                    {
                        list2.Add(indexScript);
                    }
                }
            }
            var obj = System.Activator.CreateInstance(type) as IModel;
            if (obj == null)
            {
                return list2;
            }
            var indexDic = AbsPropertyBuilder.indexs;
            var table = TypeCache.GetTable(type);
            var a = indexDic.TryGetValue(type, out var tableIndex);
            if (a)
            {
                foreach (var item in tableIndex.Index)
                {
                    var field = new Attribute.FieldInnerAttribute() { TableName = table.TableName, MapingName = item.Key, FieldIndexType = item.Value};
                    list2.Add(dbAdapter.GetColumnIndexScript(field));
                }
                foreach (var item in tableIndex.UnionIndex)
                {
                    list2.Add(dbAdapter.GetColumnUnionIndexScript(table.TableName, item.Key, item.Value.Fields, item.Value.FieldIndexType));
                }
            }
            return list2;
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static string CreateTable(Type type, AbsDBExtend db)
        {
            string msg;
            CreateTable(type, db, out msg);
            return msg;
        }
        /// <summary>
        /// 创建表
        /// 会检查表是否存在,如果存在则检查字段
        /// 创建失败则抛出异常
        /// 表存在返回失败
        /// </summary>
        /// <param name="type"></param>
        /// <param name="db"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CreateTable(Type type, AbsDBExtend db, out string message)
        {
            var dbAdapter = db._DBAdapter;
            message = "";
            //TypeCache.SetDBAdapterCache(GetType(),dbAdapter);
            string tableName = TypeCache.GetTableName(type, db.dbContext);
            var sb = new StringBuilder();
            dbAdapter.GetSelectTop(sb, "0", b =>
             {
                 b.Append(" from " + dbAdapter.KeyWordFormat(tableName));
             }, "", 1);
            var sql = sb.ToString();
            bool needCreate = false;
            try
            {
                //检查表是否存在
                db.Execute(sql);
                //return false;// 返回结果时不会检查字段
            }
            catch
            {
                needCreate = true;
            }
            if (needCreate)
            {
                List<string> list = new List<string>();
                try
                {
                    List<Attribute.FieldInnerAttribute> columns = GetColumns(type, db);
                    dbAdapter.CreateTable(db.dbContext, columns, tableName);
                    message = string.Format("创建表:{0}\r\n", tableName);
                    CheckIndexExists(type, db);
                    //return true;
                }
                catch (Exception ero)
                {
                    message = "创建表时发生错误 类型{0} {1}\r\n";
                    message = string.Format(message, type, ero.Message);
                    throw new Exception(message);
                    //return false;
                }
                //EventLog.Log(message, "", false);
            }
            else
            {
                message = CheckColumnExists(type, db);
            }
            return true;
        }

        #endregion
    }
}
