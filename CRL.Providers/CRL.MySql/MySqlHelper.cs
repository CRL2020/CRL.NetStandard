using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.IO;
using CRL.DBAccess;

namespace CRL.MySql
{
    public class MySqlHelper : DBHelper
    {
		public MySqlHelper(string content)
            : base(content)
        { }
        public override DBType CurrentDBType
        {
            get
            {
                return DBType.MYSQL;
            }
        }
        protected override void fillCmdParams_(DbCommand cmd)
        {
            foreach (KeyValuePair<string, object> kv in _params)
            {
                var key = kv.Key;
                //key = key.Replace("@","?");
                DbParameter p = new MySqlParameter(key, kv.Value);
                cmd.Parameters.Add(p);
            }
            if (OutParams != null)
            {
                foreach (KeyValuePair<string, object> kv in OutParams)
                {
                    var key = kv.Key;
                    //key = key.Replace("@", "?");
                    //不为return ,才进行OUTPUT设置
                    if (key != "return")
                    {
                        DbParameter p = new MySqlParameter(key, MySqlDbType.VarString, 500);
                        p.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(p);
                    }
                }
            }
            if (cmd.CommandType == CommandType.StoredProcedure)
            {
                DbParameter p1 = new MySqlParameter("return", MySqlDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(p1);
            }
        }
        protected override DbCommand createCmd_(string cmdText, DbConnection conn)
        {
            return new MySqlCommand(cmdText, (MySqlConnection)conn);
        }
        protected override DbCommand createCmd_()
        {
            return new MySqlCommand();
        }
        protected override DbDataAdapter createDa_(string cmdText, DbConnection conn)
        {

            return new MySqlDataAdapter(cmdText, (MySqlConnection)conn);
        }
        protected override DbConnection createConn_()
        {

            return new MySqlConnection(ConnectionString);
        }


        public override void InsertFromDataTable(DataTable table, string tableName, bool keepIdentity = false)
        {
            if (table.Rows.Count == 0) return ;
            string tmpPath = Path.GetTempFileName();
            string csv = DataTableToCsv(table);
            File.WriteAllText(tmpPath, csv);
            using (var conn = currentConn??createConn_())
            {
                try
                {
                    MySqlBulkLoader bulk = new MySqlBulkLoader(conn as MySqlConnection)
                    {
                        FieldTerminator = ",",
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",
                        FileName = tmpPath,
                        NumberOfLinesToSkip = 0,
                        TableName = tableName,
                    };
                    //bulk.Columns.AddRange(table.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToArray());
                    var insertCount = bulk.Load();
                }
                catch (MySqlException ex)
                {
                    throw ex;
                }
            }
            File.Delete(tmpPath);
        }


        private static string DataTableToCsv(DataTable table)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。
            StringBuilder sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append(",");
                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains(","))
                    {
                        sb.Append("\"" + row[colum].ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(row[colum].ToString());
                }
                sb.AppendLine();
            }


            return sb.ToString();
        }
    }

}
