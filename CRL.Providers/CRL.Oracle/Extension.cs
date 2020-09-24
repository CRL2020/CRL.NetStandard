using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Oracle
{
    public static class Extension
    {
        public static IDbConfigRegister UseOracle(this IDbConfigRegister iBuilder)
        {
            var builder = iBuilder as DBConfigRegister;
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
