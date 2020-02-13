using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Mongo
{
    public static class Extension
    {
        public static ISettingConfigBuilder UseMongoDB(this ISettingConfigBuilder iBuilder)
        {
            var builder = iBuilder as SettingConfigBuilder;
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
            builder.RegisterLambdaQueryType(DBAccess.DBType.MongoDB, typeof(MongoDBLambdaQuery<>));
            return builder;
        }
    }
}
