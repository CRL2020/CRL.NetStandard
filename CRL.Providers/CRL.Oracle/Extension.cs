using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Oracle
{
    public static class Extension
    {
        public static SettingConfigBuilder UseOracle(this SettingConfigBuilder builder)
        {
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
