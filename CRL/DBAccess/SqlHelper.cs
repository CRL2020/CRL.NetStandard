using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace CRL.DBAccess
{
    public class Sql2000Helper : SqlHelper
    {
        public override DBType CurrentDBType
        {
            get
            {
                return DBType.MSSQL2000;
            }
        }
        public Sql2000Helper(string content)
            : base(content)
        { }
    }
    public  class SqlHelper:DBHelper 
    {
        /// <summary>
        /// 根据参数类型实例化
        /// </summary>
        /// <param name="_connectionString">内容</param>
		public SqlHelper(string _connectionString)
            : base(_connectionString)
        { }
        
        static Dictionary<string, string> formatCache = new Dictionary<string, string>();
        static object lockObj = new object();
        public override DBType CurrentDBType
        {
            get
            {
                return DBType.MSSQL;
            }
        }
        public override string FormatWithNolock(string cmdText)
        {
            return cmdText;
        }
        protected override void fillCmdParams_(DbCommand cmd)
        {
            foreach (KeyValuePair<string, object> kv in _params)
            {
                DbParameter p = new SqlParameter(kv.Key, kv.Value);
                if (kv.Value != null)
                {
                    if (kv.Value is DBNull)
                    {
                        p.IsNullable = true;
                    }
                }
                cmd.Parameters.Add(p);
            }
            if (cmd.CommandType == CommandType.StoredProcedure)
            {
                if (OutParams != null)
                {
                    foreach (KeyValuePair<string, object> kv in OutParams)
                    {
                        //不为return ,才进行OUTPUT设置
                        if (kv.Key != "return")
                        {
                            DbParameter p = new SqlParameter(kv.Key, SqlDbType.NVarChar, 500);
                            p.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(p);
                        }
                    }
                }
                DbParameter p1 = new SqlParameter("return", SqlDbType.Int);
                p1.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(p1);
            }
        }
        protected override DbCommand createCmd_(string cmdText, DbConnection conn)
        {
            cmdText = FormatWithNolock(cmdText);
            return new SqlCommand(cmdText, (SqlConnection)conn);
        }
        protected override DbCommand createCmd_()
        {
            return new SqlCommand();
        }
        protected override DbDataAdapter createDa_(string cmdText, DbConnection conn)
        {
            cmdText = FormatWithNolock(cmdText);
            return new SqlDataAdapter(cmdText, (SqlConnection)conn);
        }
        protected override DbConnection createConn_()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// 根据表插入记录,dataTable需按查询生成结构
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        /// <param name="keepIdentity"></param>
        public override void InsertFromDataTable(DataTable dataTable, string tableName, bool keepIdentity = false)
        {
            SqlBulkCopy sqlBulkCopy;

            if (_trans != null)
            {
                SqlTransaction sqlTrans = _trans as SqlTransaction;
                sqlBulkCopy = new SqlBulkCopy(currentConn as SqlConnection, keepIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.KeepNulls, sqlTrans);
            }
            else
            {
                sqlBulkCopy = new SqlBulkCopy(base.ConnectionString, keepIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.KeepNulls);
            }
            sqlBulkCopy.DestinationTableName = tableName;
            sqlBulkCopy.BatchSize = dataTable.Rows.Count;
            if (dataTable != null && dataTable.Rows.Count != 0)
            {
                sqlBulkCopy.WriteToServer(dataTable);
            }
            sqlBulkCopy.Close();
        }
    }
}
