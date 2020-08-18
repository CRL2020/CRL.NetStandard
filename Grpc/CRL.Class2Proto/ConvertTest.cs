using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Class2Proto
{
    public class ConvertTest
    {
        public static void Test()
        {
            var convertInfo = ClassConvert.Convert(System.Reflection.Assembly.GetAssembly(typeof(ClassTest)));
            convertInfo.ForEach(b => b.CreateCode());
            //Console.WriteLine(code);
            Console.ReadLine();
        }
    }
}
