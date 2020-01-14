using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.Extension
{
    public static partial class Extension
    {
        public static string ToJson(this object obj)
        {
            return Core.SerializeHelper.SerializerToJson(obj);
        }
        public static T ToObject<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }
            return Core.SerializeHelper.DeserializeFromJson<T>(json);
        }
        public static object ToObject(this string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return Core.SerializeHelper.DeserializeFromJson(json, type);
        }
    }
}
