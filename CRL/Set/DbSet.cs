using CRL.LambdaQuery;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CRL.Set
{
    public abstract class IDbSet
    {
        internal abstract void Save();
        //internal abstract bool PackageTrans(TransMethod method, out string error, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);
    }

    /// <summary>
    /// DbSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbSet<T> : IDbSet where T : IModel, new()
    {
        //#region inner
        //class DbSetProvider : BaseProvider<T>
        //{
        //    public override string ManageName => _manageName;
        //    string _manageName;
        //    public DbSetProvider(string manageName)
        //    {
        //        _manageName = manageName;
        //    }
        //}
        //#endregion
        internal DbContextInner _dbContext;
        internal Expression<Func<T, bool>> _relationExp = null;
        public DbSet(string name, DbContextInner dbContext)
        {
            _dbContext = dbContext;
            _dbContext._DbSets.Add(name, this);
        }
        //BaseProvider<T> _BaseProvider;
        //BaseProvider<T> BaseProvider
        //{
        //    get
        //    {
        //        if (_BaseProvider == null)
        //        {
        //            _BaseProvider = new DbSetProvider(_dbContext.DBLocation.ManageName);
        //        }
        //        return _BaseProvider;
        //    }
        //}
        ///// <summary>
        ///// 返回BaseProvider
        ///// </summary>
        ///// <returns></returns>
        //public BaseProvider<T> GetProvider()
        //{
        //    return BaseProvider;
        //}

        ///// <summary>
        ///// 获取查询表达式
        ///// </summary>
        ///// <returns></returns>
        //public ILambdaQuery<T> GetQuery()
        //{
        //    return BaseProvider.GetLambdaQuery();
        //}

        AbsDBExtend getAbsDBExtend()
        {
            if (_dbContext == null)
            {
                throw new Exception("_dbContext为空");
            }
            var db = DBExtendFactory.CreateDBExtend(_dbContext);
            return db;
        }

        /// <summary>
        /// 返回所有
        /// </summary>
        /// <returns></returns>
        public List<T> All()
        {
            var db = getAbsDBExtend();
            return db.QueryList(_relationExp);
            //return GetQuery().ToList();
        }
        /// <summary>
        /// 按条件返回
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<T> FindAll(Expression<Func<T, bool>> expression)
        {
            var db = getAbsDBExtend();
            return db.QueryList(_relationExp.AndAlso(expression));
            //return GetQuery().Where(expression).ToList();
        }
        public T Find(Expression<Func<T, bool>> expression)
        {
            var db = getAbsDBExtend();
            return db.QueryItem(_relationExp.AndAlso(expression));
        }

        /// <summary>
        /// 查找一个
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T FindById(object id)
        {
            var db = getAbsDBExtend();
            return db.QueryItem<T>(id);
            //return BaseProvider.QueryItem(id);
        }
        #region crud
        internal List<T> addObjs;
        internal List<T> removeObjs;
        internal List<T> updateObjs;
        /// <summary>
        /// 添加
        /// 需调用Save保存更改
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            addObjs = addObjs ?? new List<T>();
            addObjs.Add(item);
        }
        /// <summary>
        /// 删除一项
        /// 需调用Save保存更改
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            removeObjs = removeObjs ?? new List<T>();
            removeObjs.Add(item);
        }
        /// <summary>
        /// 更改
        /// 需调用Save保存更改
        /// </summary>
        /// <param name="item"></param>
        public void Update(T item)
        {
            updateObjs = updateObjs ?? new List<T>();
            updateObjs.Add(item);
        }
        /// <summary>
        /// 保存更改
        /// </summary>
        internal override void Save()
        {
            var db = getAbsDBExtend();
            if (addObjs != null)
            {
                if (addObjs.Count == 1)
                {
                    db.InsertFromObj(addObjs[0]);
                }
                else
                {
                    db.BatchInsert(addObjs);
                }
            }
            if (removeObjs != null)
            {
                foreach (var item in removeObjs)
                {
                    db.Delete<T>(item);
                }
            }
            if(updateObjs!=null)
            {
                db.Update(updateObjs);
            }

            addObjs?.Clear();
            removeObjs?.Clear();
            updateObjs?.Clear();
        }
        //internal override bool PackageTrans(TransMethod method, out string error, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        //{
        //    return BaseProvider.PackageTrans(method, out error, isolationLevel);
        //}

        #endregion

        #region 函数
        public TType Sum<TType>(Expression<Func<T, bool>> expression, Expression<Func<T, TType>> field)
        {
            var db = getAbsDBExtend();
            return db.Sum(_relationExp.AndAlso(expression), field);
        }
        public int Count(Expression<Func<T, bool>> expression = null)
        {
            var db = getAbsDBExtend();
            return db.Count(_relationExp.AndAlso(expression));
        }
        #endregion
    }
}
