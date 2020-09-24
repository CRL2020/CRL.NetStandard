using CRL.DBAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRL
{
    public class DBAccessBuild
    {
        internal DBType _DBType;
        public string _connectionString;
        internal System.Data.Common.DbConnection _connection;
        public DBAccessBuild(DBType dbType, string connectionString)
        {
            _DBType = dbType;
            _connectionString = connectionString;
        }
        public DBAccessBuild(DBType dbType, System.Data.Common.DbConnection connection)
        {
            _DBType = dbType;
            _connection = connection;
        }
    }
}
