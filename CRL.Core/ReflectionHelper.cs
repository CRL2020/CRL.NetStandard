/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace CRL.Core
{
    public static class ReflectionHelper
    {
        static System.Collections.Concurrent.ConcurrentDictionary<Type, object> ReflectionInfoCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, object>();
        public static ReflectionInfo<TObject> GetInfo<TObject>(System.Reflection.ConstructorInfo constructor = null)
        {
            var type = typeof(TObject);
            object info;
            if (ReflectionInfoCache.TryGetValue(type, out info))
            {
                return (ReflectionInfo<TObject>)info;
            }
            else
            {
                var refInfo = new ReflectionInfo<TObject>(type);
                ReflectionInfoCache.TryAdd(type, refInfo);
                return refInfo;
            }
        }
    }
    public interface IReflectionInfo
    {
        void SetValue(object obj, string fieldName, object value);
        object GetValue(object obj, string fieldName);
    }
    public class ReflectionInfo<TObject>: IReflectionInfo
    {
        public string TableName { get; set; }

        public Func<TObject> CreateObjectInstance;
        internal Dictionary<string, IAccessor> accessorDict;
        internal Dictionary<string, PropertyInfo> propertyInfo;
        public ReflectionInfo(Type modelType)
        {
            CreateObjectInstance = Expression.Lambda<Func<TObject>>(Expression.New(modelType)).Compile();
            InitInfo(modelType);
        }


        private void InitInfo(Type modelType)
        {
            accessorDict = new Dictionary<string, IAccessor>();
            propertyInfo = new Dictionary<string, PropertyInfo>();
            var Properties = modelType.GetProperties();
            foreach (var prop in Properties)
            {
                IAccessor accessor = null;
                //var prop = kv.GetPropertyInfo();
                string propName = prop.Name;
                var propType = prop.PropertyType;

                if (propType.IsEnum)
                {
                    propType = propType.GetEnumUnderlyingType();
                }
                if (typeof(string) == propType)
                {
                    accessor = new StringAccessor(prop);
                }
                else if (typeof(int) == propType)
                {
                    accessor = new IntAccessor(prop);
                }
                else if (typeof(int?) == propType)
                {
                    accessor = new IntNullableAccessor(prop);
                }
                else if (typeof(DateTime) == propType)
                {
                    accessor = new DateTimeAccessor(prop);
                }
                else if (typeof(DateTime?) == propType)
                {
                    accessor = new DateTimeNullableAccessor(prop);
                }
                else if (typeof(long) == propType)
                {
                    accessor = new LongAccessor(prop);
                }
                else if (typeof(long?) == propType)
                {
                    accessor = new LongNullableAccessor(prop);
                }
                else if (typeof(float) == propType)
                {
                    accessor = new FloatAccessor(prop);
                }
                else if (typeof(float?) == propType)
                {
                    accessor = new FloatNullableAccessor(prop);
                }
                else if (typeof(double) == propType)
                {
                    accessor = new DoubleAccessor(prop);
                }
                else if (typeof(double?) == propType)
                {
                    accessor = new DoubleNullableAccessor(prop);
                }
                else if (typeof(Guid) == propType)
                {
                    accessor = new GuidAccessor(prop);
                }
                else if (typeof(Guid?) == propType)
                {
                    accessor = new GuidNullableAccessor(prop);
                }
                else if (typeof(short) == propType)
                {
                    accessor = new ShortAccessor(prop);
                }
                else if (typeof(short?) == propType)
                {
                    accessor = new ShortNullableAccessor(prop);
                }
                else if (typeof(byte) == propType)
                {
                    accessor = new ByteAccessor(prop);
                }
                else if (typeof(byte?) == propType)
                {
                    accessor = new ByteNullableAccessor(prop);

                }
                else if (typeof(char) == propType)
                {
                    accessor = new CharAccessor(prop);

                }
                else if (typeof(char?) == propType)
                {
                    accessor = new CharNullableAccessor(prop);

                }
                else if (typeof(decimal) == propType)
                {
                    accessor = new DecimalAccessor(prop);

                }
                else if (typeof(decimal?) == propType)
                {
                    accessor = new DecimalNullableAccessor(prop);
                }
                else if (typeof(byte[]) == propType)
                {
                    accessor = new ByteArrayAccessor(prop);
                }
                else if (typeof(bool) == propType)
                {
                    accessor = new BoolAccessor(prop);

                }
                else if (typeof(bool?) == propType)
                {
                    accessor = new BoolNullableAccessor(prop);

                }
                else if (typeof(TimeSpan) == propType)
                {
                    accessor = new TimeSpanAccessor(prop);
                }
                else if (typeof(TimeSpan?) == propType)
                {
                    accessor = new TimeSpanNullableAccessor(prop);

                }
                accessorDict[propName] = accessor;
                propertyInfo[propName] = prop;
            }
        }

        public IAccessor GetAccessor(string fieldName)
        {
            IAccessor accessor;
            if (accessorDict.TryGetValue(fieldName, out accessor))
            {
                return accessor;
            }
            return null;
        }
        public void SetValue(object obj, string fieldName, object value)
        {
            var ac = GetAccessor(fieldName);
            if (ac != null)
            {
                ac.Set((TObject)obj, value);
            }
            else
            {
                var a = propertyInfo.TryGetValue(fieldName,out PropertyInfo p);
                p?.SetValue(obj,value);
            }
        }
        public object GetValue(object obj, string fieldName)
        {
            var ac = GetAccessor(fieldName);
            if (ac != null)
            {
                return ac.Get((TObject)obj);
            }
            else
            {
                var a = propertyInfo.TryGetValue(fieldName, out PropertyInfo p);
                return p.GetValue(obj);
            }
        }



        public interface IAccessor
        {
            void Set(TObject obj, object value);
            object Get(TObject obj);
        }
        public abstract class Accessor<T>: IAccessor
        {
            internal PropertyInfo _prop;
            Action<TObject, T> setter;
            Func<TObject, T> getter;
            public Accessor(PropertyInfo prop)
            {
                _prop = prop;
                var setMethod = prop.GetSetMethod(true);
                var getMethod = prop.GetGetMethod(true);
                if (setMethod != null)
                {
                    setter = (Action<TObject, T>)Delegate.CreateDelegate(typeof(Action<TObject, T>), null, setMethod);
                }
                if (getMethod != null)
                {
                    getter = (Func<TObject, T>)Delegate.CreateDelegate(typeof(Func<TObject, T>), null, getMethod);
                }
            }
            public void Set(TObject obj, object value)
            {
                if (value == null || value is DBNull)
                {
                    return;
                }
                try
                {
                    setter?.Invoke(obj, (T)value);
                }
                catch(Exception ero)
                {
                    throw new Exception(string.Format("将值 {0} 赋值给类型{1}.{2}时失败,请检查对象类型和数据表字段类型是否一致", value + " " + value.GetType(), obj.GetType(), _prop));
                }
            }

            public object Get(TObject obj)
            {
                if (getter == null)
                {
                    return null;
                }
                return getter.Invoke(obj);
            }

            //protected abstract void DoSet(TObject obj, object value);
            //protected abstract object DoGet(TObject obj);

        }

        #region Accessor

        public class StringAccessor : Accessor<string>
        {
            public StringAccessor(PropertyInfo prop)
                : base(prop)
            {
                
            }
        }

        public class IntAccessor : Accessor<int>
        {
            public IntAccessor(PropertyInfo prop)
         : base(prop)
            {

            }
        }

        public class IntNullableAccessor : Accessor<int?>
        {
            public IntNullableAccessor(PropertyInfo prop)
         : base(prop)
            {

            }
        }

        public class DateTimeAccessor : Accessor<DateTime>
        {
            public DateTimeAccessor(PropertyInfo prop)
     : base(prop)
            {

            }
        }

        public class DateTimeNullableAccessor : Accessor<DateTime?>
        {
            public DateTimeNullableAccessor(PropertyInfo prop)
   : base(prop)
            {

            }
        }

        public class LongAccessor : Accessor<long>
        {
            public LongAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class LongNullableAccessor : Accessor<long?>
        {
            public LongNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class DoubleAccessor : Accessor<double>
        {
            public DoubleAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class DoubleNullableAccessor : Accessor<double?>
        {
            public DoubleNullableAccessor(PropertyInfo prop)
   : base(prop)
            {

            }
        }

        public class FloatAccessor : Accessor<float>
        {
            public FloatAccessor(PropertyInfo prop)
  : base(prop)
            {

            }
        }

        public class FloatNullableAccessor : Accessor<float?>
        {
            public FloatNullableAccessor(PropertyInfo prop)
  : base(prop)
            {

            }
        }

        public class GuidAccessor : Accessor<Guid>
        {
            public GuidAccessor(PropertyInfo prop)
 : base(prop)
            {

            }
        }

        public class GuidNullableAccessor : Accessor<Guid?>
        {
            public GuidNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class ByteAccessor : Accessor<byte>
        {
            public ByteAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }
        public class ByteNullableAccessor : Accessor<byte?>
        {
            public ByteNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class ShortAccessor : Accessor<short>
        {
            public ShortAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }
        public class ShortNullableAccessor : Accessor<short?>
        {
            public ShortNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class CharAccessor : Accessor<char>
        {
            public CharAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class CharNullableAccessor : Accessor<char?>
        {
            public CharNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class BoolAccessor : Accessor<bool>
        {
            public BoolAccessor(PropertyInfo prop)
 : base(prop)
            {

            }
        }

        public class BoolNullableAccessor : Accessor<bool?>
        {
            public BoolNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class TimeSpanAccessor : Accessor<TimeSpan>
        {
            public TimeSpanAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class TimeSpanNullableAccessor : Accessor<TimeSpan?>
        {
            public TimeSpanNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class DecimalAccessor : Accessor<decimal>
        {
            public DecimalAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class DecimalNullableAccessor : Accessor<decimal?>
        {
            public DecimalNullableAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        public class ByteArrayAccessor : Accessor<byte[]>
        {
            public ByteArrayAccessor(PropertyInfo prop)
: base(prop)
            {

            }
        }

        #endregion

    }
}


