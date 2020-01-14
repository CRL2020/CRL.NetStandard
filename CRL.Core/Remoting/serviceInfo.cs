using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Remoting
{
    #region obj
    public class serviceInfo
    {
        public static serviceInfo GetServiceInfo(Type type)
        {
            var info = new serviceInfo() { ServiceType = type, Attributes = type.GetCustomAttributes().ToList() };
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var methodInfoList = new List<methodInfo>();
            foreach (var m in methods)
            {
                var mInfo = new methodInfo()
                {
                    Attributes = m.GetCustomAttributes().ToList(),
                    MethodInfo = m,
                    Parameters = m.GetParameters()
                };
                methodInfoList.Add(mInfo);
            }
            info.Methods = methodInfoList;
            return info;
        }
        public Type ServiceType;
        public List<methodInfo> Methods = new List<methodInfo>();
        public List<Attribute> Attributes = new List<Attribute>();
        public T GetAttribute<T>() where T : Attribute
        {
            foreach (var item in Attributes)
            {
                if (item is T)
                {
                    return item as T;
                }
            }
            return null;
        }
        public methodInfo GetMethod(string name)
        {
            return Methods.Find(b => b.MethodInfo.Name == name);
        }
    }
    public class methodInfo
    {
        public MethodInfo MethodInfo;
        public List<Attribute> Attributes = new List<Attribute>();
        public ParameterInfo[] Parameters;
        public T GetAttribute<T>() where T : Attribute
        {
            foreach (var item in Attributes)
            {
                if (item is T)
                {
                    return item as T;
                }
            }
            return null;
        }
    }
    #endregion
}
