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
        internal Dictionary<Type, object> _DbSets;
        /// <summary>
        /// 创建一对多关联
        /// </summary>
        /// <typeparam name="T">关联对象</typeparam>
        /// <param name="member"></param>
        /// <param name="key">member=key</param>
        /// <param name="expression">补充条件</param>
        /// <returns></returns>
        protected DbSet<T> GetDbSet<T>(System.Linq.Expressions.Expression<Func<T, object>> member, object key, System.Linq.Expressions.Expression<Func<T, bool>> expression = null) where T : IModel, new()
        {
            if (FromCache)//当是缓存时
            {
                return new DbSet<T>(member, key);
            }
            var type = typeof(T);
            if (_DbSets == null)
            {
                _DbSets = new Dictionary<Type, object>();
            }
            if (_DbSets.ContainsKey(type))//当expression不为空时,会产生冲突
            {
                return _DbSets[type] as DbSet<T>;
            }
            var set = new DbSet<T>(member, key);
            _DbSets.Add(type, set);
            return set;
        }
        /// <summary>
        /// 创建一对一关联
        /// 在循环内调用会多次调用数据库
        /// </summary>
        /// <typeparam name="T">关联对象</typeparam>
        /// <param name="member"></param>
        /// <param name="key">member=key</param>
        /// <param name="expression">补充条件</param>
        /// <returns></returns>
        protected EntityRelation<T> GetEntityRelation<T>(System.Linq.Expressions.Expression<Func<T, object>> member, object key, System.Linq.Expressions.Expression<Func<T, bool>> expression = null) where T : IModel, new()
        {
            return new EntityRelation<T>(member, key, expression);
        }
        //public void SaveChanges()
        //{
        //    if (_DbSets == null)
        //    {
        //        return;
        //    }
        //    foreach(var kv in _DbSets)
        //    {
        //        var set = kv.Value as IDbSet;
        //        set.Save();
        //    }
        //}
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
        #region redis
        internal bool UseRedis()
        {
            return !string.IsNullOrEmpty(GetRedisHashKey());
        }
        /// <summary>
        /// 重写获到HashId
        /// </summary>
        /// <returns></returns>
        public virtual string GetRedisHashId()
        {
            return "";
        }
        /// <summary>
        /// 重写获取Key
        /// </summary>
        /// <returns></returns>
        public virtual string GetRedisHashKey()
        {
            return "";
        }
        #endregion
        #endregion
        /// <summary>
        /// 手动跟踪对象状态,使更新时能识别
        /// </summary>
        public void BeginTracking()
        {
            OriginClone = Clone();
            Changes = new ParameCollection();
        }

        internal string ManageName;
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
                throw new CRLException(string.Format("对象:{0}不存在索引值:{1}", GetType(), key));
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
                throw new CRLException("_originClone为空,请确认此对象是由查询创建");
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
