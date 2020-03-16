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
        public DbSetContext()
        {
            _dbContext = getDbContext(ManageName);
        }
        DbContext getDbContext(string manageName)
        {
            var dbLocation = new CRL.DBLocation() { DataAccessType = DataAccessType.Default, ManageType = GetType(), ManageName = manageName };
            var helper = SettingConfig.GetDBAccessBuild(dbLocation).GetDBHelper();
            var dbContext = new DbContext(helper, dbLocation);
            return dbContext;
        }
        DbContext _dbContext;
        //Dictionary<Type, IDbSet> _DbSets = new Dictionary<Type, IDbSet>();
        protected DbSet<T> GetDbSet<T>() where T : IModel, new()
        {
            var name = $"set_{typeof(T)}";
            var a = _dbContext._DbSets.TryGetValue(name, out IDbSet set);
            if (!a)
            {
                set = new DbSet<T>(name, _dbContext);
                //_dbContext._DbSets.Add(name, set);
            }
            return set as DbSet<T>;
        }

        bool PackageTrans(DbContext dbContext,TransMethod method, out string error, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            error = "";
            var db = DBExtendFactory.CreateDBExtend(dbContext);
            db.BeginTran(isolationLevel);
            bool result;
            try
            {
                result = method(out error);
                if (!result)
                {
                    db.RollbackTran();
                    return false;
                }
                db.CommitTran();
            }
            catch (Exception ero)
            {
                error = "提交事务时发生错误:" + ero.Message;
                db.RollbackTran();
                return false;
            }
            return result;
        }

        /// <summary>
        /// 保存所有
        /// </summary>
        public void SaveChanges()
        {
            if (_dbContext._DbSets.Count == 0)
            {
                return;
            }
            var a = PackageTrans(_dbContext, (out string ex) =>
             {
                 ex = "";
                 foreach (var dbSet2 in _dbContext._DbSets)
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
