using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

using System.Data;

using CRL.Core;

namespace CRL.DBAccess
{
    public abstract class DBHelper
    {
        #region 字段和属性
        /// <summary>
        /// 语句执行时间
        /// </summary>
        public double ExecuteTime;
        protected string databaseName;
        /// <summary>
        /// 数据库名,连接成功后才知道
        /// </summary>
        public virtual string DatabaseName
        {
            get
            {
                if (string.IsNullOrEmpty(databaseName))
                {
                    var conn = createConn_();
                    databaseName = conn.Database;
                    conn.Close();
                }
                return databaseName;
            }
        }

        protected Dictionary<string, object> _params;
        public Dictionary<string, object> outParams;
        /// <summary>
        /// 新的输出参数集合
        /// </summary>
        public Dictionary<string, object> OutParams
        {
            get
            {
                if (outParams == null)
                {
                    outParams = new Dictionary<string, object>();
                }
                return outParams;
            }
            set
            {
                if (outParams == null)
                {
                    outParams = new Dictionary<string, object>();
                }
                outParams = value;
            }
        }

        Dictionary<string, object> OutParamsPut;


        /// <summary>
        /// 是否自动把查询加上WithNolock
        /// </summary>
        public bool AutoFormatWithNolock = true;

        protected DbConnection currentConn = null;
        protected DbTransaction _trans = null;
        bool autoCloseConn = true;

        /// <summary>
        /// 是否自动关闭连接
        /// 默认为true
        /// 否则需要手动关闭
        /// </summary>
        public bool AutoCloseConn
        {
            get { return autoCloseConn; }
            set { autoCloseConn = value; }
        }

        /// <summary>
        /// 是否记录错误日志
        /// </summary>
        public bool LogError = true;
        /// <summary>
        /// 连接串
        /// </summary>
        public string ConnectionString;
       /// <summary>
       /// 输入参数
        /// 不推荐直接访问此属性,用AddParam方法代替
       /// </summary>
        public Dictionary<string, object> Params
        {
            get
            {
                if (_params == null)
                {
                    _params = new Dictionary<string, object>();
                }
                return _params;
            }
            set
            {
                if (_params == null)
                {
                    _params = new Dictionary<string, object>();
                }
                _params = value;
            }
        }
        /// <summary>
        /// 清除参数
        /// 在重复执行SQL时需调用进而重新设定新参数
        /// </summary>
        public void ClearParams()
        {
            Params.Clear();
            if (OutParams == null)
                return;
            OutParams.Clear();// 增加了参数OutParamsPut 单独存储
        }
        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddParam(string name, object value)
        {
            Params.Add(name, value);
        }
        /// <summary>
        /// 设置参数,没有就添加,有就更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParam(string name, object value)
        {
            if (Params.ContainsKey(name))
            {
                Params[name] = value;
            }
            else
            {
                Params.Add(name, value);
            }
        }
        /// <summary>
        /// format为加上 with(nolock)
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual string FormatWithNolock(string sql)
        {
            return sql;
        }
        /// <summary>
        /// 输出参数
        /// 不推荐直接访问此属性,用AddOutParam和GetOutPut方法代替
        /// </summary>
        //public List<DbParameter> OutParams
        //{
        //    get { return _outParams; }
        //    set { _outParams = value; }
        //}

        /// <summary>
        /// 添加一个输出参数
        /// 此参数只支持能转换为string类型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddOutParam(string name, object value = null)
        {
            if (OutParams == null)
                OutParams=new Dictionary<string, object>();
            name = name.Replace("@", "");
            OutParams.Add(name, value);
        }

        /// <summary>
        /// 获取存储过程的return值,如果没有则为0
        /// sql没有
        /// </summary>
        /// <returns></returns>
        public int GetReturnValue()
        {
            if (OutParamsPut == null)
                return 0;
            string name = "return";
            if (!OutParamsPut.ContainsKey(name))
            {
                return 0;
            }
            return Convert.ToInt32(OutParamsPut[name]);
        }
        
        /// <summary>
        /// 获取OUTPUT的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetOutParam(string name)
        {
            name = name.Replace("@","");
            if (CurrentDataReadCommand != null && !getOutPutValue)
            {
                GetOutPutValue(CurrentDataReadCommand);
            }
            return OutParamsPut[name];
        }
        void GetOutPutValue(DbCommand cmd)
        {
            if (cmd.CommandType != CommandType.StoredProcedure)
            {
                return;
            }
            if(cmd.Parameters.Count==0)
            {
                return;
            }
            OutParamsPut = new Dictionary<string, object>();
            foreach (DbParameter i in cmd.Parameters)
            {
                string key;
                switch (i.Direction)
                {
                    case ParameterDirection.Output:
                        key = i.ParameterName;
                        key = key.Replace("@", "");
                        OutParamsPut[key] = i.Value;
                        break;
                    case ParameterDirection.ReturnValue:
                        key = "return";
                        OutParamsPut[key] = i.Value;
                        break;
                }
            }
            getOutPutValue = true;
        }
        #endregion

