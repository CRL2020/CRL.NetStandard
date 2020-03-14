using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Set
{
    /// <summary>
    /// DbSet上下文
    /// </summary>
    public abstract class DbSetContext
    {
        public virtual string ManageName
        {
            get
            {
                return "";
            }
        }
        Dictionary<Type, IDbSet> _DbSets = new Dictionary<Type, IDbSet>();
        protected DbSet<T> GetDbSet<T>() where T : IModel, new()
        {
            var a = _DbSets.TryGetValue(typeof(T), out IDbSet set);
            if (!a)
            {
                set = new DbSet<T>(ManageName);
                _DbSets.Add(typeof(T), set);
            }
            return set as DbSet<T>;
        }
        /// <summary>
        /// 保存所有
        /// </summary>
        public void SaveChanges()
        {
            if (_DbSets.Count == 0)
            {
                return;
            }
            var dbSet = _DbSets.First().Value;
            var a = dbSet.PackageTrans((out string ex) =>
            {
                ex = "";
                foreach (var dbSet2 in _DbSets)
                {
                    dbSet2.Value.Save();
                }
                return true;
            }, out string error);
            if (!a)
            {
                throw new Exception(error);
            }


        }
    }
}
