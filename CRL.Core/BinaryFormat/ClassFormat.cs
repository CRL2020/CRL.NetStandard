using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
namespace CRL.Core.BinaryFormat
{
    public class ClassFormat
    {

        static Dictionary<Type, IEnumerable<PropertyInfo>> proCache = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        public static byte[] Pack(Type type, object obj)
        {
            var typeInfo = type.GetReflectionInfo();
            var arry = new List<byte[]>();
            var len = 0;
            for (int i = 0; i < typeInfo.Properties.Count; i++)
            {
                var p = typeInfo.Properties[i];
                var value = typeInfo.ReflectionInfo.GetValue(obj, p.Name);
                var d = FieldFormat.Pack(p.PropertyType, value);
                arry.Add(d);
                len += d.Length;
            }
            return arry.JoinData(len);
        }

        public static object UnPack(Type type, byte[] datas)
        {
            var obj = DynamicMethodHelper.CreateCtorFuncFromCache(type)();
            var typeInfo = type.GetReflectionInfo();
            int dataIndex = 0;
            for (int i = 0; i < typeInfo.Properties.Count; i++)
            {
                var p = typeInfo.Properties[i];
                var value = FieldFormat.UnPack(p.PropertyType, datas, ref dataIndex);
                if (value == null)
                {
                    continue;
                }
                typeInfo.ReflectionInfo.SetValue(obj, p.Name, value);
                //p.SetValue(obj, value);
            }
            return obj;
        }
    }
}
