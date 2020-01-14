using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Extension
{
    public static partial class Extension
    {
        static System.Collections.Concurrent.ConcurrentDictionary<Type, TypeInfo> TypeInfoCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, TypeInfo>();
        public static TypeInfo GetReflectionInfo(this Type type)
        {
            var a = TypeInfoCache.TryGetValue(type, out TypeInfo typeInfo);
            if (!a)
            {
                var typeRef = typeof(ReflectionHelper);
                var method = typeRef.GetMethod(nameof(ReflectionHelper.GetInfo), BindingFlags.Public | BindingFlags.Static);
                var refInfo = method.MakeGenericMethod(new Type[] { type }).Invoke(null, new object[] { null }) as IReflectionInfo;
                var pro = type.GetProperties().Where(b => b.GetSetMethod() != null);
                typeInfo = new TypeInfo() { Properties = pro, ReflectionInfo = refInfo };
                TypeInfoCache.TryAdd(type, typeInfo);
            }
            return typeInfo;
        }
        public static Type MakeGenericType(string mainTypeName, string dll, params Type[] typeArguments)
        {
            var classType = Type.GetType($"{mainTypeName}`1, {dll}");
            if (classType == null)
            {
                throw new Exception("未找到" + mainTypeName);
            }
            var constructedType = classType.MakeGenericType(typeArguments);
            return constructedType;
        }
    }
    public class TypeInfo
    {
        public IEnumerable<PropertyInfo> Properties;
        public IReflectionInfo ReflectionInfo;
    }
}
