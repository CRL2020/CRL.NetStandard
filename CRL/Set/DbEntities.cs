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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Set
{
    public abstract class IDbEntities
    {
        //public abstract void Save(); 
    }
    public class DbEntities<T> : DbSet<T> where T : class, new()
    {
        internal DbEntities(string name, DbContextInner dbContext, Expression<Func<T, bool>> relationExp) : base(name, dbContext)
        {
            _relationExp = relationExp;
        }
    }
}
