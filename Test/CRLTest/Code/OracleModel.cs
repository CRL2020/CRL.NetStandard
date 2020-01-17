using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRLTest.Code
{
    public class OracleModel : CRL.IModelBase
    {
        public string OrderId
        {
            get;
            set;
        }
        public int Numbrer
        {
            get;
            set;
        }
        protected override IList GetInitData()
        {
            var list = new List<OracleModel>();
            list.Add(new OracleModel() { Numbrer = 1, OrderId = "ttt" });
            list.Add(new OracleModel() { Numbrer = 2, OrderId = "ttt222" });
            list.Add(new OracleModel() { Numbrer = 3, OrderId = "ttt333" });
            return list;
        }
    }
    public class OracleModelManage : CRL.BaseProvider<OracleModel>
    {
        public static OracleModelManage Instance
        {
            get { return new OracleModelManage(); }
        }
        public void Test()
        {
            var list2 = GetLambdaQuery().Where(b=>b.OrderId.Contains("1")).Top(10).ToList();
            Add(new OracleModel() { Numbrer = DateTime.Now.Second, OrderId = "ttt" });
            var list = GetLambdaQuery().Where(b => b.Id > 1).Page(1,1).ToList();
            Console.WriteLine(list.Count);
        }
    }
}
