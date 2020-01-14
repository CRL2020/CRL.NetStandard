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
        //class TypeInfo
        //{
        //    public IEnumerable<PropertyInfo> Properties;
        //    public IReflectionInfo ReflectionInfo;
        //}

        static Dictionary<Type, IEnumerable<PropertyInfo>> proCache = new Dictionary<Type, IEnumerable<PropertyInfo>>();
        public static byte[] Pack(Type type, object obj)
        {
            var typeInfo = type.GetReflectionInfo();
            var arry = new List<byte[]>();
            var len = 0;
            foreach (var p in typeInfo.Properties)
            {
                var value = typeInfo.ReflectionInfo.GetValue(obj, p.Name);
                var d = FieldFormat.Pack(p.PropertyType, value);
                //body.AddRange(d);
                arry.Add(d);
                len += d.Length;
            }
            //return body.ToArray();
            return arry.JoinData(len);
        }
        //static Dictionary<Type, TypeInfo> TypeInfoCache = new Dictionary<Type, TypeInfo>();
        //static TypeInfo getTypeInfo(Type type)
        //{
        //    var a = TypeInfoCache.TryGetValue(type, out TypeInfo typeInfo);
        //    if (!a)
        //    {
        //        var typeRef = typeof(ReflectionHelper);
        //        var method = typeRef.GetMethod(nameof(ReflectionHelper.GetInfo), BindingFlags.Public | BindingFlags.Static);
        //        var refInfo = method.MakeGenericMethod(new Type[] { type }).Invoke(null, new object[] { null }) as IReflectionInfo;
        //        var pro = type.GetProperties().Where(b => b.GetSetMethod() != null);
        //        typeInfo = new TypeInfo() { Properties = pro, ReflectionInfo = refInfo };
        //        TypeInfoCache.Add(type, typeInfo);
        //    }
        //    return typeInfo;
        //}
        public static object UnPack(Type type, byte[] datas)
        {
            var obj = System.Activator.CreateInstance(type);
            var typeInfo = type.GetReflectionInfo();
            int dataIndex = 0;
            foreach (var p in typeInfo.Properties)
            {
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
