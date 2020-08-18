using System;
using System.Collections.Generic;
using System.Text;

namespace CRL.Class2Proto
{
    public class ProtoServiceAttribute : Attribute
    {
        public ProtoServiceAttribute(string packageName, string serviceName = "",string nameSpace="")
        {
            PackageName = packageName;
            ServiceName = serviceName;
            NameSpace= nameSpace;
        }
        public string PackageName { get; set; }
        public string ServiceName { get; set; }
        public string NameSpace { get; set; }
    }
}
