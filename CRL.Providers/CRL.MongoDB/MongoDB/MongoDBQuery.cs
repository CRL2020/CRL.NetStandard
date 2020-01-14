/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.LambdaQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Driver.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CRL.Mongo.MongoDBEx
{
    /// <summary>
    /// MongoDB不支持关联和直接语句查询
    /// 部份扩展方法支持
    /// </summary>
    public sealed partial class MongoDBExt
    {
        List<dynamic> GetDynamicResult<TModel>(LambdaQuery<TModel> query1) where TModel : IModel, new()
        {
            var query = query1 as MongoDBLambdaQuery<TModel>;
            //var selectField = query.__QueryFields;
            var selectField = query1.GetFieldMapping();
            var collection = GetCollection<TModel>();
            var pageIndex = query1.SkipPage-1;
            var pageSize = query1.TakeNum;
            var skip = 0;
            long rowNum = 0;
            if (query.TakeNum > 0)
            {
                skip = pageSize * pageIndex;
            }
            if (query.__GroupFields != null)
            {
                #region group

                var groupInfo = new BsonDocument();
                var groupField = new BsonDocument();
                foreach (var f in query.__GroupFields)
                {
                    groupField.Add(f.FieldName, "$" + f.FieldName);
                }

                groupInfo.Add("_id", groupField);
                var projection = new BsonDocument();
                foreach (var f in selectField)
                {
                    var method = f.MethodName.ToLower();
                    object sumField = 1;
                    if (!string.IsNullOrEmpty(method))
                    {
                        if (method == "count")
                        {
                            groupInfo.Add(f.ResultName, new BsonDocument("$sum", 1));
                        }
                        else
                        {
                            var memberName = f.QueryField;
                            memberName = System.Text.RegularExpressions.Regex.Replace(memberName, @"\w+\((\w+)\)", "$1");
                            groupInfo.Add(f.ResultName, new BsonDocument("$" + method.ToLower(), "$" + memberName));
                        }
                        projection.Add(f.ResultName, "$" + f.ResultName);
                    }
                    else
                    {
                        projection.Add(f.ResultName, "$_id." + f.FieldName);
                    }
                }

                var havingExp = query.mongoHavingCount;
                var filter = query.__MongoDBFilter;
                var aggregate = collection.Aggregate(new AggregateOptions() { AllowDiskUse = true }).Match(filter).Group(groupInfo).Project(projection);
                if(havingExp!=null)
                {
                    //var filter2 = query.HavingCount(havingExp);
                    var having = new BsonDocument();
                    var op = "";
                    #region op
                    switch (havingExp.ExpType)
                    {
                        case ExpressionType.Equal:
                            op = "eq";
                            break;
                        case ExpressionType.GreaterThan:
                            op = "gt";
                            break;
                        case ExpressionType.GreaterThanOrEqual:
                            op = "gte";
                            break;
                        case ExpressionType.LessThan:
                            op = "lt";
                            break;
                        case ExpressionType.LessThanOrEqual:
                            op = "lte";
                            break;
                        case ExpressionType.NotEqual:
                            op = "ne";
                            break;
                        default:
                            throw new InvalidCastException("不支持的运算符");
                    }
                    #endregion
                    having.AddRange(new BsonDocument()
                        .Add(havingExp.Left.Data.ToString(), new BsonDocument()
                                .Add("$" + op, new BsonInt64(Convert.ToInt64(havingExp.Right.Data)))
                        ));
                    aggregate = aggregate.Match(having);
                }
           
                if (query.TakeNum > 0)
                {
                    if (skip > 0)
                    {
                        aggregate = aggregate.Skip(skip);
                    }
                    aggregate = aggregate.Limit(pageSize);
                    //rowNum = collection.Count(query.__MongoDBFilter);//todo 总行数
                }
                //var str = aggregate.ToString();
                var result = aggregate.ToList();
                if (rowNum == 0)
                {
                    rowNum = result.Count();
                }
                var list = new List<dynamic>();
                foreach (var item in result)
                {
                    dynamic obj = new System.Dynamic.ExpandoObject();
                    var dict = obj as IDictionary<string, object>;
                    foreach (var f in selectField)
                    {
                        string columnName = f.ResultName;
                        object value = BsonTypeMapper.MapToDotNetValue(item[columnName]);
                        dict.Add(columnName, value);
                    }
                    list.Add(obj);
                }
                return list;
                #endregion
            }
            else if (query.__DistinctFields)
            {
                #region distinct
                string fieldName = selectField.FirstOrDefault().ResultName;
                FieldDefinition<TModel, dynamic> distinctField = fieldName;
                var query2 = collection.Distinct(distinctField, query.__MongoDBFilter);
                return query2.ToList();
                #endregion
            }
            else
            {
                #region 动态类型
                var query2 = collection.Find(query.__MongoDBFilter);
                if (query.TakeNum > 0)
                {
                    query2.Limit(pageSize);
                    if (skip > 0)
                    {
                        query2.Skip(skip);
                    }
                }
                var result = query2.ToList();
                var list = new List<dynamic>();
                var fields = TypeCache.GetTable(typeof(TModel)).FieldsDic;
                foreach (var item in result)
                {
                    dynamic obj = new System.Dynamic.ExpandoObject();
                    var dict = obj as IDictionary<string, object>;
                    foreach (var f in selectField)
                    {
                        string columnName = f.ResultName;
                        object value = fields[columnName].GetValue(item);
                        dict.Add(columnName, value);
                    }
                    list.Add(obj);
                }
                #endregion
                return list;
            }
        }
        #region QueryDynamic
        public override List<dynamic> QueryDynamic(LambdaQueryBase query)
        {
            throw new NotSupportedException("MongoDB暂未实现此方法");
        }
        public List<dynamic> QueryDynamic2<TModel>(LambdaQuery<TModel> query) where TModel : IModel, new()
        {
            var result = GetDynamicResult(query);
            return result;
        }
        #endregion

        #region QueryResult

        /// <summary>
        /// 按select返回指定类型
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public override List<TResult> QueryResult<TResult>(LambdaQueryBase query)
        {
            var typeDb = this.GetType();
            var method = typeDb.GetMethod(nameof(QueryResultByType), BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.MakeGenericMethod(new Type[] { query.__MainType, typeof(TResult) }).Invoke(this, new object[] { query });
            return result as List<TResult>;

        }
        List<TResult> QueryResultByType<TModel, TResult>(LambdaQuery.LambdaQuery<TModel> query) where TModel : IModel, new()
        {
            var result = GetDynamicResult(query);
            var type = typeof(TResult);
            var pro = type.GetProperties();
            var list = new List<TResult>();
            var reflection = ReflectionHelper.GetInfo<TResult>();
            foreach (var item in result)
            {
                var dict = item as IDictionary<string, object>;
                var obj = (TResult)System.Activator.CreateInstance(type);
                foreach (var f in pro)
                {
                    string columnName = f.Name;
                    if (dict.ContainsKey(columnName))
                    {
                        object value = dict[columnName];
                        var access = reflection.GetAccessor(columnName);
                        access.Set((TResult)obj, value);
                    }
                }
                list.Add(obj);
            }
            return list;
        }


        public override List<TResult> QueryResult<TResult>(LambdaQueryBase query, NewExpression newExpression)
        {
            var typeDb = this.GetType();
            var method = typeDb.GetMethod(nameof(QueryResultNewExpression), BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.MakeGenericMethod(new Type[] { query.__MainType, typeof(TResult) }).Invoke(this, new object[] { query, newExpression });
            return result as List<TResult>;
        }

        static Func<ObjContainer, T> CreateObjectGenerator<T>(ConstructorInfo constructor)
        {
            var type = typeof(ObjContainer);
            var parame = Expression.Parameter(type, "par");
            var parameters = constructor.GetParameters();
            List<Expression> arguments = new List<Expression>(parameters.Length);
            foreach (var parameter in parameters)
            {
                var method = ObjContainer.GetMethod(parameter.ParameterType, true);
                var getValue = Expression.Call(parame, method, Expression.Constant(parameter.Name));
                arguments.Add(getValue);
            }
            var body = Expression.New(constructor, arguments);
            var ret = Expression.Lambda<Func<ObjContainer, T>>(body, parame).Compile();
            return ret;
        }


        static Func<ObjContainer, T> CreateObjectGeneratorFromMapping<T>(IEnumerable<Attribute.FieldMapping> mapping)
        {
            var objectType = typeof(T);
            var fields = TypeCache.GetProperties(objectType, true);
            var parame = Expression.Parameter(typeof(ObjContainer), "par");
            var memberBindings = new List<MemberBinding>();
            //按顺序生成Binding
            //int i = 0;
            foreach (var mp in mapping)
            {
                if (!fields.ContainsKey(mp.FieldName))
                {
                    continue;
                }
                var m = fields[mp.FieldName].GetPropertyInfo();
                var method = ObjContainer.GetMethod(m.PropertyType, true);
                //Expression getValue = Expression.Call(method, parame);
                var getValue = parame.Call(method.Name, Expression.Constant(mp.ResultName));
                if (m.PropertyType.IsEnum)
                {
                    getValue = Expression.Convert(getValue, m.PropertyType);
                }
                var bind = (MemberBinding)Expression.Bind(m, getValue);
                memberBindings.Add(bind);
                //i += 1;
            }
            Expression expr = Expression.MemberInit(Expression.New(objectType), memberBindings);
            var ret = Expression.Lambda<Func<ObjContainer, T>>(expr, parame);
            return ret.Compile();
        }


        List<TResult> QueryResultNewExpression<TModel, TResult>(LambdaQuery<TModel> query, NewExpression newExpression) where TModel : IModel, new()
        {
            //query.Select(newExpression);
            var result = GetDynamicResult(query);
            var list = new List<TResult>();
            Func<ObjContainer, TResult> objCreater;
            var parameters = newExpression.Constructor.GetParameters();
            //当匿名类型指定了类型,没有构造参数
            if (parameters.Length > 0)
            {
                objCreater = CreateObjectGenerator<TResult>(newExpression.Constructor);
            }
            else
            {
                objCreater = CreateObjectGeneratorFromMapping<TResult>(query.GetFieldMapping());
            }
                
            foreach (IDictionary<string, object> item in result)
            {
                var objC = new ObjContainer(item);
                var obj = objCreater(objC);
                list.Add(obj);
            }
            return list;
        }
        #endregion

        public override List<TModel> QueryOrFromCache<TModel>(LambdaQueryBase query1, out string cacheKey)
        {
            cacheKey = "none";
            var query = query1 as MongoDBLambdaQuery<TModel>;
            var collection = GetCollection<TModel>();
            long rowNum = 0;
            var query2 = collection.Find(query.__MongoDBFilter).Sort(query._MongoDBSort);
            if (query.TakeNum > 0)
            {
                var pageIndex = query1.SkipPage-1;
                var pageSize = query1.TakeNum;
                var skip = pageSize * pageIndex;
                if (skip > 0)
                {
                    query2.Skip(skip);
                }
                query2.Limit(pageSize);
                rowNum = collection.Count(query.__MongoDBFilter);
            }
            var result = query2.ToList();
            if (rowNum == 0)
            {
                rowNum = result.Count();
            }
            query.__RowCount = (int)rowNum;
            SetOriginClone(result);
            return result;
        }
        public override Dictionary<TKey, TValue> ToDictionary<TModel, TKey, TValue>(LambdaQuery<TModel> query)
        {
            var dic = new Dictionary<TKey, TValue>();
            var result = GetDynamicResult(query);
            if (result.Count == 0)
            {
                return dic;
            }
            var first = result.First() as IDictionary<string, object>;
            var keys = first.Keys.ToList();
            var keyName = keys[0];
            var valueName = keys[1];
            foreach (var item in result)
            {
                var obj = item as IDictionary<string, object>;
                dic.Add((TKey)obj[keyName], (TValue)obj[valueName]);
            }
            return dic;
        }
        public override dynamic QueryScalar<TModel>(LambdaQuery<TModel> query)
        {
            var result = GetDynamicResult(query);
            if (result.Count == 0)
            {
                return null;
            }
            var first = result.First() as IDictionary<string, object>;
            var keys = first.Keys.ToList();
            return first[keys.First()];
        }
    }
}
