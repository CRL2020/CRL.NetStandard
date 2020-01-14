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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace CRL
{
    #region obj
        enum IndexType
        {
            Normal,
            Unique
        }
        class TableIndex
        {
            public Type Type;
            /// <summary>
            /// 单字段索引
            /// </summary>
            public ConcurrentDictionary<string, IndexType> Index = new ConcurrentDictionary<string, IndexType>();
            /// <summary>
            /// 联合索引
            /// </summary>
            public ConcurrentDictionary<string, UnionIndexItem> UnionIndex = new ConcurrentDictionary<string, UnionIndexItem>();
        }
        internal class UnionIndexItem
        {
            public List<string> Fields = new List<string>();
            public override string ToString()
            {
                return string.Join("_", Fields.OrderBy(b => b));
            }
        }
    #endregion
    public abstract class AbsPropertyBuilder
    {

        internal TableIndex indexs = new TableIndex();
    }
    /// <summary>
    /// 属性构造
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyBuilder<T>: AbsPropertyBuilder
    {
 
        /// <summary>
        /// 设置非聚集索引
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public PropertyBuilder<T> AsIndex<Tresult>(Expression<Func<T, Tresult>> member)
        {
            var m = member.Body as MemberExpression;
            if (m == null)
            {
                throw new CRLException("应为MemberExpression" + member);
            }
            var name = m.Member.Name;
            if (indexs.Index.ContainsKey(name))
            {
                return this;
            }
            indexs.Index.TryAdd(name, IndexType.Normal);
            return this;
        }
        /// <summary>
        /// 设置非聚集唯一索引
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public PropertyBuilder<T> AsUniqueIndex<Tresult>(Expression<Func<T, Tresult>> member)
        {
            var m = member.Body as MemberExpression;
            if (m == null)
            {
                throw new CRLException("应为MemberExpression" + member);
            }
            var name = m.Member.Name;
            if (indexs.Index.ContainsKey(name))
            {
                return this;
            }
            indexs.Index.TryAdd(name, IndexType.Unique);
            return this;
        }
        /// <summary>
        /// 设置联合索引
        /// </summary>
        /// <typeparam name="Tresult"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public PropertyBuilder<T> AsUnionIndex<Tresult>(string indexName, Expression<Func<T, Tresult>> expression)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new Exception("索引名称是必须的 indexName");
            }
            var type = typeof(T);

            var newExpression = expression.Body as NewExpression;
            if (newExpression ==null)
            {
                throw new Exception("必须为匿名表达式");
            }
            indexName = string.Format("{0}_{1}", typeof(T).Name, indexName);
            if (indexs.UnionIndex.ContainsKey(indexName))
            {
                return this;
            }
            var unionIndexItem = new UnionIndexItem();
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
                if (unionIndexItem.Fields.Contains(m.Member.Name))
                {
                    throw new Exception("联合索引 " + indexName + " 中已包括字段" + m.Member.Name);
                }
                unionIndexItem.Fields.Add(m.Member.Name);
            }
            indexs.UnionIndex.TryAdd(indexName, unionIndexItem);

            return this;
        }
    }
}
