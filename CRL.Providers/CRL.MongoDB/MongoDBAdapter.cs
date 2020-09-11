/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.Attribute;
using CRL.DBAccess;
using CRL.DBAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Mongo
{
    internal class MongoDBAdapter : DBAdapterBase
    {
        static bool inited = false;
        public MongoDBAdapter(DbContext _dbContext)
            : base(_dbContext)
        {
            if (!inited)
            {
                inited = true;
                //var serializer = new DateTimeSerializer(DateTimeKind.Local, BsonType.DateTime);
                //BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
            }
        }
        public override DBType DBType
        {
            get { return DBType.MongoDB; }
        }
        public override bool CanCompileSP
        {
            get
            {
                return false;
            }
        }
        public override string GetColumnType(Attribute.FieldInnerAttribute info, out string defaultValue)
        {
            defaultValue = "";
            return info.PropertyType.Name;
        }

        public override Dictionary<Type, string> FieldMaping()
        {
            //todo
            Dictionary<Type, string> dic = new Dictionary<Type, string>();
            //字段类型对应
            dic.Add(typeof(System.String), "String");
            dic.Add(typeof(System.Decimal), "Decimal");
            dic.Add(typeof(System.Double), "Double");
            dic.Add(typeof(System.Single), "Single");
            dic.Add(typeof(System.Boolean), "Boolean");
            dic.Add(typeof(System.Int32), "Integer");
            dic.Add(typeof(System.Int16), "Integer");
            dic.Add(typeof(System.Enum), "Integer");
            dic.Add(typeof(System.Byte), "Binary data");
            dic.Add(typeof(System.DateTime), "Date");
            dic.Add(typeof(System.UInt16), "Integer");
            dic.Add(typeof(System.Int64), "Integer");
            dic.Add(typeof(System.Object), "Object");
            dic.Add(typeof(System.Byte[]), "Binary data");
            dic.Add(typeof(System.Guid), "nvarchar(50)");
            return dic;
        }

        public override string GetColumnIndexScript(Attribute.FieldInnerAttribute filed)
        {
            throw new NotImplementedException();
        }

        public override string GetCreateColumnScript(Attribute.FieldInnerAttribute field)
        {
            throw new NotImplementedException();
        }

        public override string GetCreateSpScript(string spName, string script)
        {
            throw new NotImplementedException();
        }

        public override void CreateTable(DbContext dbContext, List<Attribute.FieldInnerAttribute> fields, string tableName)
        {
            throw new NotImplementedException();
        }

        public override void BatchInsert(DbContext dbContext, System.Collections.IList details, bool keepIdentity = false)
        {
            throw new NotImplementedException();
        }

        public override string GetTableFields(string tableName)
        {
            throw new NotImplementedException();
        }

        public override object InsertObject<T>(DbContext dbContext, T obj)
        {
            throw new NotImplementedException();
        }

        public override void GetSelectTop(StringBuilder sb, string fields, Action<StringBuilder> query, string sort, int top)
        {
            throw new NotImplementedException();
        }

        public override string GetWithNolockFormat(bool v)
        {
            throw new NotImplementedException();
        }

        public override string GetAllSPSql(string db)
        {
            throw new NotImplementedException();
        }

        public override string GetAllTablesSql(string db)
        {
            throw new NotImplementedException();
        }

        public override string SpParameFormat(string name, string type, bool output)
        {
            throw new NotImplementedException();
        }


        public override string TemplateGroupPage
        {
            get { throw new NotImplementedException(); }
        }

        public override string TemplatePage
        {
            get { throw new NotImplementedException(); }
        }

        public override string TemplateSp
        {
            get { throw new NotImplementedException(); }
        }

        public override string SqlFormat(string sql)
        {
            throw new NotImplementedException();
        }
        public override string CastField(string field, Type fieldType)
        {
            throw new NotImplementedException();
        }

        public override string GetParamName(string name, object index)
        {
            return name;
        }
        public override string GetColumnUnionIndexScript(string tableName, string indexName, List<string> columns, FieldIndexType fieldIndexType)
        {
            throw new NotImplementedException();
        }
        public override string DateTimeFormat(string field, string format)
        {
            throw new NotImplementedException();
        }
        public override string GetSplitFirst(string field, string parName)
        {
            throw new NotImplementedException();
        }
    }
}
