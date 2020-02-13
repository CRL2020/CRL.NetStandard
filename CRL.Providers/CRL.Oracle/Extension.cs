using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Oracle
{
    public static class Extension
    {
        public static ISettingConfigBuilder UseOracle(this ISettingConfigBuilder iBuilder)
        {
            var builder = iBuilder as SettingConfigBuilder;
            builder.RegisterDBType(DBAccess.DBType.ORACLE, (conn) =>
            {
                return new OracleHelper(conn);
            }, (context) =>
            {
                return new ORACLEDBAdapter(context);
            });
            return builder;
        }
    }
}
