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
using System.Linq.Expressions;
using System.Text;
using CRL.Core;
using CRL.LambdaQuery;
namespace CRL.DBExtend.RelationDB
{
    public sealed partial class DBExtend
    {
        /// <summary>
        /// 格式化为更新值查询
        /// </summary>
        /// <param name="setValue"></param>
        /// <param name="joinType"></param>
        /// <returns></returns>
        string ForamtSetValue<T>(ParameCollection setValue, Type joinType = null) where T : IModel
        {
            //string tableName = TypeCache.GetTableName(typeof(T), dbContext);
            string setString = "";
            var fields = TypeCache.GetProperties(typeof(T), true);
            foreach (var pair in setValue)
            {
                string name = pair.Key;
                object value = pair.Value;

                value = ObjectConvert.CheckNullValue(value);

                if (name.StartsWith("$"))//直接按值拼接 c2["$SoldCount"] = "SoldCount+" + num;
                {
                    name = name.Substring(1, name.Length - 1);
                    if (!fields.ContainsKey(name))
                    {
                        throw new CRLException("找不到对应的字段,在" + typeof(T) + ",名称" + name);
                    }
                    var field = fields[name];
                    string value1 = value.ToString();
                    //未处理空格
                    value1 = System.Text.RegularExpressions.Regex.Replace(value1, name + @"([\+\-])", _DBAdapter.KeyWordFormat(field.MapingName) + "$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    name = field.MapingName;
                    value = value1;
                    setString += string.Format(" {0}={1},", _DBAdapter.KeyWordFormat(name), value);
                }
                else
                {
                    if (joinType != null && value.ToString().Contains("$"))//当是关联更新
                    {

                        if (!fields.ContainsKey(name))
                        {
                            throw new CRLException("找不到对应的字段,在" + typeof(T) + ",名称" + name);
                        }
                        var field = fields[name];
                        name = field.MapingName;//转换映射名

                        var fields2 = TypeCache.GetProperties(joinType, true);
                        var value1 = System.Text.RegularExpressions.Regex.Match(value.ToString(), @"\$(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[1].Value;
                        if (!fields2.ContainsKey(value1))
                        {
                            throw new CRLException("找不到对应的字段,在" + joinType + ",名称" + value1);
                        }
                        var field2 = fields2[value1];
                        value = value.ToString().Replace("$" + value1, "t2." + field2.MapingName);//右边字段需加前辍
                        name = string.Format("t1.{0}", name);
                    }
                    else
                    {
                        if (!fields.ContainsKey(name))
                        {
                            throw new CRLException("找不到对应的字段,在" + typeof(T) + ",名称" + name);
                        }
                        var field = fields[name];
                        name = field.MapingName;//转换映射名
                        var parame = _DBAdapter.GetParamName(name, dbContext.parIndex);
                        AddParam(parame, value);
                        dbContext.parIndex += 1;
                        value = parame;
                        if (joinType != null)//mysql 修正
                        {
                            name = string.Format("t1.{0}", name);
                        }
                    }
                    setString += string.Format(" {0}={1},", name, value);

                }
            }
            setString = setString.Substring(0, setString.Length - 1);
            return setString;
        }
        #region update


        /// <summary>
        /// 指定拼接条件更新
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="setValue"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        internal int Update<TModel>(ParameCollection setValue, string where) where TModel : IModel, new()
        {
            CheckTableCreated<TModel>();
            Type type = typeof(TModel);
            string table = TypeCache.GetTableName(type, dbContext);
            string setString = ForamtSetValue<TModel>(setValue);
            string sql = _DBAdapter.GetUpdateSql(table, setString, where);
            sql = _DBAdapter.SqlFormat(sql);
            var db = GetDBHelper();
            var n = SqlStopWatch.Execute(db, sql);
            ClearParame();
            return n;
        }
        
        ///// <summary>
        ///// 按对象差异更新,由主键确定记录
        ///// </summary>
        ///// <typeparam name="TModel"></typeparam>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public override int Update<TModel>(TModel obj)
        //{
        //    var c = GetUpdateField(obj);
        //    if (c.Count == 0)
        //    {
        //        return 0;
        //        //throw new CRLException("更新集合为空");
        //    }
        //    var primaryKey = TypeCache.GetTable(obj.GetType()).PrimaryKey;
        //    var keyValue = primaryKey.GetValue(obj);
        //    var pName = _DBAdapter.GetParamName("par", "1");
        //    var where = string.Format("where {0}={1}", _DBAdapter.KeyWordFormat(primaryKey.MapingName), pName);

        //    AddParam(pName, keyValue);
        //    int n = Update<TModel>(c, where);
        //    UpdateCacheItem(obj, c);
        //    if (n == 0)
        //    {
        //        throw new CRLException("更新失败,找不到主键为 " + keyValue + " 的记录");
        //    }
        //    obj.CleanChanges();
        //    return n;
        //}

        /// <summary>
        /// 按完整查询条件进行更新
        /// goup语法不支持,其它支持
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="updateValue"></param>
        /// <returns></returns>
        public override int Update<TModel>(LambdaQuery<TModel> query, ParameCollection updateValue)
        {
            var query1 = query as RelationLambdaQuery<TModel>;
            if (query1.__GroupFields != null)
            {
                throw new CRLException("update不支持group查询");
            }
            if (query1.__Relations != null && query1.__Relations.Count > 1)
            {
                throw new CRLException("update关联不支持多次");
            }
            if (updateValue.Count == 0)
            {
                throw new ArgumentNullException("更新时发生错误,参数值为空 ParameCollection setValue");
            }

            var sb = new StringBuilder();
            query1.GetQueryConditions(sb, false);
            var conditions = sb.ToString().Trim();

 
            query1.FillParames(this);

            if (query1.__Relations!=null)
            {
                var kv = query1.__Relations.First();
                string setString = ForamtSetValue<TModel>(updateValue, kv.Key.OriginType);
                var t1 = query1.QueryTableName;
                var t2 = TypeCache.GetTableName(kv.Key.OriginType, query1.__DbContext);

                string sql = _DBAdapter.GetRelationUpdateSql(t1, t2, conditions, setString, query1);
                return Execute(sql);
            }
            else
            {
                conditions = conditions.Replace("t1.","");
            }
            return Update<TModel>(updateValue, conditions);
        }
        public override int Update<TModel>(List<TModel> objs)
        {
            var table = TypeCache.GetTable(typeof(TModel));
            var primaryKey = table.PrimaryKey;
            int index = 0;
            var db = GetDBHelper();
            var sb = new StringBuilder();
            foreach (var obj in objs)
            {
                var c = GetUpdateField(obj);
                if (c.Count == 0)
                {
                    continue;
                }
                index += 1;
                var keyValue = primaryKey.GetValue(obj);
                var keyParme = _DBAdapter.GetParamName(primaryKey.MapingName, index);
                var where = $" where {_DBAdapter.KeyWordFormat(primaryKey.MapingName)}={keyParme}";
                db.AddParam(keyParme, keyValue);
                var setString = string.Join(",", c.Select(b => string.Format("{0}='{1}'", _DBAdapter.KeyWordFormat(b.Key), b.Value)));
                string sql = _DBAdapter.GetUpdateSql(table.TableName, setString, where);
                sb.AppendLine(sql + ";");
            }
            return db.Execute(sb.ToString());
        }
        ///// <summary>
        ///// 关联更新
        ///// </summary>
        ///// <typeparam name="TModel"></typeparam>
        ///// <typeparam name="TJoin"></typeparam>
        ///// <param name="expression"></param>
        ///// <param name="updateValue"></param>
        ///// <returns></returns>
        //public override int Update<TModel, TJoin>(Expression<Func<TModel, TJoin, bool>> expression, ParameCollection updateValue)
        //{
        //    var query = new RelationLambdaQuery<TModel>(dbContext);
        //    query.Join<TJoin>(expression);
        //    return Update(query, updateValue);
        //}

        #endregion
    }
}
