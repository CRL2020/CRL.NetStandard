namespace CRL.Oracle
{
    public static class Extension
    {
        public static IDbConfigRegister UseOracle(this IDbConfigRegister iBuilder)
        {
            var builder = iBuilder as DBConfigRegister;
            builder.RegisterDBType(DBAccess.DBType.ORACLE, (dBAccessBuild) =>
            {
                return new OracleHelper(dBAccessBuild);
            }, (context) =>
            {
                return new ORACLEDBAdapter(context);
            });
            return builder;
        }
    }
}
