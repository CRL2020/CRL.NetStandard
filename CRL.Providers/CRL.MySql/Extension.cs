using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.MySql
{
    public static class Extension
    {
        public static ISettingConfigBuilder UseMySql(this SettingConfigBuilder iBuilder)
        {
            var builder = iBuilder as SettingConfigBuilder;
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
