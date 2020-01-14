using CRL.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Mongo
{
    public class MongoDBHelper : DBHelper
    {
        string _dataBaseName;
        public MongoDBHelper(string _connectionString)
            : base(_connectionString)
        {
            var lastIndex = _connectionString.LastIndexOf("/");
            var dataBaseName = _connectionString.Substring(lastIndex + 1);//like mongodb://localhost:27017/db1
            _dataBaseName = dataBaseName;
        }
        public override string DatabaseName
        {
            get
            {
                return _dataBaseName;
            }
        }
        protected override void fillCmdParams_(System.Data.Common.DbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public override DBType CurrentDBType
        {
            get { return DBType.MongoDB; }
        }

        protected override System.Data.Common.DbCommand createCmd_(string cmdText, System.Data.Common.DbConnection conn)
        {
            throw new NotImplementedException();
        }

        protected override System.Data.Common.DbCommand createCmd_()
        {
            throw new NotImplementedException();
        }

        protected override System.Data.Common.DbDataAdapter createDa_(string cmdText, System.Data.Common.DbConnection conn)
        {
            throw new NotImplementedException();
        }

        protected override System.Data.Common.DbConnection createConn_()
        {
            throw new NotImplementedException();
        }

        public override void InsertFromDataTable(System.Data.DataTable dataTable, string tableName, bool keepIdentity = false)
        {
            throw new NotImplementedException();
        }
    }
}
