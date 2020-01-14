using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace CRL.Core.Session
{
    public class WebSession : AbsSession
    {
        public WebSession(IWebContext _context) : base(_context)
        {
          
        }

        public override void Clean()
        {
            context.Clear();
        }

        public override T Get<T>(string name)
        {
            var obj = context.GetSession(name);
            return (T)obj;
        }

        public override void Refresh()
        {
            
        }

        public override void Remove(string name)
        {
            context.RemoveSession(name);
        }

        public override void Set(string name, object value)
        {
            context.SetSession(name, value);
        }
    }
}
