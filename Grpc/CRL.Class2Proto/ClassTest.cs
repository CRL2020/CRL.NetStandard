using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Class2Proto
{
    [ProtoServiceAttribute("protoTest", "ClassTestAction")]
    public abstract class ClassTestAction
    {
        public abstract ClassTest getObj(TestObj a);
        public abstract ClassTest getObj2(TestObj a);
    }
    [ProtoServiceAttribute("protoTest", "ClassTestAction2")]
    public abstract class ClassTestAction2
    {
        public abstract ClassTest getObj(TestObj a);
        public abstract ClassTest getObj2(TestObj a);
    }
    public class ClassTestBase
    {
        public string Id { get; set; }
    }
    //[ProtoClassConvert]
    public class ClassTest: ClassTestBase
    {
        public string Name { get; set; }
        public int? nullableValue { get; set; }
        public Status Status { get; set; }
        public TestObj Data { get; set; }
        public List<string> Name2 { get; set; }
        public List<TestObj> Data2 { get; set; }
        public Dictionary<string, TestObj> Data3 { get; set; }
        public DateTime time { get; set; }
        public decimal decimalValue { get; set; }

    }
    public enum Status
    {
        ok,fail
    }
    public class TestObj
    {
        public string Id { get; set; }
    }
}
