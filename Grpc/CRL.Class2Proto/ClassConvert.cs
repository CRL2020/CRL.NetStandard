using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CRL.Class2Proto
{
    public class ClassConvert
    {
        //static Dictionary<string, List<string>> nameSpaceCache = new Dictionary<string, List<string>>();
        public static List<ConvertInfo> Convert(params Assembly[] assemblies)
        {
            var infos = new List<ConvertInfo>();
            foreach (var asb in assemblies)
            {
                var allTypes = asb.GetTypes();
                foreach (var type in allTypes)
                {
                    if(!type.IsAbstract)
                    {
                        continue;
                    }
                    var p = type.GetCustomAttribute(typeof(ProtoServiceAttribute));
                    if (p == null)
                    {
                        continue;
                    }
                    var p2 = p as ProtoServiceAttribute;
                    var info = new ConvertInfo();
                    info.ServiceType = type;
                    info.Namespace = p2.NameSpace;
                    if (string.IsNullOrEmpty(info.Namespace))
                    {
                        info.Namespace = $"gRPC.{type.Namespace}.{p2.PackageName}";
                    }
                    info.PackageName = p2.PackageName;
                    info.ServiceName = p2.ServiceName;
                    if (string.IsNullOrEmpty(info.ServiceName))
                    {
                        info.ServiceName = "gRPC" + type.Name;
                    }
                    infos.Add(info);
                }
            }
   
            return infos;
        }
        internal static void CreateTypeInfo(ConvertInfo convertInfo,Type type)
        {
            if (type == typeof(object))
            {
                throw new Exception($"不能为object");
            }
            if (!type.IsClass || type == typeof(string))
            {
                return ;//非class跳过
            }
            //if (Nullable.GetUnderlyingType(type) != null)
            //{
            //    //Nullable<T> 可空属性
            //    var type2 = type.GenericTypeArguments[0];
            //    CreateTypeInfo(convertInfo, type2);
            //    return;
            //}
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                CreateTypeInfo(convertInfo, keyType);
                CreateTypeInfo(convertInfo, valueType);
                return;
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var keyType = type.GenericTypeArguments[0];
                CreateTypeInfo(convertInfo, keyType);
                return;
            }

            var pros = type.GetProperties();
            var typeInfo = new TypeInfo() { Type = type };

            //var a = nameSpaceCache.TryGetValue(convertInfo.Namespace, out var cacheTypes);
            //if (!a)
            //{
            //    cacheTypes = new List<string>();
            //    nameSpaceCache.Add(convertInfo.Namespace, cacheTypes);
            //}

            foreach (var p in pros)
            {
                typeInfo.Fields.Add(new FieldInfo() { FieldName = p.Name, Type = p.PropertyType });
                if (p.PropertyType.IsEnum)
                {
                    if (!convertInfo.Enums.ContainsKey(p.PropertyType.FullName))
                    {
                        //if (cacheTypes.Contains(p.PropertyType.FullName))
                        //{
                        //    return;
                        //}
                        //cacheTypes.Add(p.PropertyType.FullName);
                        convertInfo.Enums.Add(p.PropertyType.FullName, p.PropertyType);
                    }
                }
                else if (p.PropertyType.IsClass && p.PropertyType != typeof(string))
                {
                    CreateTypeInfo(convertInfo, p.PropertyType);
                }
            }
            if (!convertInfo.ClassTypes.ContainsKey(type.FullName))
            {
                //if (cacheTypes.Contains(type.FullName))
                //{
                //    return;
                //}
                //cacheTypes.Add(type.FullName);
                convertInfo.ClassTypes.Add(type.FullName, typeInfo);
            }
        }
    }
    public class ConvertInfo
    {
        internal Type ServiceType;
        internal Dictionary<string, Type> Enums = new Dictionary<string, Type>();

        internal Dictionary<string, TypeInfo> ClassTypes = new Dictionary<string, TypeInfo>();
        internal string Namespace;
        internal string PackageName;
        internal string ServiceName;

        string convertType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                //Nullable<T> 可空属性
                var type2 = type.GenericTypeArguments[0];
                return convertType(type2);
            }
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                return $"map<{convertType(keyType)}, {convertType(valueType)}>";
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var keyType = type.GenericTypeArguments[0];
                return $"repeated {convertType(keyType)}";
            }

            if (type.IsClass && type != typeof(string))
            {
                return type.Name+"DTO";
            }
            if (type.IsEnum)
            {
                return type.Name + "DTO";
            }
            //https://blog.csdn.net/jadeshu/article/details/79183909
            switch (type.Name)
            {
                case nameof(Boolean):
                    return "bool";
                case nameof(Decimal):
                    return "double";
                case nameof(Single):
                    return "float";
                case nameof(Guid):
                    return "string";
                case nameof(DateTime):
                    return "string";
            }
            return type.Name.ToLower();
        }
        public string CreateCode()
        {
            //var packageName = ServiceType.Name;
            var lines = new List<string>();
            lines.Add("syntax = \"proto3\";");
            lines.Add($"option csharp_namespace = \"{Namespace}\";");
            lines.Add($"package {PackageName};");

            //var serviceName = ServiceType.Name;
            //methods
            lines.Add("service " + ServiceName + " {");
            foreach (var method in ServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var arguments = method.GetParameters();
                if (arguments.Length > 1)
                {
                    throw new Exception($"{method.Name} 参数不能大于一个");
                }
                var args = new List<string>();
                foreach (var f in arguments)
                {
                    var parameType = f.ParameterType;
                    if (!parameType.IsClass || parameType == typeof(string) || typeof(System.Collections.IEnumerable).IsAssignableFrom(parameType))
                    {
                        throw new Exception($"{ServiceType.Name}.{method.Name} 参数必须为class");
                    }
                    args.Add(convertType(parameType));
                    ClassConvert.CreateTypeInfo(this, parameType);
                }

                var returnType = method.ReturnType;
                if (!returnType.IsClass || returnType == typeof(string)|| typeof(System.Collections.IEnumerable).IsAssignableFrom(returnType))
                {
                    throw new Exception($"{ServiceType.Name}.{method.Name} 返回类型必须为class");
                }
                ClassConvert.CreateTypeInfo(this, returnType);

                //like  rpc SayHello (HelloRequest) returns (HelloReply);
                lines.Add($"    rpc {method.Name}({string.Join(",", args)}) returns ({convertType(returnType)});");
            }
            lines.Add("}");//end service
            //enum
            foreach (var kv in Enums)
            {
                var type = kv.Value;
                lines.Add("message " + type.Name + "DTO {");
                foreach (var e in Enum.GetValues(type))
                {
                    lines.Add($"    {e} = {(int)e};");
                }
                lines.Add("}");
            }
            //types
            foreach (var kv in ClassTypes)
            {
                var typeInfo = kv.Value;
                lines.Add("message " + typeInfo.Type.Name + "DTO {");
                int i = 0;
                foreach (var p in typeInfo.Fields)
                {
                    i++;
                    lines.Add($"    {convertType(p.Type)} {p.FieldName} = {i};");
                }
                lines.Add("}");
            }

            var code = string.Join("\r\n", lines);
            Console.WriteLine(code);
            var path = System.Environment.CurrentDirectory + "\\Protos";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //path += $"\\{Namespace}";
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            var file = path + $"\\{PackageName}.proto";
            System.IO.File.WriteAllText(file, code);
            return code;
        }
    }
    class TypeInfo
    {
        public Type Type;
        public List<FieldInfo> Fields = new List<FieldInfo>();

    }
    class FieldInfo
    {
        public Type Type;
        public string FieldName;
    }
}
