/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CRL
{
    public class TypeCache
    {
        //static object lockObj = new object();
        internal static ConcurrentDictionary<Type, Attribute.TableInnerAttribute> typeCache = new ConcurrentDictionary<Type, Attribute.TableInnerAttribute>();
        /// <summary>
        /// 对象类型缓存
        /// 类型+key
        /// </summary>
        static ConcurrentDictionary<string, string> ModelKeyCache = new ConcurrentDictionary<string, string>();

        #region modelKey
        public static bool GetModelKeyCache(Type type, string dataBase, out string key)
        {
            var typeKey = string.Format("{0}|{1}", type, dataBase);
            return ModelKeyCache.TryGetValue(typeKey, out key);
        }
        public static void SetModelKeyCache(Type type, string dataBase,  string key)
        {
            var typeKey = string.Format("{0}|{1}", type, dataBase);
            ModelKeyCache[typeKey] = key;
        }
        public static void RemoveModelKeyCache(Type type, string dataBase)
        {
            var typeKey = string.Format("{0}|{1}", type, dataBase);
            string val;
            ModelKeyCache.TryRemove(typeKey, out val);
        }
        #endregion
        /// <summary>
        /// 根据类型返回表名
        /// 如果设置了分表,返回分表名
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static string GetTableName(Type type, DbContext dbContext)
        {
            var tableName = GetTable(type).TableName;
            return GetTableName(tableName, dbContext);
        }
        public static string GetTableName(string tableName, DbContext dbContext)
        {
            if (dbContext != null && dbContext.UseSharding)
            {
                if (dbContext.DBLocation.ShardingLocation != null)
                {//没有设置定位,则找默认库

                    var location = dbContext.DBLocation.ShardingLocation;
                    tableName = location.TablePartName;
                }
            }
            return tableName;
        }
        /// <summary>
        /// 获取表属性,如果要获取表名,调用GetTableName方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Attribute.TableInnerAttribute GetTable(Type type)
        {
            Attribute.TableInnerAttribute table;
            var b = typeCache.TryGetValue(type, out table);
            if (b)
            {
                if (table.Fields.Count > 0)
                {
                    return table;
                }
            }
            object[] objAttrs = type.GetCustomAttributes(typeof(Attribute.TableAttribute), true);
            Attribute.TableInnerAttribute des;
            if (objAttrs == null || objAttrs.Length == 0)
            {
                des = new Attribute.TableInnerAttribute() { TableName = type.Name };
            }
            else
            {
                des = (objAttrs[0] as Attribute.TableAttribute).ToType<Attribute.TableInnerAttribute>();
            }
            des.Type = type;
            des.Fields = new List<Attribute.FieldInnerAttribute>();
            if (!typeCache.ContainsKey(type))
            {
                typeCache.TryAdd(type, des);
            }
            if (string.IsNullOrEmpty(des.TableName))
            {
                des.TableName = type.Name;
            }
            //des.TableNameFormat = string.Format("[{0}]", des.TableName);
            InitProperties(des);
            return des;
        }
        /// <summary>
        /// 获取数据库字段
        /// </summary>
        /// <param name="type"></param>
        /// <param name="onlyField"></param>
        /// <returns></returns>
        public static IgnoreCaseDictionary<Attribute.FieldInnerAttribute> GetProperties(Type type, bool onlyField=true)
        {
            var table = GetTable(type);
            return table.FieldsDic;
        }
        static void InitProperties(Attribute.TableInnerAttribute table)
        {
            if (table.Fields.Count > 0)
            {
                return;
            }
            var type = table.Type;
            List<Attribute.FieldInnerAttribute> list = new List<CRL.Attribute.FieldInnerAttribute>();
            var fieldDic = new IgnoreCaseDictionary<Attribute.FieldInnerAttribute>();
            //string fieldPat = @"^([A-Z][a-z|\d]+)+$";
            int n = 0;
            Attribute.FieldInnerAttribute keyField = null;
            #region 读取
            var typeArry = table.Type.GetProperties().ToList();
            //移除重复的
            var dic = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo info in typeArry)
            {
                if (!dic.ContainsKey(info.Name))
                {
                    dic.Add(info.Name, info);
                }
            }
            foreach (PropertyInfo info in dic.Values)
            {
                //if (!System.Text.RegularExpressions.Regex.IsMatch(info.Name, fieldPat))
                //{
                //    throw new CRLException(string.Format("属性名:{0} 不符合规则:{1}", info.Name, fieldPat));
                //}
                //排除没有SET方法的属性
                if (info.GetSetMethod() == null)
                {
                    continue;
                }
                Type propertyType = info.PropertyType;
                var f = new CRL.Attribute.FieldInnerAttribute();
                //排除集合类型
                if (propertyType.FullName.IndexOf("System.Collections") > -1)
                {
                    continue;
                }

                object[] attrs = info.GetCustomAttributes(typeof(Attribute.FieldAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    var atr = attrs[0] as Attribute.FieldAttribute;
                    atr.MemberName = info.Name;
                    f = atr.ToType<Attribute.FieldInnerAttribute>();
                }
                f.SetPropertyInfo(info);
                f.PropertyType = propertyType;
                f.MemberName = info.Name;
                f.TableName = table.TableName;
                f.ModelType = table.Type;

                //排除不映射字段
                if (!f.MapingField)
                {
                    continue;
                }
                if (propertyType == typeof(System.String))
                {
                    if (f.Length == 0)
                        f.Length = 30;
                }
                if (f.IsPrimaryKey)//保存主键
                {
                    table.PrimaryKey = f;
                    f.FieldIndexType = Attribute.FieldIndexType.非聚集唯一;
                    keyField = f;
                    n += 1;
                }

                if (!fieldDic.ContainsKey(f.MemberName))
                {
                    fieldDic.Add(f.MemberName, f);
                }

                list.Add(f);
            }
            if (n == 0)
            {
                //throw new CRLException(string.Format("对象{0}未设置任何主键", type.Name));
            }
            else if (n > 1)
            {
                throw new CRLException(string.Format("对象{0}设置的主键字段太多 {1}", type.Name, n));
            }
            #endregion
            //主键排前面
            if (keyField != null)
            {
                list.Remove(keyField);
                list.Insert(0, keyField);
            }
            table.Fields = list;
            table.FieldsDic = fieldDic;
        }
    }
}
