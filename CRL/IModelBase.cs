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
using System.Reflection;
using System.Dynamic;
using CRL.Set;
using System.Runtime.Serialization;
using CRL.Core;
using System.Linq.Expressions;

namespace CRL
{
    /// <summary>
    /// 基类,包含Id, AddTime字段
    /// </summary>
    public abstract class IModelBase : IModel
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Attribute.FieldInner(IsPrimaryKey = true)]
        public int Id
        {
            get;
            set;
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime
        {
            get; set;
        } = DateTime.Now;

    }
    /// <summary>
    /// 基类,不包含任何字段
    /// 如果有自定义主键名对象,请继承此类型
    /// </summary>
    //[Attribute.ModelProxy]
    public abstract class IModel /*: ContextBoundObject*/
    {
        /// <summary>
        /// 序列化为JSON
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return StringHelper.SerializerToJson(this);
        }
        internal bool FromCache;
        #region 外关联

        /// <summary>
        /// 创建一对多关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected DbEntities<TJoin> IncludeMany<T, TJoin>(Expression<Func<T, TJoin,bool>> expression) where T : IModel, new()
            where TJoin : IModel, new()
        {
            var type = typeof(TJoin);
            var name = $"ents_{type}_{GetpPrimaryKeyValue()}";
            var a = DbContext._DbSets.TryGetValue(name, out IDbSet set2);
            if (a)
            {
                return set2 as DbEntities<TJoin>;
            }
            if (FromCache)//当是缓存时
            {
                throw new Exception("缓存对象不能调用");
            }
            var _relationExp = CreaterelationExp(expression);

            var set = new DbEntities<TJoin>(name, DbContext, _relationExp);
            //DbContext._DbSets.Add(name, set);
            return set;
        }
        Expression<Func<TJoin, bool>> CreaterelationExp<T, TJoin>(Expression<Func<T, TJoin, bool>> expression)
        {
            var type = typeof(TJoin);
            var be = (BinaryExpression)expression.Body;
            var left = (MemberExpression)be.Left;
            var right = (MemberExpression)be.Right;
            MemberExpression relationExpression;
            object relationValue;
            if (left.Member.DeclaringType == typeof(T))
            {
                relationExpression = right;
                relationValue = TypeCache.GetTable(left.Member.DeclaringType).FieldsDic[left.Member.Name].GetValue(this);
            }
            else
            {
                relationExpression = left;
                relationValue = TypeCache.GetTable(right.Member.DeclaringType).FieldsDic[right.Member.Name].GetValue(this);
            }

            var parameterExpression = Expression.Parameter(type, "b");

            var constant = Expression.Constant(relationValue);
            var body = Expression.Equal(relationExpression, constant);
            var _relationExp = Expression.Lambda<Func<TJoin, bool>>(body, parameterExpression);
            return _relationExp;
        }
        /// <summary>
        /// 创建一对一关联
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TJoin"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected DbEntity<TJoin> IncludeOne<T, TJoin>(Expression<Func<T, TJoin, bool>> expression) where T : IModel, new()
            where TJoin : IModel, new()
        {
            var _relationExp = CreaterelationExp(expression);
            return new DbEntity<TJoin>(DbContext, _relationExp);
        }
        #endregion
        #region 方法重写
        /// <summary>
        /// ToJson
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + GetType().FullName + "] " + ToJson();
        }
        /// <summary>
        /// 数据校验方法,可重写
        /// </summary>
        /// <returns></returns>
        public virtual string CheckData()
        {
            return "";
        }
        /// <summary>
        /// 默认属性构造
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual AbsPropertyBuilder BuildIndex()
        {
            return null;
        }

        /// <summary>
        /// 当列创建时,可重写
        /// 可处理在添加字段后数据的升级
        /// </summary>
        /// <param name="fieldName"></param>
        protected internal virtual void OnColumnCreated(string fieldName)
        {
        }
        /// <summary>
        /// 创建表时的初始数据,可重写
        /// </summary>
        /// <returns></returns>
        protected internal virtual System.Collections.IList GetInitData()
        {
            return null;
        }
        
        #endregion
        /// <summary>
        /// 手动跟踪对象状态,使更新时能识别
        /// </summary>
        public void BeginTracking()
        {
            OriginClone = Clone();
            Changes = new ParameCollection();
        }

        //internal string ManageName;
        internal DbContext DbContext;
        #region 索引

        [System.Xml.Serialization.XmlIgnore]
        [NonSerialized]
        Dictionary<string, object> Datas = null;


        ///// <summary>
        ///// 获取关联查询的值
        ///// 不分区大小写
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //[Attribute.Field(MapingField = false)]
        //public dynamic this[string key]
        //{
        //    get
        //    {
        //        dynamic obj = null;
        //        Datas = Datas ?? new Dictionary<string, object>();
        //        var a = Datas.TryGetValue(key.ToLower(), out obj);
        //        if (!a)
        //        {
        //            throw new CRLException(string.Format("对象:{0}不存在索引值:{1}", GetType(), key));
        //        }
        //        return obj;
        //    }
        //    set
        //    {
        //        Datas = Datas ?? new Dictionary<string, object>();
        //        Datas[key.ToLower()] = value;
        //    }
        //}
        /// <summary>
        /// 获以索引的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public dynamic GetIndexData(string key)
        {
            dynamic obj = null;
            Datas = Datas ?? new Dictionary<string, object>();
            var a = Datas.TryGetValue(key.ToLower(), out obj);
            if (!a)
            {
                throw new Exception(string.Format("对象:{0}不存在索引值:{1}", GetType(), key));
            }
            return obj;
        }
        /// <summary>
        /// 设置索引值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SetIndexData(string key, object value)
        {
            Datas = Datas ?? new Dictionary<string, object>();
            Datas[key.ToLower()] = value;
        }
        #endregion


        #region 更新值判断
        /// <summary>
        /// 存放原始克隆
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        [NonSerialized]
        internal object OriginClone = null;
        internal object GetOriginClone()
        {
            return OriginClone;
        }
        internal void SetOriginClone()
        {
            OriginClone = null;
            OriginClone = Clone();
        }

        //[System.Xml.Serialization.XmlIgnore]
        //[NonSerialized]
        //internal bool BoundChange = true;

        /// <summary>
        /// 存储被更改的属性
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        internal Dictionary<string, object> Changes = null;

        /// <summary>
        /// 表示值被更改了
        /// 当更新后,将被清空
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal void SetChanges(string name,object value)
        {
            //if (!BoundChange)
            //    return;
            if (name.ToLower() == "boundchange")
                return;
            Changes = Changes ?? new ParameCollection();
            Changes[name] = value;
        }
        Dictionary<string, object> GetChanges()
        {
            Changes = Changes ?? new Dictionary<string, object>();
            return Changes;
        }
        /// <summary>
        /// 清空Changes并重新Clone源对象
        /// </summary>
        internal void CleanChanges()
        {
            Changes.Clear();
            if (SettingConfig.UsePropertyChange)
            {
                return;
            }
            OriginClone = Clone();
        }
        /// <summary>
        /// 获取被修改的字段
        /// </summary>
        /// <returns></returns>
        public ParameCollection GetUpdateField(DBAdapter.DBAdapterBase dBAdapterBase= null, bool check = true)
        {
            var c = new ParameCollection();
            var fields = TypeCache.GetProperties(GetType(), true);
            if (this.GetChanges().Count > 0)//按手动指定更改
            {
                foreach (var item in this.GetChanges())
                {
                    var key = item.Key.Replace("$", "");
                    var f = fields[key];
                    if (f == null)
                        continue;
                    if (f.IsPrimaryKey)
                        continue;
                    var value = item.Value;
                    //如果表示值为被追加 名称为$name
                    //使用Cumulation扩展方法后按此处理
                    if (key != item.Key)//按$name=name+'123123'
                    {
                        if (dBAdapterBase != null)
                        {
                            value = dBAdapterBase.GetFieldConcat(dBAdapterBase.KeyWordFormat(f.MapingName), value, f.PropertyType);
                        }
                    }
                    c[item.Key] = value;
                }
                return c;
            }
            //按对象对比
            var origin = this.OriginClone;
            if (origin == null && check)
            {
                throw new Exception("_originClone为空,请确认此对象是由查询创建");
            }
            foreach (var f in fields.Values)
            {
                if (f.IsPrimaryKey)
                    continue;
                var originValue = f.GetValue(origin);
                var currentValue = f.GetValue(this);
                if (!Equals(originValue, currentValue))
                {
                    c.Add(f.MapingName, currentValue);
                }
            }
            return c;
        }
        /// <summary>
        /// 对象是否被修改
        /// </summary>
        public bool IsModified()
        {
            return GetUpdateField(null,false).Count > 0;
        }
        /// <summary>
        /// 清除通过Change方法设置的变更字典
        /// </summary>
        public void ClearChangedFiled()
        {
            Changes.Clear();
        }
        #endregion

        /// <summary>
        /// 创建当前对象的浅表副本
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        internal string GetModelKey()
        {
            var type = GetType();
            var tab = TypeCache.GetTable(type);
            var modelKey = string.Format("{0}_{1}", type, tab.PrimaryKey.GetValue(this));
            return modelKey;
        }
        internal object GetpPrimaryKeyValue()
        {
            var primaryKey = TypeCache.GetTable(GetType()).PrimaryKey;
            var keyValue = primaryKey.GetValue(this);
            return keyValue;
        }
        #region 动态字典,效果同索引
        //private Dynamic.DynamicViewDataDictionary _dynamicViewDataDictionary;

        /// <summary>
        /// 动态Bag,可用此取索引值
        /// 不区分大小写
        /// </summary>
        public dynamic GetBag()
        {
            return new Dynamic.DynamicViewDataDictionary(Datas);
        }

        #endregion

    }
}
