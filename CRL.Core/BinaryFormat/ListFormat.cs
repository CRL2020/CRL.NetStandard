using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.BinaryFormat
{
    class ListFormat
    {
        public static byte[] Pack(object param)
        {
            var list = (System.Collections.IEnumerable)param;
            var body = new List<byte>();
            var type = param.GetType();
            var innerType = type.GenericTypeArguments[0];
            var arry = new List<byte[]>();
            var len = 0;
            foreach (var obj in list)
            {
                var data = FieldFormat.Pack(innerType, obj);
                //body.AddRange(data);
                arry.Add(data);
                len += data.Length;
            }
            //return body.ToArray();
            return arry.JoinData(len);
        }
        static Func<object, object[], object> addInvoker = null;
        public static object UnPack(Type type, byte[] datas)
        {
            if (addInvoker == null)
            {
                var method = type.GetMethod("Add");
                addInvoker = DynamicMethodHelper.CreateMethodInvoker(method);
            }
            var obj = DynamicMethodHelper.CreateCtorFuncFromCache(type)();
            var innerType = type.GenericTypeArguments[0];
            int dataIndex = 0;
            while (dataIndex < datas.Length)
            {
                var value = FieldFormat.UnPack(innerType, datas, ref dataIndex);
                addInvoker.Invoke(obj, new object[] { value });
            }
            return obj;
        }
    }
}
