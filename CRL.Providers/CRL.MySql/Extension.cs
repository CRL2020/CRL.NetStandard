using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.MySql
{
    public static class Extension
    {
        public static IDbConfigRegister UseMySql(this IDbConfigRegister iBuilder)
        {
            var builder = iBuilder as DBConfigRegister;
            builder.RegisterDBType(DBAccess.DBType.MYSQL, (dBAccessBuild) =>
            {
                return new MySqlHelper(dBAccessBuild);
            }, (context) =>
            {
                return new MySQLDBAdapter(context);
            });
            return builder;
        }
    }
}
