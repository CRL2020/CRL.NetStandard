using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL.Core.BinaryFormat
{
    public class FieldFormat
    {
        delegate byte[] toByte(Type type, object obj);
        delegate object fromByte(Type type, byte[] data);
        static Dictionary<Type, Tuple<toByte, fromByte>> methods = new Dictionary<Type, Tuple<toByte, fromByte>>();
        static FieldFormat()
        {
            if (methods.Count != 0)
            {
                return;
            }
            #region methods
            methods.Add(typeof(decimal), new Tuple<toByte, fromByte>((type, param) =>
            {
                var bits = Decimal.GetBits((decimal)param);
                var bytes = new Byte[bits.Length * 4];
                for (var i = 0; i < bits.Length; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        bytes[i * 4 + j] = (Byte)(bits[i] >> (j * 8));
                    }
                }
                return bytes;
            }, (type, data) =>
            {
                var bits = new Int32[data.Length / 4];
                for (var i = 0; i < bits.Length; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        bits[i] |= data[i * 4 + j] << j * 8;
                    }
                }
                return new Decimal(bits);
            }));

            methods.Add(typeof(string),new Tuple<toByte, fromByte>((type,param) =>
            {
                return Encoding.UTF8.GetBytes((string)param);
            },(type,data)=>
            {
                return Encoding.UTF8.GetString(data);
            }));
            methods.Add(typeof(byte), new Tuple<toByte, fromByte>((type, param) =>
            {
                return new byte[] { (byte)param };
            }, (type, data) =>
            {
                return data;
            }));
            methods.Add(typeof(bool), new Tuple<toByte, fromByte>((type, param) =>
            {
                return BitConverter.GetBytes((bool)param);
            }, (type, data) =>
            {
                return BitConverter.ToBoolean(data,0); 
            }));
            methods.Add(typeof(short), new Tuple<toByte, fromByte>((type, param) =>
            {
                return BitConverter.GetBytes((short)param);
            }, (type, data) =>
            {
                return BitConverter.ToInt16(data, 0);
            }));
            methods.Add(typeof(int), new Tuple<toByte, fromByte>((type, param) =>
            {
                return BitConverter.GetBytes((int)param);
            }, (type, data) =>
            {
                return BitConverter.ToInt32(data, 0);
            }));
            methods.Add(typeof(long), new Tuple<toByte, fromByte>((type, param) =>
            {
                return BitConverter.GetBytes((long)param);
            }, (type, data) =>
            {
                return BitConverter.ToInt64(data, 0);
            }));
            methods.Add(typeof(float), new Tuple<toByte, fromByte>((type, param) =>
            {
                return BitConverter.GetBytes((float)param);
            }, (type, data) =>
            {
                return BitConverter.ToSingle(data, 0);
            }));
            methods.Add(typeof(double), new Tuple<toByte, fromByte>((type, param) =>
            {
                return BitConverter.GetBytes((double)param);
            }, (type, data) =>
            {
                return BitConverter.ToDouble(data, 0);
            }));
            methods.Add(typeof(DateTime), new Tuple<toByte, fromByte>((type, param) =>
            {
                var ticks = ((DateTime)param).Ticks;
                return BitConverter.GetBytes(ticks);
            }, (type, data) =>
            {
                var ticks= BitConverter.ToInt64(data, 0);
                return new DateTime(ticks);
            }));
            methods.Add(typeof(byte[]), new Tuple<toByte, fromByte>((type, param) =>
            {
                return (byte[])param;
            }, (type, data) =>
            {
                return data;
            }));
            #endregion
        }
        /// <summary>
        /// 保存长度的字节长度
        /// </summary>
        static int lenSaveLength = 3;
        static Type ReturnType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                //Nullable<T> 可空属性
                return type.GenericTypeArguments[0];
            }
            //else if (type.IsByRef)
            //{
            //    var name = type.FullName.Substring(0, type.FullName.Length - 1);
            //    return Type.GetType(name);//引用类型
            //}
            return type;
        }
        public static byte[] Pack(Type type, object param)
        {
            type = ReturnType(type);

            var len = 0;

            byte[] data = null;

            if (param == null)
            {
                len = 0;
            }
            else
            {
                if (type == typeof(object))//object转为string
                {
                    throw new Exception("类型不能为object:" + param);
                    type = typeof(string);
                    if (param != null)
                    {
                        param = param.ToString();
                    }
                    else
                    {
                        param = "";
                    }
                }
                if (param is Enum)
                {
                    type = Enum.GetUnderlyingType(param.GetType());
                }
                var a = methods.TryGetValue(type, out Tuple<toByte, fromByte> method);
                if (a)
                {
                    data = method.Item1(type, param);
                }
                else
                {
                    if (type.IsGenericType || type.IsArray)
                    {
                        if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
                        {
                            data = DicFormat.Pack(param);
                        }
                        else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                        {
                            data = ListFormat.Pack(param);
                        }
                        else
                        {
                            data = ClassFormat.Pack(type, param);
                        }
                    }
                    else
                    {
                        data = ClassFormat.Pack(type, param);
                    }
                }

                if (data != null)
                {
                    len = data.Length;
                }
            }
            var lenData = BitConverter.GetBytes(len);
            var datas = new byte[lenSaveLength + len];
            Buffer.BlockCopy(lenData, 0, datas, 0, lenSaveLength);
            if (len > 0)
            {
                Buffer.BlockCopy(data, 0, datas, lenSaveLength, data.Length);
            }
            return datas;
        }

        public static object UnPack(Type type, byte[] datas, ref int offset)
        {
            type = ReturnType(type);
            if (type == typeof(object))
            {
                throw new Exception("类型不能为object:" + type);
                type = typeof(string);
            }
            object obj = null;
            var lenData = new byte[4];
            if (datas == null || datas.Length == 0)
            {
                return null;
            }
            Buffer.BlockCopy(datas, offset, lenData, 0, lenSaveLength);

            int len = BitConverter.ToInt32(lenData, 0);
            offset += lenSaveLength;
            if (len > 0)
            {
                byte[] data = new byte[len];
                Buffer.BlockCopy(datas, offset, data, 0, len);
                offset += len;

                if (type.BaseType == typeof(Enum))
                {
                     type = Enum.GetUnderlyingType(type);
                }
                var a = methods.TryGetValue(type, out Tuple<toByte, fromByte> method);
                if (a)
                {
                    return method.Item2(type, data);
                }
                if (type.IsGenericType || type.IsArray)
                {
                    if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
                    {
                        return DicFormat.UnPack(type, data);
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                    {
                        return ListFormat.UnPack(type, data);
                    }
                }
                return ClassFormat.UnPack(type, data);
            }
            return obj;
        }
    }
}
