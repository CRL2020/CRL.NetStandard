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

        void BatchInsert<TModel>(List<TModel> details, bool keepIdentity = false) where TModel : class;

        void BeginTran(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        void CheckTableCreated(Type type);

        void ClearParame();

        void CommitTran();

        int Count<TType>(Expression<Func<TType, bool>> expression, bool compileSp = false) where TType : class;

        void CreateTableIndex<TModel>();

        int Delete<T>(ILambdaQuery<T> query) where T : class;

        int Delete<TModel, TJoin>(Expression<Func<TModel, TJoin, bool>> expression)
            where TModel : class
            where TJoin : class;

        int Delete<TModel>(Expression<Func<TModel, bool>> expression) where TModel : class;

        int Delete<TModel>(object id) where TModel : class;

        Dictionary<TKey, TValue> ExecDictionary<TKey, TValue>(string sql);

        List<dynamic> ExecDynamicList(string sql);

        List<T> ExecList<T>(string sql) where T : class, new();

        T ExecObject<T>(string sql) where T : class, new();

        object ExecScalar(string sql);

        T ExecScalar<T>(string sql);

        int Execute(string sql);

        //TType GetFunction<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> selectField, FunctionType functionType, bool compileSp = false) where TModel : class;
        object GetOutParam(string name);

        //T GetOutParam<T>(string name);
        int GetReturnValue();

        void InsertFromObj<TModel>(TModel obj) where TModel : class;
        TType Max<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : class;

        TType Min<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : class;

        List<dynamic> QueryDynamic(LambdaQueryBase query);

        TModel QueryItem<TModel>(Expression<Func<TModel, bool>> expression, bool idDest = true, bool compileSp = false) where TModel : class;

        TModel QueryItem<TModel>(object id) where TModel : class;

        List<TModel> QueryList<TModel>(Expression<Func<TModel, bool>> expression = null, bool compileSp = false) where TModel : class;

        List<TModel> QueryList<TModel>(ILambdaQuery<TModel> query) where TModel : class;

        List<TModel> QueryOrFromCache<TModel>(ILambdaQuery<TModel> query, out string cacheKey) where TModel : class;

        List<TResult> QueryResult<TResult>(LambdaQueryBase query);

        List<TResult> QueryResult<TResult>(LambdaQueryBase query, NewExpression newExpression);

        dynamic QueryScalar<TModel>(ILambdaQuery<TModel> query) where TModel : class;

        void RollbackTran();

        int Run(string sp);

        List<dynamic> RunDynamicList(string sp);

        List<T> RunList<T>(string sp) where T : class, new();

        T RunObject<T>(string sp) where T : class, new();

        object RunScalar(string sp);

        //void SetOriginClone<TModel>(List<TModel> list) where TModel : class;
        //void SetParam(string name, object value);
        TType Sum<TType, TModel>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false) where TModel : class;

        Dictionary<TKey, TValue> ToDictionary<TModel, TKey, TValue>(ILambdaQuery<TModel> query) where TModel : class;

        //string ToString();
        int Update<TModel, TJoin>(Expression<Func<TModel, TJoin, bool>> expression, ParameCollection updateValue)
            where TModel : class
            where TJoin : class;

        int Update<TModel>(Expression<Func<TModel, bool>> expression, dynamic updateValue) where TModel : class;

        int Update<TModel>(Expression<Func<TModel, bool>> expression, ParameCollection setValue) where TModel : class;

        int Update<TModel>(Expression<Func<TModel, bool>> expression, TModel model) where TModel : class;

        int Update<TModel>(ILambdaQuery<TModel> query, ParameCollection updateValue) where TModel : class;

        int Update<TModel>(List<TModel> objs) where TModel : class;

        int Update<TModel>(TModel obj) where TModel : class;

    }
}