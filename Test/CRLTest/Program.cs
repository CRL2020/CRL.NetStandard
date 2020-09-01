using CRL;
using CRL.Core;
using CRL.DBAccess;
using CRLTest.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using CRL.RedisProvider;
using CRL.Mongo;
using ProtoBuf;

namespace CRLTest
{
    
    class Program
    {
        static void Main(string[] args)
        {
            CRLInit.Init();

        label1:
            Console.WriteLine("ok");
            Console.ReadLine();
            goto label1;
        }

        static void TestAll()
        {
            var array = typeof(Code.TestAll).GetMethods(BindingFlags.Static | BindingFlags.Public).OrderBy(b => b.Name.Length);
            var instance = new Code.TestAll();
            foreach (var item in array)
            {
                if (item.Name == "TestUpdate")
                {
                    continue;
                }
                try
                {
                    item.Invoke(instance, null);
                    Console.WriteLine($"{item.Name} ok");
                }
                catch(System.Exception ero)
                {
                    Console.WriteLine($"{item.Name} error {ero.Message}");
                }
 
            }
        }
    }
}
