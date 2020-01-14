using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Mongo
{
    public static class Extension
    {
        public static SettingConfigBuilder UseMongoDB(this SettingConfigBuilder builder)
        {
            builder.RegisterDBType(DBAccess.DBType.MongoDB, (conn) =>
            {
                return new MongoDBHelper(conn);
            }, (context) =>
            {
                return new MongoDBAdapter(context);
            });
            builder.RegisterDBExtend<MongoDBEx.MongoDBExt>(DBAccess.DBType.MongoDB, (context) =>
            {
                return new MongoDBEx.MongoDBExt(context);
            });
            return builder;
        }
    }
}
