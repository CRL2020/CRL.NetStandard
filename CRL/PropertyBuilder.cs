/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.Attribute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace CRL
{
    #region obj
    class TableIndex
    {
        public Type Type;
        /// <summary>
        /// 单字段索引
        /// </summary>
        public ConcurrentDictionary<string, FieldIndexType> Index = new ConcurrentDictionary<string, FieldIndexType>();
        /// <summary>
        /// 联合索引
        /// </summary>
        public ConcurrentDictionary<string, UnionIndexItem> UnionIndex = new ConcurrentDictionary<string, UnionIndexItem>();
    }
    internal class UnionIndexItem
    {
        public List<string> Fields = new List<string>();

        public Attribute.FieldIndexType FieldIndexType;
        public override string ToString()
        {
            return string.Join("_", Fields.OrderBy(b => b));
        }
    }
    #endregion
    public abstract class AbsPropertyBuilder
    {

        internal static Dictionary<Type, TableIndex> indexs = new Dictionary<Type, TableIndex>();
        internal static TableIndex getTableIndex<T>()
        {
            var a = indexs.TryGetValue(typeof(T), out var tableIndex);
            if (a)
            {
                return tableIndex;
            }
            tableIndex = new TableIndex();
            indexs.Add(typeof(T), tableIndex);
            return tableIndex;
        }
        public static void SetUnionIndex<T>(string indexName, List<string> fields, Attribute.FieldIndexType fieldIndexType)
        {
            indexName = string.Format("{0}_{1}", typeof(T).Name, indexName);
            var indexs = getTableIndex<T>();
            if (indexs.UnionIndex.ContainsKey(indexName))
            {
                return;
            }
            var unionIndexItem = new UnionIndexItem() { FieldIndexType = fieldIndexType };
            for (int i = 0; i < fields.Count(); i++)
            {
                var field = fields[i];
                if (unionIndexItem.Fields.Contains(field))
                {
                    throw new Exception("联合索引 " + indexName + " 中已包括字段" + field);
                }
                unionIndexItem.Fields.Add(field);
            }
            indexs.UnionIndex.TryAdd(indexName, unionIndexItem);
        }
    }
    /// <summary>
    /// 属性构造
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyBuilder<T> : AbsPropertyBuilder
    {

        /// <summary>
        /// 设置非聚集索引
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public PropertyBuilder<T> AsIndex<Tresult>(Expression<Func<T, Tresult>> member)
        {
            return AsUniqueIndex(member, FieldIndexType.非聚集);
        }
        /// <summary>
        /// 设置非聚集唯一索引
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public PropertyBuilder<T> AsUniqueIndex<Tresult>(Expression<Func<T, Tresult>> member)
        {
            return AsUniqueIndex(member, FieldIndexType.非聚集唯一);
        }

        PropertyBuilder<T> AsUniqueIndex<Tresult>(Expression<Func<T, Tresult>> member, FieldIndexType indexType)
        {
            var m = member.Body as MemberExpression;
            if (m == null)
            {
                throw new Exception("应为MemberExpression" + member);
            }
            var name = m.Member.Name;
            var indexs = getTableIndex<T>();
            if (indexs.Index.ContainsKey(name))
            {
                return this;
            }
            indexs.Index.TryAdd(name, indexType);
            return this;
        }

        /// <summary>
        /// 设置联合索引
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public PropertyBuilder<T> AsUnionIndex<Tresult>(string indexName, Expression<Func<T, Tresult>> expression, Attribute.FieldIndexType fieldIndexType = Attribute.FieldIndexType.非聚集)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new Exception("索引名称是必须的 indexName");
            }
            var type = typeof(T);

            var newExpression = expression.Body as NewExpression;
            if (newExpression == null)
            {
                throw new Exception("必须为匿名表达式");
            }
            indexName = string.Format("{0}_{1}", typeof(T).Name, indexName);
            var indexs = getTableIndex<T>();
            if (indexs.UnionIndex.ContainsKey(indexName))
            {
                return this;
            }

            var fields = new List<string>();
            var table = TypeCache.GetTable(typeof(T));
            for (int i = 0; i < newExpression.Arguments.Count(); i++)
            {
                var item = newExpression.Arguments[i];
                MemberExpression m;
                if (item is UnaryExpression)
                {
                    var uExp = item as UnaryExpression;
                    m = uExp.Operand as MemberExpression;
                }
                else
                {
                    m = item as MemberExpression;
                }
                if (m == null)
                {
                    throw new Exception(item + "不为MemberExpression");
                }
                table.FieldsDic.TryGetValue(m.Member.Name, out var f);
                fields.Add(f.MapingName);
            }
            SetUnionIndex<T>(indexName, fields, fieldIndexType);
            return this;
        }
    }
}
