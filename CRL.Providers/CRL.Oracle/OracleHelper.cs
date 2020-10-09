﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types; 
using System.Data;
using CRL.DBAccess;
using System.Text.RegularExpressions;

namespace CRL.Oracle
{
    public class OracleHelper : DBHelper
    {
        public OracleHelper(DBAccessBuild dBAccessBuild)
            : base(dBAccessBuild)
        { }
        public override DBType CurrentDBType
        {
            get
            {
                return DBType.ORACLE;
            }
        }
        protected override void fillCmdParams_(DbCommand cmd)
        {
            if (cmd.CommandType == CommandType.Text)
            {
                if (_params.Count > 0)
                {
                    string sql = cmd.CommandText;
                    MatchCollection matches = Regex.Matches(sql, @":\w+", RegexOptions.ECMAScript);
                    for (int i = 0; i < matches.Count; i++)
                    {
                        string k = matches[i].Value;
                        if (_params.ContainsKey(k))
                        {
                            var kv = _params[k];
                            DbParameter p = new OracleParameter(k, kv);
                            if (kv != null)
                            {
                                if (kv is DBNull)
                                {
                                    p.IsNullable = true;
                                }
                            }
                            else
                            {
                                p.IsNullable = true;
                                p.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(p);
                        }
                    }

                }
            }
            else
            {
                foreach (KeyValuePair<string, object> kv in _params)
                {
                    DbParameter p = new OracleParameter(kv.Key, kv.Value);
                    cmd.Parameters.Add(p);
                }
            }
            if (OutParams != null)
            {
                foreach (KeyValuePair<string, object> kv in OutParams)
                {
                    //不为return ,才进行OUTPUT设置
                    if (kv.Key != "return")
                    {
                        DbParameter p;
                        if (kv.Key.ToLower().EndsWith("cursor"))// cursor
                        {
                            p = new OracleParameter(kv.Key,  OracleDbType.Char);
                        }
                        else
                        {
                            p = new OracleParameter(kv.Key, OracleDbType.NChar, 500);
                        }
                        p.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(p);
                    }
                }
            }
            if (cmd.CommandType == CommandType.StoredProcedure)
            {
                //DbParameter p1 = new OracleParameter("return", OracleType.Int32);
                //p1.Direction = ParameterDirection.ReturnValue;
                //cmd.Parameters.Add(p1);
            }
        }
        protected override DbCommand createCmd_(string cmdText, DbConnection conn)
        {
            var cmd = new OracleCommand(cmdText, (OracleConnection)conn)
            {
                BindByName = true
            };
            return cmd;
        }
        protected override DbCommand createCmd_()
        {
            return new OracleCommand();
        }
        protected override DbDataAdapter createDa_(string cmdText, DbConnection conn)
        {

            return new OracleDataAdapter(cmdText, (OracleConnection)conn);
        }
        protected override DbConnection createConn_()
        {

            return new OracleConnection(ConnectionString);
        }


        public override void InsertFromDataTable(DataTable dataTable, string tableName, bool keepIdentity = false)
        {
            throw new NotSupportedException("Oracle不支持批量插入");
            throw new NotImplementedException();
        }

        public int Insert(string sql,string seqName)
        {
            using (DbConnection conn = createConn_())
            {
                string seqSql = string.Format("Select {0}.nextval  from dual", seqName);

                DbCommand cmd = createCmd_(seqSql, conn);
                conn.Open();
                //todo 事务控制
                //if (_trans != null)
                //{
                //    cmd.Transaction = _trans;
                //}
                cmd.CommandType = CommandType.Text;
                int id = Convert.ToInt32(cmd.ExecuteScalar());    //获取ID
                SetParam("Id",id);
                fillCmdParams_(cmd);
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                return id;

            }
        }
    }
}
