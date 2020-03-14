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
    /// <summary>
    /// DbSet结构,增强对象关联性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbEntities<T> where T : IModel, new()
    {
        internal object mainValue = null;

        Expression<Func<T, bool>> _relationExp;
        string _manageName;
        internal DbEntities(Expression<Func<T, object>> member, object key, Expression<Func<T, bool>> expression = null,string manageName="")
        {
            mainValue = key;
            _manageName = manageName;
            MemberExpression relationExpression;
            var parameterExpression = member.Parameters.ToArray();
            if (member.Body is UnaryExpression)
            {
                relationExpression = ((UnaryExpression)member.Body).Operand as MemberExpression;
            }
            else
            {
                relationExpression = member.Body as MemberExpression;
            }
            if (relationExpression == null)
            {
                throw new CRLException("member 不为 MemberExpression");
            }
            //memberName = relationExpression.Member.Name;
            var constant = Expression.Constant(mainValue);
            var body = Expression.Equal(relationExpression, constant);
            _relationExp = Expression.Lambda<Func<T, bool>>(body, parameterExpression);
            if (expression != null)
            {
                _relationExp = _relationExp.AndAlso(expression);
            }
        }
        AbsDBExtend getAbsDBExtend()
        {
            var dbLocation = new CRL.DBLocation() { DataAccessType = DataAccessType.Default, ManageType = typeof(T), ManageName = _manageName };
            var helper = SettingConfig.GetDBAccessBuild(dbLocation).GetDBHelper();
            var dbContext = new DbContext(helper, dbLocation);
            var db = DBExtendFactory.CreateDBExtend(dbContext);
            return db;
        }

        /// <summary>
        /// 返回所有
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            var db = getAbsDBExtend();
            return db.QueryList(_relationExp);
        }
        #region 函数
        public TType Sum<TType>(Expression<Func<T, bool>> expression, Expression<Func<T, TType>> field, bool compileSp = false)
        {
            var db = getAbsDBExtend();
            return db.Sum(expression,field,compileSp);
        }
        public int Count(Expression<Func<T, bool>> expression, bool compileSp = false)
        {
            var db = getAbsDBExtend();
            return db.Count(expression, compileSp);    
        }
        #endregion
    }
}
