/**
* CRL 快速开发框架 V5
* Copyright (c) 2019 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL5
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using CRL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
namespace CRL
{
    /// <summary>
    /// 对象结构信息导出
    /// </summary>
    public class SummaryAnalysis
    {
        public static bool OnlyCRLModel = true;
        #region obj
        class ObjItem
        {
            public string Name;
            public string Remark;
            public List<FieldItem> Fields = new List<FieldItem>();
            public void Add(FieldItem item)
            {
                Fields.Add(item);
            }
        }
        class FieldItem
        {
            public string Remark;
            public string Name;
            public Type Type;
            public override string ToString()
            {
                return string.Format("{0} {1}", Name, Remark);
            }
        }
        #endregion
        static List<Type> GetInfoFromDll(Type[] currentTypes, out List<string> xmlFiles)
        {
            List<Type> findTypes = new List<Type>();
            xmlFiles = new List<string>();
            var removeKeys = new List<string>() { "RequestModel", "ResponseModel" };
            Dictionary<string, List<FieldItem>> fileds = new Dictionary<string, List<FieldItem>>();
            foreach (var currentType in currentTypes)
            {
                #region 加载类型
                var assembyle = System.Reflection.Assembly.GetAssembly(currentType);
                var file = assembyle.CodeBase.ToLower().Replace("file:///", "").Replace("/","\\").Replace(".dll", ".xml");
                if (!System.IO.File.Exists(file))
                {
                    throw new Exception(file + " 注释文件不存在");
                }
                xmlFiles.Add(file);
                Type[] types = assembyle.GetTypes();
                foreach (var type in types)
                {
                    if (OnlyCRLModel && !typeof(CRL.IModel).IsAssignableFrom(type))
                    {
                        continue;
                    }
                    bool remove = false;
                    foreach (var k in removeKeys)
                    {
                        if (type.FullName.Contains(k))
                        {
                            remove = true;
                        }
                    }
                    if (!remove && type.IsClass)
                    {
                        findTypes.Add(type);
                    }
                }
                #endregion
            }
            return findTypes.OrderBy(b => b.Name).ToList();
        }
        static Dictionary<string, ObjItem> GetInfoFromXml(List<string> xmlFiles)
        {
            Dictionary<string, ObjItem> objItems = new Dictionary<string, ObjItem>();
            if (xmlFiles == null)
            {
                return objItems;
            }
            if (xmlFiles.Count == 0)
            {
                return objItems;
            }
            var CRLModelFile = Core.Request.RequestHelper.GetFilePath("/bin/CRL.Package.xml");
            if (System.IO.File.Exists(CRLModelFile))
            {
                xmlFiles.Add(CRLModelFile);
            }
            foreach (string xmlFile in xmlFiles)
            {
                var rootE = XElement.Load(xmlFile);
                //找对象注释
                IEnumerable<XElement> query2 =
                                            from ele in rootE.Element("members").Elements("member")
                                            where ele.Attribute("name").Value.StartsWith("T:")
                                            select ele;
                foreach (XElement e in query2)
                {
                    string name = e.Attribute("name").Value.Substring(2);
                    var summary = e.Element("summary");
                    string remark = "";
                    if (summary != null)
                    {
                        remark = summary.Value.Trim();
                    }
                    if (!objItems.ContainsKey(name))
                    {
                        objItems.Add(name, new ObjItem() { Name = name, Remark = remark });
                    }
                }
                //属性
                IEnumerable<XElement> query =
                                            from ele in rootE.Element("members").Elements("member")
                                            where ele.Attribute("name").Value.StartsWith("P:")
                                            select ele;
                foreach (XElement e in query)
                {
                    string name = e.Attribute("name").Value.Substring(2);
                    int index = name.LastIndexOf('.');
                    string propertyName = name.Substring(index + 1, name.Length - index - 1);
                    string typeName = name.Substring(0, index);
                    var summary = e.Element("summary");
                    string remark = "";
                    if (summary != null)
                    {
                        remark = summary.Value.Trim();
                    }
                    if (!objItems.ContainsKey(typeName))
                    {
                        objItems.Add(typeName, new ObjItem() { Name = typeName });
                    }
                    objItems[typeName].Add(new FieldItem() { Name = propertyName, Remark = remark });
                }
                
            }
            var list2 = new List<FieldItem>();
            list2.Add(new FieldItem() { Name = "Id", Type = typeof(Int32), Remark = "自增主键" });
            list2.Add(new FieldItem() { Name = "AddTime", Type = typeof(DateTime), Remark = "添加时间" });
            objItems.Add("CRL.IModelBase", new ObjItem() { Name = "CRL.IModelBase", Remark = "", Fields = list2 });
            return objItems;
        }

        static Dictionary<Type, CRL.Attribute.TableInnerAttribute> Merge(List<Type> types,Dictionary<string, ObjItem> objItems)
        {
            var result = new Dictionary<Type, CRL.Attribute.TableInnerAttribute>();
            foreach(var type in types)
            {
                string typeName = type.FullName;
                var list2 = CRL.TypeCache.GetProperties(type, true);
                var table = new CRL.Attribute.TableInnerAttribute();
                if (objItems.ContainsKey(typeName))
                {
                    var objItem = objItems[typeName];
                    table.Remark = objItem.Remark;
                    Type parentType = type.BaseType;
                    while (parentType != typeof(Object))
                    {
                        if (objItems.ContainsKey(parentType.FullName))
                        {
                            objItem.Fields.AddRange(objItems[parentType.FullName].Fields);
                        }
                        parentType = parentType.BaseType;
                    }
         
                    foreach (var item in list2.Values)
                    {
                        var item2 = objItem.Fields.Find(b => b.Name == item.MemberName);
                        if (item2 != null)
                        {
                            item.Remark = item2.Remark;
                        }
                    }
                }
                else
                {
                    objItems.Remove(typeName);
                }
                table.Fields = list2.Values.ToList();
                result.Add(type, table);
            }
            return result;
        }

        public static string ExportToFile(Type[] currentTypes)
        {
            List<string> xmlFiles;
            var a = GetInfoFromDll(currentTypes, out xmlFiles);
            var b = GetInfoFromXml(xmlFiles);
            var c = Merge(a, b);
            StringBuilder sb = new StringBuilder("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>");
            int i = 0;
            var count = c.Count;
            foreach (var kv in c)
            {
                i += 1;
                var numOf = string.Format("{0}/{1}", i, count);
                sb.Append(numOf);
                var tableName = CRL.TypeCache.GetTableName(kv.Key, null);
                sb.Append(@"<table border='1' cellpadding='4' cellspacing='1' style='width:100%'>
  <tr>
    <td colspan='5'><b>" + kv.Key.FullName + "[" + tableName + "]</b>(" + kv.Value.Remark+ ")</td></tr><tr><th>名称</th><th>类型</th><th>长度</th><th>索引</th><th>备注</th></tr>");
                var list = kv.Value.Fields;
                foreach (var p in list)
                {
                    var lengthStr = "";
                    if (p.PropertyType == typeof(string) || p.PropertyType == typeof(System.Byte[]))
                    {
                        lengthStr = p.Length.ToString();
                    }
                    string remark = p.Remark;
                    if (p.PropertyType.BaseType == typeof(Enum))
                    {
                        var enumValues = Enum.GetValues(p.PropertyType);
                        string enumStr = "";
                        foreach (var enu in enumValues)
                        {
                            enumStr += string.Format("{0}={1},",enu,Convert.ToInt32(enu));
                        }
                        if (enumStr.Length > 1)
                        {
                            enumStr = enumStr.Substring(0, enumStr.Length-1);
                        }
                        remark += p.MemberName + "[" + enumStr + "]";
                    }
                    //if (p.FieldType == Attribute.FieldType.虚拟字段)
                    //{
                    //    remark += string.Format("[as {0}]", System.Text.RegularExpressions.Regex.Replace(p.VirtualField, @"\{.+?\}", ""));
                    //}
                    sb.Append(@"<tr><td width='250'>" + p.MemberName + "</td><td  width='280'>" + p.PropertyType + "</td><td  width='40'>" + lengthStr + "</td><td  width='80'>" + p.FieldIndexType + "</td><td>" + remark + "</td></tr>");
                }
                sb.Append("</table><br>");
            }
            //string saveFile = System.Web.Hosting.HostingEnvironment.MapPath(string.Format("/model_{0}.html", currentTypes[0].Assembly.ManifestModule.Name));
            //System.IO.File.WriteAllText(saveFile, sb.ToString());
            //System.Diagnostics.Process.Start(saveFile);
            return sb.ToString(); 
        }
    }
}
