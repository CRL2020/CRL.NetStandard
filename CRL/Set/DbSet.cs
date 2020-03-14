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
        internal abstract bool PackageTrans(TransMethod method, out string error, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);
    }

    /// <summary>
    /// DbSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DbSet<T> : IDbSet where T : IModel, new()
    {
        #region inner
        class DbSetProvider : BaseProvider<T>
        {
            public override string ManageName => _manageName;
            string _manageName;
            public DbSetProvider(string manageName)
            {
                _manageName = manageName;
            }
        }
        #endregion
        public DbSet(string manageName)
        {
            _BaseProvider = new DbSetProvider(manageName);
        }
        BaseProvider<T> _BaseProvider;
        /// <summary>
        /// 返回BaseProvider
        /// </summary>
        /// <returns></returns>
        public BaseProvider<T> GetProvider()
        {
            return _BaseProvider;
        }

        /// <summary>
        /// 获取查询表达式
        /// </summary>
        /// <returns></returns>
        public ILambdaQuery<T> GetQuery()
        {
            return _BaseProvider.GetLambdaQuery();
        }
        /// <summary>
        /// 返回所有
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return GetQuery().ToList();
        }
        /// <summary>
        /// 按条件返回
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<T> FindAll(Expression<Func<T, bool>> expression)
        {
            return GetQuery().Where(expression).ToList();
        }
        /// <summary>
        /// 查找一个
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Find(object id)
        {
            return _BaseProvider.QueryItem(id);
        }
        internal List<T> addObjs = new List<T>();
        internal List<T> removeObjs = new List<T>();
        internal List<T> updateObjs = new List<T>();
        /// <summary>
        /// 添加
        /// 需调用Save保存更改
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            addObjs.Add(item);
            //_BaseProvider.Add(item);
        }
        /// <summary>
        /// 删除一项
        /// 需调用Save保存更改
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            removeObjs.Add(item);
            //return _BaseProvider.Delete(item);
        }
        /// <summary>
        /// 保存更改
        /// </summary>
        internal override void Save()
        {
            _BaseProvider.Add(addObjs);
            foreach (var item in removeObjs)
            {
                _BaseProvider.Delete(item);
            }
            _BaseProvider.Update(updateObjs);
            addObjs.Clear();
            removeObjs.Clear();
            updateObjs.Clear();
        }
        internal override bool PackageTrans(TransMethod method, out string error, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            return _BaseProvider.PackageTrans(method, out error, isolationLevel);
        }
        /// <summary>
        /// 更改
        /// 需调用Save保存更改
        /// </summary>
        /// <param name="item"></param>
        public void Update(T item)
        {
            updateObjs.Add(item);
            //return _BaseProvider.Update(item);
        }
        #region 函数
        public TType Sum<TType>(Expression<Func<T, bool>> expression, Expression<Func<T, TType>> field, bool compileSp = false)
        {
            return _BaseProvider.Sum(expression, field, compileSp);
        }
        public int Count(Expression<Func<T, bool>> expression, bool compileSp = false)
        {
            return _BaseProvider.Count(expression, compileSp);
        }
        #endregion
    }
}
