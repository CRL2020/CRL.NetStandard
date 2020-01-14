using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Mongo.MongoDBEx
{
    class ObjContainer
    {
        IDictionary<string, object> _data;
        public ObjContainer(IDictionary<string, object> data)
        {
            _data = data;
        }
        object GetData(string name)
        {
            _data.TryGetValue(name, out object v);
            return v;
        }
        static Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();
        public static MethodInfo GetMethod(Type propType, bool anonymousClass = false)
        {
            var unType = Nullable.GetUnderlyingType(propType);
            var isNullable = unType != null;
            MethodInfo result;
            if (propType.IsEnum && !anonymousClass)//按是按lanbda表达式创建对象赋值时,需返回强类型方法
            {
                propType = propType.GetEnumUnderlyingType();
            }
            var Type2 = typeof(ObjContainer);
            if (methods.Count == 0)
            {
                var array = Type2.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var item in array)
                {
                    if (item.Name == "GetHashCode")
                    {
                        continue;
                    }
                    if (!item.Name.StartsWith("Get"))
                    {
                        continue;
                    }
                    methods.Add(item.ReturnType, item);
                }
            }
            if (propType.IsEnum && anonymousClass)
            {
                //按是按lanbda表达式便建对象赋值时,需返回强类型方法
                var m1 = Type2.GetMethod("GetEnum");
                var m2 = Type2.GetMethod("GetEnumNullable");
                if (isNullable)
                {
                    return m2.MakeGenericMethod(unType);
                }
                return m1.MakeGenericMethod(propType);
            }

            var a = methods.TryGetValue(propType, out result);
            if (a)
            {
                return result;
            }
            if (propType == typeof(Guid))
            {
                result = Type2.GetMethod("GetGuid");
            }
            return result;
        }

        #region method
        public TEnum GetEnum<TEnum>(string name) where TEnum : struct
        {

            var value = GetData(name);
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }
        public TEnum? GetEnumNullable<TEnum>(string name) where TEnum : struct
        {
            var value = GetData(name);
            if (value==null)
            {
                return default(TEnum);
            }
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }

        public short GetInt16(string name)
        {
            var value = GetData(name);
            return (short)value;
        }
        public short? GetInt16Nullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (short)value;
        }
        public int GetInt32(string name)
        {
            var value = GetData(name);
            return (Int32)value;
        }
        public int? GetInt32Nullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (Int32)value;
        }
        public long GetInt64(string name)
        {
            var value = GetData(name);
            return (Int64)value;
        }
        public long? GetInt64Nullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (Int64)value;
        }
        public decimal GetDecimal(string name)
        {
            var value = GetData(name);
            return (decimal)value;
        }
        public decimal? GetDecimalNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (decimal)value;
        }
        public double GetDouble(string name)
        {
            var value = GetData(name);
            return (double)value;
        }
        public double? GetDoubleNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (double)value;
        }
        public float GetFloat(string name)
        {
            var value = GetData(name);
            return (float)value;
        }
        public float? GetFloatNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (float)value;
        }
        public bool GetBoolean(string name)
        {
            var value = GetData(name);
            return Convert.ToBoolean(value);
        }
        public bool? GetBooleanNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return Convert.ToBoolean(value);
        }
        public DateTime GetDateTime(string name)
        {
            var value = GetData(name);
            return (DateTime)(value);
        }
        public DateTime? GetDateTimeNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (DateTime)(value);
        }
        public Guid GetGuid(string name)
        {
            var value = GetData(name);
            return (Guid)(value);
        }
        public Guid? GetGuidNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (Guid)(value);
        }
        public byte GetByte(string name)
        {
            var value = GetData(name);
  
            return (byte)(value);
        }
        public byte? GetByteNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (byte)(value);
        }
        public char GetChar(string name)
        {
            var value = GetData(name);
            return (char)(value);
        }
        public char? GetCharNullable(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (char)(value);
        }
        public string GetString(string name)
        {
            var value = GetData(name);
            if (value == null)
            {
                return null;
            }
            return (string)(value);
        }

        #endregion
    }
}
