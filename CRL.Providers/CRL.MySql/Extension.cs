using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.MySql
{
    public static class Extension
    {
        public static IDbConfigRegister UseMySql(this DBConfigRegister iBuilder)
        {
            var builder = iBuilder as DBConfigRegister;
            builder.RegisterDBType(DBAccess.DBType.MYSQL, (conn) =>
            {
                return new MySqlHelper(conn);
            }, (context) =>
            {
                return new MySQLDBAdapter(context);
            });
            return builder;
        }
    }
}
