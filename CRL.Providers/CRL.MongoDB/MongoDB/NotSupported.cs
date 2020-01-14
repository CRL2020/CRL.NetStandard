/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Mongo.MongoDBEx
{
    public sealed partial class MongoDBExt
    {



        public override void BeginTran(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            return;
        }

        public override void RollbackTran()
        {
            return;
        }

        public override void CommitTran()
        {
            return;
        }

        public override void CheckTableCreated(Type type)
        {
            return;
        }

       

        public override Dictionary<TKey, TValue> ExecDictionary<TKey, TValue>(string sql)
        {
            throw new NotSupportedException();//不支持
        }

        public override List<dynamic> ExecDynamicList(string sql)
        {
            throw new NotSupportedException();//不支持
        }

        public override List<T> ExecList<T>(string sql)
        {
            throw new NotSupportedException();//不支持
        }


        public override T ExecObject<T>(string sql)
        {
            throw new NotSupportedException();//不支持
        }

        public override object ExecScalar(string sql)
        {
            throw new NotSupportedException();//不支持
        }

        public override T ExecScalar<T>(string sql)
        {
            throw new NotSupportedException();//不支持
        }

        public override int Execute(string sql)
        {
            throw new NotSupportedException();//不支持
        }

        public override int Run(string sp)
        {
            throw new NotSupportedException();//不支持
        }

        public override List<dynamic> RunDynamicList(string sp)
        {
            throw new NotSupportedException();//不支持
        }

        public override List<T> RunList<T>(string sp)
        {
            throw new NotSupportedException();//不支持
        }

        public override T RunObject<T>(string sp)
        {
            throw new NotSupportedException();//不支持
        }

        public override object RunScalar(string sp)
        {
            throw new NotSupportedException();//不支持
        }
    }
}