        #region 结构函数

        public DBHelper(string _connectionString)
        {
            if(string.IsNullOrEmpty(_connectionString))
            {
                throw new Exception("连接字符串为空");
            }
            _params = new Dictionary<string, object>();
            ConnectionString = _connectionString;
        }
        #endregion

        #region 子类要实现的抽象方法
        protected abstract void fillCmdParams_(DbCommand cmd);
        /// <summary>
        /// 当前数据库类型
        /// </summary>
        public abstract DBType CurrentDBType { get;}
        protected abstract DbCommand createCmd_(string cmdText, DbConnection conn);
        protected abstract DbCommand createCmd_();
        protected abstract DbDataAdapter createDa_(string cmdText, DbConnection conn);
        protected abstract DbConnection createConn_();

        public abstract void InsertFromDataTable(DataTable dataTable, string tableName, bool keepIdentity = false);

        #endregion
        public string Name;
        public override string ToString()
        {
            return string.Format("{0} {1}",DatabaseName,Name);
        }
        #region 私有方法
        void CreateConn()
        {
            if (currentConn == null)
            {
                currentConn = createConn_();
                currentConn.Open();
            }
            else
            {
                if (currentConn.State == ConnectionState.Closed)
                {
                    currentConn.Open();
                }
            }
        }
      
        void LogCommand(DbCommand cmd,Exception error)
        {
            if (!LogError)
                return;
            string str = error.Message;
            if (cmd != null)//可能为空
            {
                str += string.Format("\r\n在库{0} 类型:{1} 语句:{2} 参数:\r\n", DatabaseName,cmd.CommandType, cmd.CommandText);
                List<string> list = new List<string>();
                foreach (DbParameter a in cmd.Parameters)
                {
                    if (a.ParameterName != "return")
                    {
                        string p = string.Format("[{0}] {1}:{2}", a.Direction, a.ParameterName, a.Value);
                        list.Add(p);
                    }
                }
                str += string.Join("\r\n", list.ToArray());
            }
            EventLog.Log(str, "DbError");
        }
        private int do_(string text, CommandType type)
        {
            var time = DateTime.Now;
            CreateConn();
            DbCommand cmd = createCmd_(text, currentConn);
            cmd.CommandTimeout = 180;
            if (_trans != null)
            {
                cmd.Transaction = _trans;
            }
            cmd.CommandType = type;
            fillCmdParams_(cmd);
            int a = 0;
            try
            {
                a = cmd.ExecuteNonQuery();
                var ts = DateTime.Now - time;
                ExecuteTime += ts.TotalMilliseconds;
            }
            catch (DbException ero)
            {
                LogCommand(cmd, ero);
                CloseConn(true);
                throw ero;
            }
            GetOutPutValue(cmd);
            CloseConn();
            return a;
        }

