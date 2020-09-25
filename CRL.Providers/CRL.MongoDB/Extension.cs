using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Mongo
{
    public static class Extension
    {
        public static IDbConfigRegister UseMongoDB(this IDbConfigRegister iBuilder)
        {
            var builder = iBuilder as DBConfigRegister;
            builder.RegisterDBType(DBAccess.DBType.MongoDB, (dBAccessBuild) =>
            {
                return new MongoDBHelper(dBAccessBuild);
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
