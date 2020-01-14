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
using System.Data;
using System.Linq.Expressions;

namespace CRL
{
    public interface IAbsDBExtend
    {

        void AddOutParam(string name, object value = null);

        void AddParam(string name, object value);

        void BatchInsert<TModel>(List<TModel> details, bool keepIdentity = false) where TModel : IModel, new();

        void BeginTran(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        void CheckTableCreated(Type type);

        void ClearParame();

        void CommitTran();

        int Count<TType>(Expression<Func<TType, bool>> expression, bool compileSp = false) where TType : IModel, new();

        void CreateTableIndex<TModel>();

        int Delete<T>(LambdaQuery<T> query) where T : IModel, new();

        int Delete<TModel, TJoin>(Expression<Func<TModel, TJoin, bool>> expression)
            where TModel : IModel, new()
            where TJoin : IModel, new();

        int Delete<TModel>(Expression<Func<TModel, bool>> expression) where TModel : IModel, new();

        int Delete<TModel>(object id) where TModel : IModel, new();

        Dictionary<TKey, TValue> ExecDictionary<TKey, TValue>(string sql);

        List<dynamic> ExecDynamicList(string sql);

        List<T> ExecList<T>(string sql) where T : class, new();

        T ExecObject<T>(string sql) where T : class, new();

        object ExecScalar(string sql);

        T ExecScalar<T>(string sql);

        int Execute(string sql);

        //TType GetFunction<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> selectField, FunctionType functionType, bool compileSp = false) where TModel : IModel, new();
        object GetOutParam(string name);

        //T GetOutParam<T>(string name);
        int GetReturnValue();

        void InsertFromObj<TModel>(TModel obj) where TModel : IModel, new();
        TType Max<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new();

        TType Min<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new();

        List<dynamic> QueryDynamic(LambdaQueryBase query);

        TModel QueryItem<TModel>(Expression<Func<TModel, bool>> expression, bool idDest = true, bool compileSp = false) where TModel : IModel, new();

        TModel QueryItem<TModel>(object id) where TModel : IModel, new();

        List<TModel> QueryList<TModel>(Expression<Func<TModel, bool>> expression = null, bool compileSp = false) where TModel : IModel, new();

        List<TModel> QueryList<TModel>(LambdaQuery<TModel> query) where TModel : IModel, new();

        List<TModel> QueryOrFromCache<TModel>(LambdaQueryBase query, out string cacheKey) where TModel : IModel, new();

        List<TResult> QueryResult<TResult>(LambdaQueryBase query);

        List<TResult> QueryResult<TResult>(LambdaQueryBase query, NewExpression newExpression);

        dynamic QueryScalar<TModel>(LambdaQuery<TModel> query) where TModel : IModel, new();

        void RollbackTran();

        int Run(string sp);

        List<dynamic> RunDynamicList(string sp);

        List<T> RunList<T>(string sp) where T : class, new();

        T RunObject<T>(string sp) where T : class, new();

        object RunScalar(string sp);

        //void SetOriginClone<TModel>(List<TModel> list) where TModel : IModel, new();
        //void SetParam(string name, object value);
        TType Sum<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : IModel, new();

        Dictionary<TKey, TValue> ToDictionary<TModel, TKey, TValue>(LambdaQuery<TModel> query) where TModel : IModel, new();

        //string ToString();
        int Update<TModel, TJoin>(Expression<Func<TModel, TJoin, bool>> expression, ParameCollection updateValue)
            where TModel : IModel, new()
            where TJoin : IModel, new();

        int Update<TModel>(Expression<Func<TModel, bool>> expression, dynamic updateValue) where TModel : IModel, new();

        int Update<TModel>(Expression<Func<TModel, bool>> expression, ParameCollection setValue) where TModel : IModel, new();

        int Update<TModel>(Expression<Func<TModel, bool>> expression, TModel model) where TModel : IModel, new();

        int Update<TModel>(LambdaQuery<TModel> query, ParameCollection updateValue) where TModel : IModel, new();

        int Update<TModel>(List<TModel> objs) where TModel : IModel, new();

        int Update<TModel>(TModel obj) where TModel : IModel, new();

    }
}