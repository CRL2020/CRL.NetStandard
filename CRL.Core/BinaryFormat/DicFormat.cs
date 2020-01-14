using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.BinaryFormat
{
    class DicFormat
    {
        public static byte[] Pack(object param)
        {
            var list = (System.Collections.IDictionary)param;
            var type = param.GetType();
            var allArgs = type.GenericTypeArguments;
            var innerType = allArgs[0];
            var innerType2 = allArgs[1];
            var arry = new List<byte[]>();
            var len = 0;
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key;
                var value = enumerator.Value;

                var keyData = FieldFormat.Pack(innerType, key);
                var valueData = FieldFormat.Pack(innerType2, value);

                arry.Add(keyData);
                arry.Add(valueData);
                len += keyData.Length + valueData.Length;
            }
            return arry.JoinData(len);
        }
        public static object UnPack(Type type, byte[] datas)
        {
            var dic = (System.Collections.IDictionary)System.Activator.CreateInstance(type);
            var allArgs = type.GenericTypeArguments;
            var innerType = allArgs[0];
            var innerType2 = allArgs[1];
            int dataIndex = 0;
            while (dataIndex < datas.Length)
            {
                var key = FieldFormat.UnPack(innerType, datas, ref dataIndex);
                var value = FieldFormat.UnPack(innerType2, datas, ref dataIndex);
                if (key == null)
                {
                    continue;
                }
                dic.Add(key,value);
            }
            return dic;
        }
    }
}

