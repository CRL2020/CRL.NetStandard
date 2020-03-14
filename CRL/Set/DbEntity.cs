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
using System.Text;
using System.Threading.Tasks;

namespace CRL.Set
{
    /// <summary>
    /// 对象关联
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbEntity<T> where T : IModel, new()
    {
        internal object mainValue = null;

        Expression<Func<T, bool>> _relationExp;
        string _manageName;
        internal DbEntity(Expression<Func<T, object>> member, object key, Expression<Func<T, bool>> expression = null,string manageName = "")
        {
            _manageName = manageName;
            mainValue = key;
            Expression relationExpression;
            var parameterExpression = member.Parameters.ToArray();
            if (member.Body is UnaryExpression)
            {
                relationExpression = ((UnaryExpression)member.Body).Operand;
            }
            else
            {
                relationExpression = member.Body;
            }
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
        /// 获取当前值
        /// </summary>
        /// <returns></returns>
        public T Value
        {
            get
            {
                var db = getAbsDBExtend();
                var item = db.QueryItem(_relationExp);
                return item;
            } 
        }
    }
}
