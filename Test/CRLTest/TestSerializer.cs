using CRL.Core;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRLTest
{
    #region obj
    [ProtoContract]
    class testClass
    {
        [ProtoMember(1)]
        public string name
        {
            get; set;
        }
        public string name2
        {
            get; set;
        }
        [ProtoMember(2)]
        public DateTime? time
        {
            get; set;
        }
        //public b b
        //{
        //    get; set;
        //}
        [ProtoMember(3)]
        public decimal price
        {
            get; set;
        }
        //public Dictionary<string, object> dic
        //{
        //    get; set;
        //}
        [ProtoMember(4)]
        public decimal price2
        {
            get; set;
        }
        [ProtoMember(5)]
        public decimal price3
        {
            get; set;
        }
    }
    public class b
    {
        public string name
        {
            get; set;
        }
        public string name2
        {
            get; set;
        }
    }
    public class MyGenericClass<T>
    {

    }
    #endregion
    class TestSerializer
    {
        static void testFormat()
        {

            var obj = new testClass() { };
            //obj.b = new b() { name = "b22424" };
            //obj.dic = new Dictionary<string, object>() { { "tes111111111112t", 1 }, { "t2222222222122est2", 1 }, { "te22222221s3t", 1 } };
            obj.name = "test2ConvertObject";
            obj.time = DateTime.Now;
            obj.price = 1002;
            //obj.name2 = "sss";
            obj.price2 = 2;
            obj.price3 = 10;
            int count = 1;


            new CounterWatch().Start("testJson", () =>
            {
                testJson(obj);
            }, count);

            new CounterWatch().Start("testBinary", () =>
            {
                testBinary(obj);
            }, count);
            new CounterWatch().Start("testProtobuf", () =>
            {
                testProtobuf(obj);
            }, count);
        }
        static int testJson(testClass obj)
        {
            var json = SerializeHelper.SerializerToJson(obj);
            var len = Encoding.UTF8.GetBytes(json).Length;
            Console.WriteLine(len);
            //var obj2 = SerializeHelper.DeserializeFromJson<testClass>(json);
            return len;
        }
        static int testProtobuf(testClass obj)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, obj);
                //var obj2 = ProtoBuf.Serializer.Deserialize<testClass>(ms);
                Console.WriteLine(ms.Length);
                return (int)ms.Length;
            }

        }

        static int testBinary(testClass obj)
        {
            var data = CRL.Core.BinaryFormat.ClassFormat.Pack(obj.GetType(), obj);
            //var obj2 = CRL.Core.BinaryFormat.ClassFormat.UnPack(obj.GetType(), data);
            Console.WriteLine(data.Length);
            return data.Length;
        }
    }
}