        private DataSet doDateSet_(string text, CommandType type)
        {
            var time = DateTime.Now;
            CreateConn();
            DbDataAdapter da = createDa_(text, currentConn);
            if (_trans != null)
            {
                da.SelectCommand.Transaction = _trans;
            }
            da.SelectCommand.CommandType = type;
            fillCmdParams_(da.SelectCommand);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds);
                GetOutPutValue(da.SelectCommand);
                var ts = DateTime.Now - time;
                ExecuteTime += ts.TotalMilliseconds;
            }
            catch (DbException ero)
            {
                LogCommand(da.SelectCommand, ero);
                CloseConn(true);
                throw ero;
            }
            CloseConn();
            return ds;
        }
        private object doScalar_(string text, CommandType type)
        {
            var time = DateTime.Now;
            CreateConn();
            DbCommand cmd = createCmd_(text, currentConn);
            if (_trans != null)
            {
                cmd.Transaction = _trans;
            }
            fillCmdParams_(cmd);
            cmd.CommandType = type;
            object a = null;
            try
            {
                a = cmd.ExecuteScalar();
                GetOutPutValue(cmd);
                var ts = DateTime.Now - time;
                ExecuteTime += ts.TotalMilliseconds;
            }
            catch (Exception ero)
            {
                LogCommand(cmd, ero);
                CloseConn(true);
                throw ero;
            }
            CloseConn();
            return a;
        }
        /// <summary>
        /// 使用DataReader时,上次Command
        /// DbDataReader在关闭前,取不到存储过程out值
        /// </summary>
        DbCommand CurrentDataReadCommand = null;
        bool getOutPutValue = false;
        private DbDataReader doDataReader_(string text, CommandType type)
        {
            var time = DateTime.Now;
            CreateConn();
            getOutPutValue = false;
            CurrentDataReadCommand = null;
            CurrentDataReadCommand = createCmd_(text, currentConn);
            CurrentDataReadCommand.Transaction = _trans;
            CurrentDataReadCommand.CommandType = type;
            fillCmdParams_(CurrentDataReadCommand);
            DbDataReader r;
            try
            {
                r = CurrentDataReadCommand.ExecuteReader(AutoCloseConn ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                //GetOutPutValue(CurrentDataReadCommand);
                var ts = DateTime.Now - time;
                ExecuteTime += ts.TotalMilliseconds;
            }
            catch (Exception ero)
            {
                LogCommand(CurrentDataReadCommand, ero);
                CloseConn(true);
                throw ero;
            }
            return r;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 当需手动关闭时调用
        /// </summary>
        /// <param name="close">是否强制关闭</param>
        public void CloseConn(bool close = false)
        {
            if (_trans != null)//有事务时不关闭
            {
                return;
            }
            if (!AutoCloseConn && !close)//不是自动关闭时不关闭
            {
                return;
            }
            if (currentConn != null)
            {
                currentConn.Close();
                currentConn.Dispose();
            }
            currentConn = null;
        }
        /// <summary>
        /// 执行一条sql语句，返回影响行数
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public int Execute(string sql)
        {
            return do_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回影响行数
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public int Run(string sp)
        {
            return do_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行一条sql语句，返回DataTable
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public DataTable ExecDataTable(string sql)
        {
            return ExecDataSet(sql).Tables[0];
        }
        
        /// <summary>
        /// 执行一个存储过程，返回DataTable
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public DataTable RunDataTable(string sp)
        {
            return RunDataSet(sp).Tables[0];
        }
        /// <summary>
        /// 执行一条sql语句，返回DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet ExecDataSet(string sql)
        {
            return doDateSet_(sql, CommandType.Text);
        }

        /// <summary>
        /// 执行一个存储过程，返回DataSet
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public DataSet RunDataSet(string sp)
        {
            return doDateSet_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行一条sql语句，返回首行首列
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public object ExecScalar(string sql)
        {
            return doScalar_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回首行首列
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public object RunScalar(string sp)
        {
            return doScalar_(sp, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行一条sql语句，返回DbDataReader
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public DbDataReader ExecDataReader(string sql)
        {
            return doDataReader_(sql, CommandType.Text);
        }
        /// <summary>
        /// 执行一个存储过程，返回DbDataReader
        /// </summary>
        /// <param name="sp">存储过程</param>
        /// <returns></returns>
        public DbDataReader RunDataReader(string sp)
        {
            return doDataReader_(sp, CommandType.StoredProcedure);
        }
        #endregion

        #region 事务处理
        /// <summary>
        /// 开始事务,调用事务必须调用CommitTran()提交事务或者调用RollbackTran()回滚事务
        /// </summary>
        public void BeginTran()
        {
            if (_trans != null)
            {
                throw new Exception("事务已启动");
            }
            CreateConn();
            _trans = currentConn.BeginTransaction();
        }
        public void BeginTran(IsolationLevel isolationLevel)
        {
            if (_trans != null)
            {
                throw new Exception("事务已启动");
            }
            CreateConn();
            _trans = currentConn.BeginTransaction(isolationLevel);
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTran()
        {
            if (currentConn == null)
            {
                throw new Exception("数据连接意外关闭");
            }
            try
            {
                _trans.Commit();
                currentConn.Close();
                _trans = null;
                currentConn = null;
            }
            catch (InvalidOperationException ex)
            {
                currentConn.Close();
                _trans = null;
                currentConn = null;
            }
            catch (DbException ee)
            {
                currentConn.Close();
                _trans = null;
                currentConn = null;
                throw new Exception(ee.Message);
            }
        }
        /// <summary>
        /// 回滚事务事务
        /// </summary>
        public void RollbackTran()
        {
            if (currentConn == null)
            {
                throw new Exception("数据连接意外关闭");
            }
            try
            {
                _trans.Rollback();
                currentConn.Close();
                _trans = null;
                currentConn = null;
            }
            catch (InvalidOperationException ex)
            {
                currentConn.Close();
                _trans = null;
                currentConn = null;
            }
            catch (DbException ee)
            {
                currentConn.Close();
                _trans = null;
                currentConn = null;
                throw new Exception(ee.Message);
            }
        }
        #endregion
    }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DBType
    {
        /// <summary>
        /// ms sql
        /// </summary>
        MSSQL,
        /// <summary>
        /// ms sql2000
        /// </summary>
        MSSQL2000,
        /// <summary>
        /// ms access
        /// </summary>
        ACCESS,
        /// <summary>
        /// mysql
        /// </summary>
        MYSQL,
        /// <summary>
        /// oracle
        /// </summary>
        ORACLE,
        /// <summary>
        /// MongoDB
        /// </summary>
        MongoDB
    }
}